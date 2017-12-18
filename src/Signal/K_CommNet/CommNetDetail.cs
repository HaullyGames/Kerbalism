using UnityEngine;
using System;
using KERBALISM;

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
      if (Lib.IsFlight())
      {
        if(vessel.connection.IsConnected)
        {
          AntennaInfo antennaInfo = new AntennaInfo(vessel.connection.Comm);
          GetPower = KSPUtil.PrintSI(antennaInfo.AntennaRange, string.Empty, 3, false);

          maxDistHop = KSPUtil.PrintSI(Math.Sqrt(antennaInfo.AntennaRange * vessel.connection.ControlPath.First.b.antennaRelay.power), string.Empty, 3, false); // NEED FIX: vessel.connection.ControlPath.First.b.antennaRelay.power needs to be replaced 
          Events["maxDistHop"].guiActive = true;
        }
        else Events["maxDistHop"].guiActive = false;
      }
    }
  }
}
