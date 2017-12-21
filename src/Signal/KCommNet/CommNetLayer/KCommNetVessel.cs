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

    public void UpdateControl(bool doIt)
    {
      if (doIt) this.unloadedDoOnce = this.doUnloadedUpdate = true;
      else this.doUnloadedUpdate = false;
    }
  }
}
