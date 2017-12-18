using CommNet;
using System;
using System.Collections.Generic;

namespace KCOMMNET
{
  public sealed class AntennaInfo
  {
    public AntennaInfo(CommNode node)
    {
      double mainAntenna = 0;                                                                                     //  Strongest Antenna
      double AntennaSum = 0;                                                                                      //  Sum of all Antennas
      double AntennaComb = 0;                                                                                     //  Sum of all Antennas Combinability
      double AntennaCombExponent = 0;                                                                             //  Average Weighted Combinability Exponent

      double mainRelay = 0;                                                                                       //  Strongest Relay
      double RelaySum = 0;                                                                                        //  Sum of all Relay
      double RelayComb = 0;                                                                                       //  Sum of all Relay Combinability
      double RelayCombExponent = 0;                                                                               //  Average Weighted Combinability Exponent

      bool extended = false;                                                                                      //  Fixed and deployable antennas
      bool allAntennaEqual = true;                                                                                //  If all antennas is Equal, antennaCombinableExponent = 1
      bool allRelayEqual = true;                                                                                  //  If all relay is Equal, antennaCombinableExponent = 1

      string antennaT = null;                                                                                     //  Only one temporary antenna for compare
      string relayT = null;                                                                                       //  Only one temporary relay for compare

      double AntennaXModified;                                                                                    //  (AntennaPower || RelayPower) * rangeModifier

      //  Kerbalism
      //  initialize data
      type = new List<AntennaType>();
      ecCost = new List<double>();
      rate = new List<double>();
      antennaPower = new List<double>();
      relay = new List<bool>();
      no_antenna = true;

      if (node.isHome)
      {
        RelayRange = node.antennaRelay.power;
      }
      else
      {
        Vessel v = KERBALISM.Lib.KCOMMNET.CommNodeToVessel(node);
        if (v != null)
        {
          if (v.loaded)
          {
            foreach (ModuleDataTransmitter antenna in v.FindPartModulesImplementing<ModuleDataTransmitter>())
            {
              ModuleDeployableAntenna anim = antenna.part.FindModuleImplementing<ModuleDeployableAntenna>();

              //  Assume extended if there is no animator or If statys is Extended
              if (anim != null)
              {
                extended = anim.deployState == ModuleDeployablePart.DeployState.EXTENDED;
              }
              else extended = true;

              //  Calc range only for Antennas that is working
              if (extended || !KERBALISM.Settings.ExtendedAntenna)
              {
                NetworkAdaptor ant = antenna.part.FindModuleImplementing<NetworkAdaptor>();
                if (ant != null)
                {
                  AntennaRate += ant.rate;
                }

                ListAntenna.Add(antenna);

                //  Have to confirm, I put to arround up
                //  Antenna Power X CommNet rangeModifier
                AntennaXModified = Math.Round(antenna.antennaPower * rangeModifier, 1);

                //  Strongest antenna
                mainAntenna = (mainAntenna < AntennaXModified ? AntennaXModified : mainAntenna);

                //  Total Antenna Power
                AntennaSum += AntennaXModified;

                //  Set first value
                if (antennaT == null) antennaT = antenna.name;

                allAntennaEqual = (allAntennaEqual && antennaT == antenna.name);

                if (antenna.antennaType == AntennaType.RELAY)
                {
                  if (relayT == null) relayT = antenna.name;
                  allRelayEqual = (allRelayEqual && relayT == antenna.name);
                }
              }
            }
          }
          else
          {
            foreach (ProtoPartSnapshot p in v.protoVessel.protoPartSnapshots)
            {
              //  get part prefab (required for module properties)
              Part part_prefab = PartLoader.getPartInfoByName(p.partName).partPrefab;

              //  get module prefab
              ModuleDataTransmitter antenna = part_prefab.FindModuleImplementing<ModuleDataTransmitter>();

              //  if there is none, skip the part
              if (antenna == null) continue;

              //  for each module
              foreach (ProtoPartModuleSnapshot m in p.modules)
              {
                //  we are only interested in antennas
                if (m.moduleName != "ModuleDataTransmitter") continue;

                //  if the module is disabled, skip it
                if (!KERBALISM.Lib.Proto.GetBool(m, "isEnabled")) continue;

                ProtoPartModuleSnapshot deployState = FlightGlobals.FindProtoPartByID(p.flightID).FindModule("ModuleDeployableAntenna");
                ProtoPartModuleSnapshot netAdaptor = FlightGlobals.FindProtoPartByID(p.flightID).FindModule("NetworkAdaptor");

                if (deployState != null)
                {
                  extended = KERBALISM.Lib.Proto.GetString(deployState, "deployState") == "EXTENDED";
                }
                else extended = true;

                //  Calc range only for Antennas that is working
                if (extended || !KERBALISM.Settings.ExtendedAntenna)
                {
                  if (netAdaptor != null) AntennaRate += KERBALISM.Lib.Proto.GetDouble(netAdaptor, "rate");

                  ListAntenna.Add(antenna);

                  //  Have to confirm, I put to arround up
                  //  Antenna Power X CommNet rangeModifier
                  AntennaXModified = Math.Round(antenna.antennaPower * rangeModifier, 1);

                  //  Strongest antenna
                  mainAntenna = (mainAntenna < AntennaXModified ? AntennaXModified : mainAntenna);

                  //  Total Antenna Power
                  AntennaSum += AntennaXModified;

                  //  Set first value
                  if (antennaT == null) antennaT = antenna.name;

                  allAntennaEqual = (allAntennaEqual && antennaT == antenna.name);

                  if (antenna.antennaType == AntennaType.RELAY)
                  {
                    if (relayT == null) relayT = antenna.name;
                    allRelayEqual = (allRelayEqual && relayT == antenna.name);
                  }
                }
              }
            }
          }

          foreach (ModuleDataTransmitter antenna in ListAntenna)
          {
            //  Have to confirm, I put to arround up
            //  Antenna Power X CommNet rangeModifier
            AntennaXModified = Math.Round(antenna.antennaPower * rangeModifier, 1);

            //  Strongest antenna
            mainAntenna = (mainAntenna < AntennaXModified ? AntennaXModified : mainAntenna);

            //  Total Antenna Power
            AntennaSum += AntennaXModified;

            //  Average Weighted Combinability
            //  If all antennas are Equal, CombinableExponent = 1, I guess this is the right because only this way I can have the same result to signal
            if (antenna.antennaCombinable) AntennaComb += Math.Round(AntennaXModified * ((allAntennaEqual) ? 1 : antenna.antennaCombinableExponent), 1);

            //  If connected through Relay, accept only AntennaType = Relay to calc
            if (antenna.CommType == AntennaType.RELAY)
            {
              //  Strongest antenna
              mainRelay = (mainRelay < AntennaXModified ? AntennaXModified : mainRelay);

              //  Total Antenna Power
              RelaySum += AntennaXModified;

              //  Average Weighted Combinability
              if (antenna.CommCombinable) RelayComb += Math.Round(AntennaXModified * ((allRelayEqual) ? 1 : antenna.antennaCombinableExponent), 1);
            }
          }

          //  Average Weighted Combinability Exponent 
          AntennaCombExponent = AntennaComb / AntennaSum;
          RelayCombExponent = RelayComb / RelaySum;

          //  Formula source = https://wiki.kerbalspaceprogram.com/wiki/CommNet
          //  Vessel Antenna Power = Strongest Antenna Power * ( Sum of Antenna's Powers / Strongest Antenna Power ) ^ ( Average Weighted Combinability Exponent for Vessel )
          //  Vessel Antenna Power = mainAntenna * (AntennaComb/AntennaSum) ^ AntennaCombExponent;
          AntennaRange = Math.Ceiling(mainAntenna * Math.Pow((AntennaSum / mainAntenna), AntennaCombExponent));
          RelayRange = Math.Ceiling(mainRelay * Math.Pow((RelaySum / mainRelay), RelayCombExponent));
        }
      }
    }

    public double AntennaRange;
    public double RelayRange;
    public double MaxDistanceTarget;                                                                              //  Max distance to the Target
    public double AntennaRate;                                                                                    //  Kbps
    public List<ModuleDataTransmitter> ListAntenna = new List<ModuleDataTransmitter>();                           //  List of antennas

    public static float rangeModifier = HighLogic.fetch.currentGame.Parameters.CustomParams<CommNetParams>().rangeModifier;   //  CommNet Range Modifier

    //  Kerbalism
    List<AntennaType> type;
    List<double> ecCost;                                                                                           //  TODO: implement ecCost to NetworkAdaptor module.
    List<double> rate;
    List<double> antennaPower;
    List<bool> relay;

    public bool no_antenna;
  }
}
