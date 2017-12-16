using System;

namespace KERBALISM
{
  public class K_ConnectionsInfo : PartModule
  {
    [KSPField(guiName = "Antenna Power", guiActive = false, guiFormat = "")]
    public string GetPower;

    [KSPField(guiName = "Max First Hop Dist", guiActive = false, guiFormat = "")]
    public string maxDistanceHop;

    public void Update()
    {
      GetPower = KSPUtil.PrintSI(part.vessel.connection.Comm.antennaTransmit.power, string.Empty, 3, false);

      if (part.vessel.connection.IsConnected)
      {
        maxDistanceHop = KSPUtil.PrintSI(Math.Sqrt(part.vessel.connection.Comm.antennaTransmit.power * part.vessel.connection.ControlPath.First.b.antennaRelay.power), string.Empty, 3, false);
        Events["maxDistanceHop"].guiActive = true;
      }
      else Events["maxDistanceHop"].guiActive = false;
    }
  }
}
