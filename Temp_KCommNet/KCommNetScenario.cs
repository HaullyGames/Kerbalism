using CommNet;
using KERBALISM;
using System.Collections.Generic;

// Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
namespace KCOMMNET
{
  // This class is the key that allows to break into and customise KSP's CommNet. This is possibly the secondary model in the Model–view–controller sense
  //[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR })]
  public class KCommNetScenario : CommNetScenario
  {
    // Note:
    //   1) On entering a desired scene, OnLoad() and then Start() are called.
    //   2) On leaving the scene, OnSave() is called
    //   3) GameScenes.SPACECENTER is recommended so that the constellation data can be verified and error-corrected in advance

    //TODO: investigate to add extra ground stations to the group of existing stations
    KCommNetUI CustomCommNetUI = null;                              //  Looks like that I don't need this class, custom line color.
    KCommNetNetwork CustomCommNetNetwork = null;
    List<KCommNetHome> groundStations;                              // leave the initialisation to OnLoad()
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
      Lib.Log("KCommNetUI loaded");

      //Replace the CommNet network
      CommNetNetwork net = FindObjectOfType<CommNetNetwork>();
      CustomCommNetNetwork = gameObject.AddComponent<KCommNetNetwork>();
      Destroy(net);
      Lib.Log("KCommNetNetwork loaded");

      //Replace the CommNet ground stations
      groundStations = new List<KCommNetHome>();
      CommNetHome[] homes = FindObjectsOfType<CommNetHome>();
      for (int i = 0; i < homes.Length; i++)
      {
        KCommNetHome customHome = homes[i].gameObject.AddComponent(typeof(KCommNetHome)) as KCommNetHome;
        customHome.copyOf(homes[i]);
        UnityEngine.Object.Destroy(homes[i]);
        groundStations.Add(customHome);
      }
      groundStations.Sort();
      Lib.Log("KCommNetHome loaded");

      //Replace the CommNet celestial bodies
      CommNetBody[] bodies = FindObjectsOfType<CommNetBody>();
      for (int i = 0; i < bodies.Length; i++)
      {
        KCommNetBody customBody = bodies[i].gameObject.AddComponent(typeof(KCommNetBody)) as KCommNetBody;
        customBody.copyOf(bodies[i]);
        UnityEngine.Object.Destroy(bodies[i]);
      }
      Lib.Log("KCommNetBody loaded");

      Lib.Log("CommNet Scenario loading done! ");
    }

    public override void OnAwake()
    {
      //override to turn off CommNetScenario's instance check

      GameEvents.onVesselCreate.Add(new EventData<Vessel>.OnEvent(onVesselCountChanged));
      GameEvents.onVesselDestroy.Add(new EventData<Vessel>.OnEvent(onVesselCountChanged));
    }

    private void OnDestroy()
    {
      if (CustomCommNetUI != null) Destroy(CustomCommNetUI);
      if (CustomCommNetNetwork != null) Destroy(CustomCommNetNetwork);

      this.commVessels.Clear();

      GameEvents.onVesselCreate.Remove(new EventData<Vessel>.OnEvent(onVesselCountChanged));
      GameEvents.onVesselDestroy.Remove(new EventData<Vessel>.OnEvent(onVesselCountChanged));
    }

    // Obtain all communicable vessels that have the given frequency
    public List<KCommNetVessel> getCommNetVessels(short targetFrequency = -1)
    {
      cacheCommNetVessels();

      List<KCommNetVessel> newList = new List<KCommNetVessel>();
      return newList;
    }

    // GameEvent call for newly-created vessels (launch, staging, new asteriod etc)
    // NOTE: Vessel v is fresh bread straight from the oven before any curation is done on this (i.e. debris.Connection is valid)
    private void onVesselCountChanged(Vessel v)
    {
      if (v.vesselType == VesselType.Base || v.vesselType == VesselType.Lander || v.vesselType == VesselType.Plane ||
         v.vesselType == VesselType.Probe || v.vesselType == VesselType.Relay || v.vesselType == VesselType.Rover ||
         v.vesselType == VesselType.Ship || v.vesselType == VesselType.Station)
      {
        Lib.Log("Change in the vessel list detected. Cache refresh required.");
        this.dirtyCommNetVesselList = true;
      }
    }

    // Cache eligible vessels of the FlightGlobals
    private void cacheCommNetVessels()
    {
      if (!this.dirtyCommNetVesselList)
        return;

      Lib.Log("CommNetVessel cache - " + this.commVessels.Count + " entries deleted");
      this.commVessels.Clear();

      List<Vessel> allVessels = FlightGlobals.fetch.vessels;
      for (int i = 0; i < allVessels.Count; i++)
      {
        if (allVessels[i].connection != null && allVessels[i].vesselType != VesselType.Unknown)// && allVessels[i].vesselType != VesselType.Debris) // debris could be spent stage with functional probes and antennas
        {
          Lib.Log("Caching CommNetVessel " + allVessels[i].vesselName);
          this.commVessels.Add(allVessels[i].connection as KCommNetVessel);
        }
      }

      Lib.Log("CommNetVessel cache - " + this.commVessels.Count + " entries added");

      this.dirtyCommNetVesselList = false;
    }
  }
}
