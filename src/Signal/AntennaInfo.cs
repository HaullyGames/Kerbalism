using System;
using System.Collections.Generic;
using UnityEngine;


namespace KERBALISM {

public sealed class AntennaInfo
{
  public AntennaInfo(Vessel v)
  {
    // initialize data
    type = new List<AntennaType>();
    cost = new List<double>();
    rate = new List<double>();
    dist = new List<double>();
    relay = new List<bool>();
    no_antenna = true;

    // get ec available
    // - this is the amount available at previous simulation step
    bool ec_available = ResourceCache.Info(v, "ElectricCharge").amount > double.Epsilon;

    // if the vessel is loaded
    if (v.loaded)
    {
      if (Features.KCommNet)
      {
        foreach (ModuleDataTransmitter a in Lib.FindModules<ModuleDataTransmitter>(v))
        {
          if (a.CanComm())
          {
            // Try get Trasmitter(CommNet)
            KCOMMNET.NetworkAdaptor transmitter = a.part.FindModuleImplementing<KCOMMNET.NetworkAdaptor>();
            if (transmitter != null)
            {
              ModuleDeployableAntenna anim = transmitter.part.FindModuleImplementing<ModuleDeployableAntenna>();
              //	Assume extended if there is no animator or If statys is Extended
              bool extended = false;
              if (anim == null) extended = true;
              else if (anim.deployState == ModuleDeployablePart.DeployState.EXTENDED) extended = true;

              if (!Settings.ExtendedAntenna && !extended) continue;

              bool antenna_is_relay = a.antennaType == global::AntennaType.RELAY;

              type.Add(antenna_is_relay ? AntennaType.low_gain : AntennaType.high_gain);
              cost.Add(transmitter.ecCost);
              rate.Add(transmitter.rate);
              relay.Add(antenna_is_relay);
              is_relay |= antenna_is_relay;
              direct_cost += transmitter.ecCost;
              if (antenna_is_relay) indirect_cost += transmitter.ecCost;
            }
          }
          no_antenna = false;
        }
      }
      else
      {
        // get all antennas data
        foreach (Antenna a in Lib.FindModules<Antenna>(v))
        {
          if (!Settings.ExtendedAntenna || a.extended || Features.KCommNet)
          {
            type.Add(a.type);
            cost.Add(a.cost);
            rate.Add(a.rate);
            dist.Add(a.dist);
            relay.Add(ec_available && a.relay);
            is_relay |= ec_available && a.relay;
            direct_cost += a.cost;
            if (a.type == AntennaType.low_gain) indirect_cost += a.cost;
          }
          no_antenna = false;
        }
      }
    }
    // if the vessel isn't loaded
    // - we don't support multiple antenna modules per-part
    else
    {
      if(Features.KCommNet)
      {
        // for each part
        foreach (ProtoPartSnapshot p in v.protoVessel.protoPartSnapshots)
        {
          // get part prefab (required for module properties)
          Part part_prefab = PartLoader.getPartInfoByName(p.partName).partPrefab;

          // get module prefab
          KCOMMNET.NetworkAdaptor a = part_prefab.FindModuleImplementing<KCOMMNET.NetworkAdaptor>();

          // if there is none, skip the part
          if (a == null) continue;

          // for each module
          foreach (ProtoPartModuleSnapshot m in p.modules)
          {
            // we are only interested in antennas
            if (m.moduleName != "NetworkAdaptor") continue;

            // if the module is disabled, skip it
            if (!Lib.Proto.GetBool(m, "isEnabled")) continue;

            ProtoPartModuleSnapshot deployState = FlightGlobals.FindProtoPartByID(p.flightID).FindModule("ModuleDeployableAntenna");
            ProtoPartModuleSnapshot transmitter = FlightGlobals.FindProtoPartByID(p.flightID).FindModule("ModuleDataTransmitter");

            if (deployState != null)
            {
              // If it has animation, need be equal EXTENDED to work
              if (Lib.Proto.GetString(deployState, "deployState") != "EXTENDED" && !Settings.ExtendedAntenna) continue;
            }

            if (!Lib.Proto.GetBool(transmitter, "canComm")) continue;

            bool antenna_is_relay = Lib.Proto.GetString(transmitter, "antennaType") == "RELAY";

            type.Add(antenna_is_relay ? AntennaType.low_gain : AntennaType.high_gain);
            cost.Add(a.ecCost);
            rate.Add(a.rate);
            relay.Add(antenna_is_relay);
            is_relay |= antenna_is_relay;
            direct_cost += a.ecCost;
            if (antenna_is_relay) indirect_cost += a.ecCost;
          }
          no_antenna = false;
        }
      }
      else
      { 
        // for each part
        foreach (ProtoPartSnapshot p in v.protoVessel.protoPartSnapshots)
        {
          // get part prefab (required for module properties)
          Part part_prefab = PartLoader.getPartInfoByName(p.partName).partPrefab;

          // get module prefab
          Antenna a = part_prefab.FindModuleImplementing<Antenna>();

          // if there is none, skip the part
          if (a == null) continue;

          // for each module
          foreach(ProtoPartModuleSnapshot m in p.modules)
          {
            // we are only interested in antennas
            if (m.moduleName != "Antenna") continue;

            // if the module is disabled, skip it
            if (!Lib.Proto.GetBool(m, "isEnabled")) continue;

            // get antenna data
            if (!Settings.ExtendedAntenna || Lib.Proto.GetBool(m, "extended"))
            {
              bool antenna_is_relay = Lib.Proto.GetBool(m, "relay");
              type.Add(a.type);
              cost.Add(a.cost);
              rate.Add(a.rate);
              dist.Add(a.dist);
              relay.Add(ec_available && antenna_is_relay);
              is_relay |= ec_available && antenna_is_relay;
              direct_cost += a.cost;
              if (a.type == AntennaType.low_gain) indirect_cost += a.cost;
            }
            no_antenna = false;
          }
        }
      }
    }
  }

