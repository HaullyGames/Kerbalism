using CommNet;
using KERBALISM;

// Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
namespace KCOMMNET
{
  // Copy required for the customised CommNet
  public class KCommNetBody : CommNetBody
  {
    public void copyOf(CommNetBody stockBody)
    {
      Lib.Log("CommNet Body "+ stockBody.name+" added");

      body = stockBody.GetComponentInChildren<CelestialBody>();
    }
  }
}
