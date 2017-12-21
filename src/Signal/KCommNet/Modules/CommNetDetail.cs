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
        CommNode node = vessel.connection.Comm;
        if(node!=null)
        {
          AntennaInfo antennaInfo = new AntennaInfo(node);
          GetPower = KSPUtil.PrintSI(antennaInfo.AntennaRange, string.Empty, 3, false);
        }


        //CommPath cPath = new CommPath();
        //if (KERBALISM.Kerbalism.KCommNet.FindClosestControlSource(antennaInfo., cPath))
        //{
        //  // NEED FIX: vessel.connection.ControlPath.First.b.antennaRelay.power needs to be replaced, it is a cache value.
        //  //maxDistHop = KSPUtil.PrintSI(Math.Sqrt(antennaInfo.AntennaRange * cPath.First.b.antennaRelay.power), string.Empty, 3, false);
        //  Events["maxDistHop"].guiActive = true;
        //}
        else Events["maxDistHop"].guiActive = false;
      }
    }
  }
}
