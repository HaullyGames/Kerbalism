using System;
using System.Collections.Generic;
using CommNet;
using KERBALISM;

namespace KCOMMNET
{
  public class KCommNetwork : CommNetwork
  {
    private short publicFreq = 0;

    //  Edit the connectivity between two potential nodes
    //protected override bool SetNodeConnection(CommNode a, CommNode b)
    //{
    //  //stop links between ground stations
    //  if (a.isHome && b.isHome)
    //  {
    //    Disconnect(a, b, true);
    //    return false;
    //  }

    //  List<short> aFreqs, bFreqs;

    //  //each CommNode has at least some frequencies?
    //  try
    //  {
    //    aFreqs = KCommNetScenario.Instance.getFrequencies(a);
    //    bFreqs = KCommNetScenario.Instance.getFrequencies(b);
    //  }
    //  catch (NullReferenceException e) // either CommNode could be a kerbal on EVA
    //  {
    //    Lib.Log("Error on KCommNetwork: "+ e.Message);
    //    Lib.Log("Maybe CommNode is Kerbal");
    //    Disconnect(a, b, true);
    //    return false;
    //  }

    //  //share same frequency?
    //  for (int i = 0; i < aFreqs.Count; i++)
    //  {
    //    if (bFreqs.Contains(aFreqs[i]))         // yes, it does
    //    {
    //      return base.SetNodeConnection(a, b);
    //    }
    //  }

    //  this.Disconnect(a, b, true);
    //  return false;
    //}
  }
}
