using System;
using System.Collections.Generic;
using CommNet;
using KERBALISM;

// Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
namespace KCOMMNET
{
  public class K_CommNetVessel : CommNetVessel //, IPersistenceSave, IPersistenceLoad
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
    protected override void OnNetworkInitialized()
    {
      base.OnNetworkInitialized();
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();
    }

    // GameEvent of staging a vessel
    private void stageActivate(int stageIndex)
    {
      if (this.Vessel.isActiveVessel)
      {
        this.stageActivated = true;
      }
    }

    //public void PersistenceSave()
    //{
    //  List<short> keys = new List<short>();
    //  var keyItr = FrequencyDict.Keys.GetEnumerator();
    //  while (keyItr.MoveNext())
    //  {
    //    keys.Add(keyItr.Current);
    //  }
    //  FreqDictionaryKeys = keys;

    //  List<double> values = new List<double>();
    //  var valueItr = FrequencyDict.Values.GetEnumerator();
    //  while (valueItr.MoveNext())
    //  {
    //    values.Add(valueItr.Current);
    //  }
    //  FreqDictionaryValues = values;
    //}

    //public void PersistenceLoad()
    //{
    //  FrequencyDict = new Dictionary<short, double>();
    //  for (int i = 0; i < FreqDictionaryKeys.Count; i++)
    //  {
    //    FrequencyDict.Add(FreqDictionaryKeys[i], FreqDictionaryValues[i]);
    //  }
    //}
  }
}
