using System;
using System.Collections.Generic;

namespace KCOMMNET
{
  public class NetworkAdaptor : PartModule
  {
    [KSPField] public uint vlan = 0;                            // What is the lan that this will communicate
    [KSPField] public double ecCost;                            // cost of transmission in EC/s
    [KSPField] public double rate;                              // transmission rate at zero distance in Mb/s
    public Guid Target;

    [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "Network Adaptor Setup", active = true)]
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
