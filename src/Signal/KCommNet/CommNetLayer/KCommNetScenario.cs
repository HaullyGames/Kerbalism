using CommNet;
using KERBALISM;
using System.Collections.Generic;

// Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
namespace KCOMMNET
{
  // This class is the key that allows to break into and customise KSP's CommNet. This is possibly the secondary model in the Model–view–controller sense
  [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR })]
  public class KCommNetScenario : CommNetScenario
  {
    // Note:
    //   1) On entering a desired scene, OnLoad() and then Start() are called.
    //   2) On leaving the scene, OnSave() is called
    //   3) GameScenes.SPACECENTER is recommended so that the constellation data can be verified and error-corrected in advance

    //TODO: investigate to add extra ground stations to the group of existing stations
    KCommNetUI CustomCommNetUI = null;

    List<KCommNetVessel> commVessels;
    bool dirtyCommNetVesselList;

    public static new KCommNetScenario Instance { get; protected set; }

    protected override void Start()
    {
      Instance = this;
      commVessels = new List<KCommNetVessel>();
      dirtyCommNetVesselList = true;

      Lib.Log("CommNet Scenario loading ...");

      //Replace the CommNet user interface
      CommNetUI ui = FindObjectOfType<CommNetUI>();
      CustomCommNetUI = gameObject.AddComponent<KCommNetUI>();  // gameObject.AddComponent<>() is "new" keyword for Monohebaviour class
      Destroy(ui);

    }
  }
}
