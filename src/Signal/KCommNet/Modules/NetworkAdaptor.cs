using System;
using System.Collections.Generic;

namespace KCOMMNET
{
  // Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
  // CNConstellationAntennaModule
  public class NetworkAdaptor : PartModule
  {
    [KSPField(isPersistant = true)] public uint frequency;
    [KSPField] public double ecCost;                            // cost of transmission in EC/s
    [KSPField] public double rate;                              // transmission rate at zero distance in Mb/s
    public Guid Target = Guid.Empty;

    [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "Antenna Setup", active = true)]
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

      // I hide it because if don't show the antenna power X rangeModifier
      transmitter.Fields["powerText"].guiActive = false;
      base.OnStart(state);
    }
  }
}
