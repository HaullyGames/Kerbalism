using UnityEngine;
using System;
using CommNet;

namespace KCOMMNET
{
  public class CommNetDetail : PartModule
  {
    [KSPField(guiName = "Vessel Antenna Power", guiActive = true, guiFormat = "")]
    public string GetPower = "";

    [KSPField(guiName = "Max Dist First Hop", guiActive = true, guiFormat = "")]
    public string maxDistHop = "";

    public void Update()
    {
      if (KERBALISM.Lib.IsFlight())
      {
        if (vessel == FlightGlobals.ActiveVessel)
        {
          CommNode node = vessel.connection.Comm;
          if (node != null)
          {
            AntennaInfo antennaInfo = new AntennaInfo(node);
            if (antennaInfo != null)
            {
              GetPower = KSPUtil.PrintSI(antennaInfo.AntennaRange, string.Empty, 3, false);

              if (vessel.connection.IsConnected)
              {
                // NEED FIX: vessel.connection.ControlPath.First.b.antennaRelay.power needs to be replaced, it is a cache value.
                maxDistHop = KSPUtil.PrintSI(Math.Sqrt(antennaInfo.AntennaRange * vessel.connection.ControlPath.First.end.antennaRelay.power), string.Empty, 3, false);
              }
              else maxDistHop = "";
            }
          }
        }
      }
    }
  }
}
