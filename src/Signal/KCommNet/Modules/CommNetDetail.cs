﻿using UnityEngine;
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
        AntennaInfo antennaInfo = new AntennaInfo(vessel.connection.Comm);
        GetPower = KSPUtil.PrintSI(antennaInfo.AntennaRange, string.Empty, 3, false);

        if (vessel.connection != null)
        {
          if (vessel.connection.Comm.Net.FindHome(vessel.connection.Comm))
          {
            // NEED FIX: vessel.connection.ControlPath.First.b.antennaRelay.power needs to be replaced, it is a cache value.
            maxDistHop = KSPUtil.PrintSI(Math.Sqrt(antennaInfo.AntennaRange * vessel.connection.ControlPath.First.b.antennaRelay.power), string.Empty, 3, false);
            Events["maxDistHop"].guiActive = true;
          }
          else Events["maxDistHop"].guiActive = false;
        }
      }
    }
  }
}