  public double direct_rate(double d)
  {
    double r = 0.0;
    for(int i=0; i < type.Count; ++i)
    {
      r += Antenna.calculate_rate(d, dist[i], rate[i]);
    }
    return r;
  }

  public double direct_rate_KCommNet(double signalStrong)
  {
    double r = 0.0;
    for (int i = 0; i < type.Count; ++i)
    {
      r += rate[i] * signalStrong;
    }
    return r;
  }

  public double indirect_rate(double d, AntennaInfo relay_antenna)
  {
    double r = 0.0;
    for(int i=0; i < type.Count; ++i)
    {
      if (type[i] == AntennaType.low_gain)
      {
        r += Antenna.calculate_rate(d, dist[i], rate[i]);
      }
    }

    double indirect_r = 0.0;
    for(int i=0; i < relay_antenna.type.Count; ++i)
    {
      if (relay_antenna.type[i] == AntennaType.low_gain && relay_antenna.relay[i])
      {
        indirect_r += Antenna.calculate_rate(d, relay_antenna.dist[i], relay_antenna.rate[i]);
      }
    }

    return Math.Min(r, indirect_r);
  }

  public double indirect_rate_KCommNet(double ControlSignalStrength, AntennaInfo relay_antenna, double RelaySignal)
  {
    double r = 0.0;
    for (int i = 0; i < type.Count; ++i)
    {
      r += rate[i] * ControlSignalStrength;
    }

    double indirect_r = 0.0;
    for (int i = 0; i < relay_antenna.type.Count; ++i)
    {
      if (relay_antenna.type[i] == AntennaType.low_gain)
      {
        indirect_r += RelaySignal * relay_antenna.rate[i];
      }
    }
    return Math.Min(r, indirect_r);
  }

  public double relay_rate(double d)
  {
    double r = 0.0;
    for(int i=0; i < type.Count; ++i)
    {
      if (type[i] == AntennaType.low_gain && relay[i])
      {
        r += Antenna.calculate_rate(d, dist[i], rate[i]);
      }
    }
    return r;
  }

  List<AntennaType> type;
  List<double> cost;
  List<double> rate;
  List<double> dist;
  List<bool> relay;

  public bool no_antenna;
  public bool is_relay;
  public double direct_cost;
  public double indirect_cost;
}


} // KERBALISM

