using System;
using System.Collections.Generic;
using CommNet;
using KERBALISM;

// Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
namespace KCOMMNET
{
  public class KCommNetVessel : CommNetVessel //, IPersistenceSave, IPersistenceLoad
  {
    public enum FrequencyListOperation
    {
      AutoBuild,
      LockList
    };

    protected Dictionary<short, double> FrequencyDict             = new Dictionary<short, double>();
    [Persistent] private List<short> FreqDictionaryKeys           = new List<short>();
    [Persistent] private List<double> FreqDictionaryValues        = new List<double>();
    [Persistent] public FrequencyListOperation FreqListOperation  = FrequencyListOperation.AutoBuild;

    protected short strongestFreq = -1;
    protected short freqHighestRate = -1;
    protected List<NetworkAdaptor> vNetAdaptors = new List<NetworkAdaptor>();
    protected bool stageActivated = false;

    // Retrieve the CNC data from the vessel
    //protected override void OnNetworkInitialized()
    //{
    //  base.OnNetworkInitialized();
    //  try
    //  {
    //    // NEED IMPLEMENT

    //    GameEvents.onStageActivate.Add(stageActivate);
    //  }
    //  catch (Exception e)
    //  {
    //    Lib.Log("Vessel " + this.Vessel.GetName() + " doesn't have any CommNet capability, likely a mislabelled junk or a kerbin on EVA");
    //    Lib.Log("Error Message: " + e.Message);
    //  }
    //}

    //public override void OnNetworkPreUpdate()
    //{
    //  if (vessel.loaded  || !vessel.loaded || overridePreUpdate)
    //  {
    //    UpdateComm();
    //    CalculatePlasmaMult();
    //  }
    //  else
    //    inPlasma = false;
    //  comm.precisePosition = vessel.GetWorldPos3D();
    //}

    protected override void UpdateComm()
    {
      base.UpdateComm();
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();
      if (HighLogic.CurrentGame == null) return;

      GameEvents.onStageActivate.Remove(stageActivate);
      GameEvents.onVesselWasModified.Remove(vesselModified);
      // NEED IMPLEMENT
    }

    // GameEvent of staging a vessel
    private void stageActivate(int stageIndex)
    {
      if (this.Vessel.isActiveVessel)
      {
        this.stageActivated = true;
      }
    }

    // GameEvent of vessel being modified: Decouple event
    private void vesselModified(Vessel thisVessel)
    {
      if (this.Vessel.isActiveVessel && this.stageActivated)
      {
        Lib.Log("Active CommNet Vessel " + this.Vessel.vesselName + " is staged. Rebuilding the freq list on suriving antennas...");

        // NEED IMPLEMENT

        this.stageActivated = false;
      }
    }
  }
}
