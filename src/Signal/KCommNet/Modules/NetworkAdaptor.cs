using System;
using KERBALISM;

namespace KCOMMNET
{
  // Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
  // CNConstellationAntennaModule
  public class NetworkAdaptor : PartModule
  {
    [KSPField(isPersistant = true)] public uint frequency = 0;
    [KSPField] public double ecCost;                            // cost of transmission in EC/s
    [KSPField] public double rate;                              // transmission rate at zero distance in Mb/s
    public Guid Target = Guid.Empty;

    [KSPField(guiName = "Bandwidth", guiUnits = "", guiActive = false, guiFormat = "")]
    string Rate;

    [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "Antenna Setup", active = false)]
    public void KSPEventAntennaConfig()
    {
      // NEED IMPLEMENT
    }

    // This module is always exist in part that has ModuleDataTransmitter
    ModuleDataTransmitter transmitter;

    public override void OnStart(StartState state)
    {
      if (state == StartState.Editor && state == StartState.None && state == StartState.PreLaunch) return;
      
      transmitter = part.FindModuleImplementing<ModuleDataTransmitter>();

      if (transmitter!=null)
      {
        // I hide it because if don't show the antenna power X rangeModifier
        transmitter.Fields["powerText"].guiActive = false;
        transmitter.Fields["statusText"].guiActive = false;
        transmitter.Events["StartTransmission"].active = false;
        transmitter.Events["TransmitIncompleteToggle"].active = false;

        // Show transmissiter rate
        if (transmitter.antennaType != global::AntennaType.INTERNAL)
        {
          Rate = Lib.HumanReadableDataRate(rate);
          Fields["Rate"].guiActive = true;
        }
      }
      base.OnStart(state);
    }
  }
}
