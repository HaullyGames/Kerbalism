using CommNet;
using KERBALISM;

namespace KCOMMNET
{
  // Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
  public class KCommNetNetwork : CommNetNetwork
  {
    //  Extend the functionality of the KSP's CommNetNetwork (co-primary model in the Model–view–controller sense; CommNet<> is the other co-primary one)
    public static new KCommNetNetwork Instance { get; protected set; }

    protected override void Awake()
    {
      Lib.Log("CommNet Network booting");

      CommNetNetwork.Instance = this;
      CommNet = new KCommNetwork();

      if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
      {
        GameEvents.onPlanetariumTargetChanged.Add(new EventData<MapObject>.OnEvent(OnMapFocusChange));
      }

      GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(ResetNetwork));
      ResetNetwork(); // Please retain this so that KSP can properly reset
    }

    protected new void ResetNetwork()
    {
      Lib.Log("CommNet Network rebooted");

      CommNet = new KCommNetwork();
      GameEvents.CommNet.OnNetworkInitialized.Fire();
    }
  }
}
