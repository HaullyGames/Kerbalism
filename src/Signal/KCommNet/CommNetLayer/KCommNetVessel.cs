using System;
using System.Collections.Generic;
using CommNet;
using KERBALISM;

// Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
namespace KCOMMNET
{
  public class KCommNetVessel : CommNetVessel, IPersistenceSave, IPersistenceLoad
  {
    public enum FrequencyListOperation
    {
      AutoBuild,
      LockList
    };

    protected Dictionary<short, double> FrequencyDict = new Dictionary<short, double>();
    [Persistent] private List<short> FreqDictionaryKeys = new List<short>();
    [Persistent] private List<double> FreqDictionaryValues = new List<double>();
    [Persistent] public FrequencyListOperation FreqListOperation = FrequencyListOperation.AutoBuild;

    // Player will be able to define what is the best connection ( Strongest_Signal Or Highest_Rate )
    protected short strongestFreq = -1;
    protected short freqHighestRate = -1;
    protected List<NetworkAdaptor> vNetAdaptors = new List<NetworkAdaptor>();
    protected bool stageActivated = false;

    // Testing, This method force CommNet update NetWork for all vessels
    public void UpdateControl(bool doIt)
    {
      if (doIt) unloadedDoOnce = doUnloadedUpdate = true;
      else doUnloadedUpdate = false;
    }

    // GameEvent of staging a vessel
    private void stageActivate(int stageIndex)
    {
      if (Vessel.isActiveVessel)
      {
        stageActivated = true;
      }
    }

    // GameEvent of vessel being modified
    private void vesselModified(Vessel thisVessel)
    {
      if (Vessel.isActiveVessel && stageActivated) // decouple event
      {
        Lib.Log("Active CommNet Vessel '" + this.Vessel.vesselName + "' is staged. Rebuilding the freq list on suriving antennas...");

        //force-rebuild freq list to stop players from abusing LockList
        //this.vesselAntennas = this.readAntennaData();
        //this.FrequencyDict = buildFrequencyList(this.vesselAntennas);
        //this.strongestFreq = computeStrongestFrequency(this.FrequencyDict);

        stageActivated = false;
      }
    }

    // Check if given vessel has CNConstellationModule and its attributes required, and if not, "upgrade" the vessel data
    public void validateAndUpgrade(Vessel thisVessel)
    {
      if (thisVessel == null)
        return;
      if (thisVessel.loaded)
        return;

      Lib.Log("Unloaded CommNet vessel '" + thisVessel.GetName() + "' is validated and upgraded");

      if (thisVessel.protoVessel != null)
      {
        List<ProtoPartSnapshot> parts = thisVessel.protoVessel.protoPartSnapshots;
        for (int i = 0; i < parts.Count; i++)
        {
          if (parts[i].FindModule("ModuleCommand") != null)
          {
            ProtoPartModuleSnapshot cncModule;
            if ((cncModule = parts[i].FindModule("CNConstellationModule")) == null) //check if CNConstellationModule is there
            {
              CNConstellationModule realcncModule = gameObject.AddComponent<CNConstellationModule>(); // don't use new keyword. PartModule is Monobehavior
              parts[i].modules.Add(new ProtoPartModuleSnapshot(realcncModule));

              CNCLog.Verbose("CNConstellationModule is added to CommNet Vessel '{0}'", thisVessel.GetName());
            }
          }
          if (parts[i].FindModule("ModuleDataTransmitter") != null)
          {
            ProtoPartModuleSnapshot cncModule;
            if ((cncModule = parts[i].FindModule("CNConstellationAntennaModule")) == null) //check if CNConstellationAntennaModule is there
            {
              CNConstellationAntennaModule realcncModule = gameObject.AddComponent<CNConstellationAntennaModule>(); // don't use new keyword. PartModule is Monobehavior
              parts[i].modules.Add(new ProtoPartModuleSnapshot(realcncModule));

              CNCLog.Verbose("CNConstellationAntennaModule is added to CommNet Vessel '{0}'", thisVessel.GetName());
            }
            else //check if all attributes are or should not be there
            {
              if (!cncModule.moduleValues.HasValue("CosAngle"))
                cncModule.moduleValues.AddValue("CosAngle", -1.0);
            }
          }
        } // end of part loop
      }
    }

    protected override void OnSave(ConfigNode gameNode)
    {
      base.OnSave(gameNode);

      if (gameNode.HasNode(GetType().FullName))
        gameNode.RemoveNode(GetType().FullName);

      gameNode.AddNode(ConfigNode.CreateConfigFromObject(this));
    }

    protected override void OnLoad(ConfigNode gameNode)
    {
      base.OnLoad(gameNode);

      if (gameNode.HasNode(GetType().FullName))
        ConfigNode.LoadObjectFromConfig(this, gameNode.GetNode(GetType().FullName));
    }

    public void PersistenceSave()
    {
      List<short> keys = new List<short>();
      var keyItr = FrequencyDict.Keys.GetEnumerator();
      while (keyItr.MoveNext())
      {
        keys.Add(keyItr.Current);
      }
      FreqDictionaryKeys = keys;

      List<double> values = new List<double>();
      var valueItr = FrequencyDict.Values.GetEnumerator();
      while (valueItr.MoveNext())
      {
        values.Add(valueItr.Current);
      }
      FreqDictionaryValues = values;
    }

    public void PersistenceLoad()
    {
      FrequencyDict = new Dictionary<short, double>();
      for (int i = 0; i < FreqDictionaryKeys.Count; i++)
      {
        FrequencyDict.Add(FreqDictionaryKeys[i], FreqDictionaryValues[i]);
      }
    }
  }
}
