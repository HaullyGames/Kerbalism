using System;
using System.Collections.Generic;
using UnityEngine;

namespace KERBALISM
{
  public static class Network
  {
    public static void network(this Panel p, Vessel v)
    {
      // avoid corner-case when this is called in a lambda after scene changes
      v = FlightGlobals.FindVessel(v.id);

      // if vessel doesn't exist anymore, leave the panel empty
      if (v == null) return;

      // get info from the cache
      vessel_info vi = Cache.VesselInfo(v);

      // if not a valid vessel, leave the panel empty
      if (!vi.is_valid) return;

      // set metadata
      p.title(Lib.BuildString(Lib.Ellipsis(v.vesselName, 20), " <color=#cccccc>NETWORK ADAPTOR</color>"));

      // time-out simulation
      if (p.timeout(vi)) return;

      render_netadaptors(p, v);
    }

    static void render_netadaptors(Panel p, Vessel v)
    {
      // get all sensor readings
      List<uint> readings = new List<uint>();

      if (v.loaded)
      {
        foreach (var s in Lib.FindModules<KCOMMNET.NetworkAdaptor>(v))
        {
          readings.Add(s.frequency);
        }
      }
      else
      {
        foreach (ProtoPartModuleSnapshot m in Lib.FindModules(v.protoVessel, "NetworkAdaptor"))
        {
          readings.Add(Lib.Proto.GetUInt(m, "frequency"));
        }
      }

      p.section("ADAPTORS");
      foreach (double frequency in readings)
      {
        p.selector("Adaptor", Sensor.netadaptor_content(v, frequency),"");
      }
    }
  }
}
