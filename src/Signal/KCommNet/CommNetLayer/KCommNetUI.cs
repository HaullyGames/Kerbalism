using CommNet;
using CommNet.Network;
using KSP.Localization;
using KSP.UI.Screens.Mapview;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

// Original code by TaxiService from https://github.com/KSP-TaxiService/CommNetConstellation
namespace KCOMMNET
{
  public class KCommNetUI : CommNetUI
  {
    public static new KCommNetUI Instance { get; protected set; }

    // Activate things when the player enter a scene that uses CommNet UI
    public override void Show()
    {
      registerMapNodeIconCallbacks();
      base.Show();
    }

    // Clean up things when the player exits a scene that uses CommNet UI
    public override void Hide()
    {
      deregisterMapNodeIconCallbacks();
      base.Hide();
    }

    // Run own display updates
    protected override void UpdateDisplay()
    {
      //base.UpdateDisplay();       // Testing, updateView already do all the same of UpdateDisplay(). but updateView has support to custom color line
      updateView();
    }

    // Maybe I will use this method
    // TODO: when vessel is transmitting, line will have a special effect
    protected override void CreateLine(ref VectorLine l, List<Vector3> points)
    {
      if (l != null)
        VectorLine.Destroy(ref l);
      l = new VectorLine("CommNetUIVectorLine", points, lineWidth2D, LineType.Discrete);
      l.rectTransform.gameObject.layer = 31;
      l.material = lineMaterial;
      l.smoothColor = smoothColor;
      l.UpdateImmediate = true;
    }

    // Register own callbacks
    protected void registerMapNodeIconCallbacks()
    {
      List<KCommNetVessel> commnetVessels = KCommNetScenario.Instance.getCommNetVessels();

      for (int i = 0; i < commnetVessels.Count; i++)
      {
        MapObject mapObj = commnetVessels[i].Vessel.mapObject;

        if (mapObj.type == MapObject.ObjectType.Vessel)
          mapObj.uiNode.OnUpdateVisible += new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
      }
    }

    // Remove own callbacks
    protected void deregisterMapNodeIconCallbacks()
    {
      List<KCommNetVessel> commnetVessels = KCommNetScenario.Instance.getCommNetVessels();

      for (int i = 0; i < commnetVessels.Count; i++)
      {
        MapObject mapObj = commnetVessels[i].Vessel.mapObject;
        mapObj.uiNode.OnUpdateVisible -= new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
      }
    }

    //  Update the MapNode object of each CommNet vessel
    private void OnMapNodeUpdateVisible(MapNode node, MapNode.IconData iconData)
    {
      KCommNetVessel thisVessel = (KCommNetVessel)node.mapObject.vessel.connection;

      if (thisVessel != null && node.mapObject.type == MapObject.ObjectType.Vessel)
      {
        if (thisVessel.getStrongestFrequency() < 0)   // blind vessel
          iconData.color = Color.grey;
        else
          iconData.color = Constellation.getColor(thisVessel.getStrongestFrequency());
      }
    }

    // Compute the color based on the connection between two nodes
    private Color getConstellationColor(CommNode a, CommNode b)
    {
      //Assume the connection between A and B passes the check test
      List<short> commonFreqs = Constellation.NonLinqIntersect(KCommNetScenario.Instance.getFrequencies(a), KCommNetScenario.Instance.getFrequencies(b));
      IRangeModel rangeModel = KCommNetScenario.RangeModel;
      short strongestFreq = -1;
      double longestRange = 0.0;

      for (int i = 0; i < commonFreqs.Count; i++)
      {
        short thisFreq = commonFreqs[i];
        double thisRange = rangeModel.GetMaximumRange(KCommNetScenario.Instance.getCommPower(a, thisFreq), KCommNetScenario.Instance.getCommPower(b, thisFreq));

        if (thisRange > longestRange)
        {
          longestRange = thisRange;
          strongestFreq = thisFreq;
        }
      }

      return Constellation.getColor(strongestFreq);
    }

    //  Render the CommNet presentation
    //  This method has been created to modify line color
    private void updateView()
    {
      if (FlightGlobals.ActiveVessel == null) useTSBehavior = true;
      else
      {
        useTSBehavior = false;
        vessel = FlightGlobals.ActiveVessel;
      }
      if (vessel == null || vessel.connection == null || vessel.connection.Comm.Net == null)
      {
        useTSBehavior = true;
        if (ModeTrackingStation != DisplayMode.None)
        {
          if (ModeTrackingStation != DisplayMode.Network)
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118264", new string[1]
            {
              Localizer.Format(DisplayMode.Network.displayDescription())
            }), 5f);
          ModeTrackingStation = DisplayMode.Network;
        }
      }
      if (CommNetNetwork.Instance == null) return;

      CommNetwork net = CommNetNetwork.Instance.CommNet;
      CommNetVessel cnvessel = null;
      CommNode node = null;
      CommPath path = null;

      if (vessel != null && vessel.connection != null && vessel.connection.Comm.Net != null)
      {
        cnvessel = this.vessel.connection;
        node = cnvessel.Comm;
        path = cnvessel.ControlPath;
      }

      //work out how many connections to paint
      int numLinks = 0;
      int count = points.Count;

      switch (Mode)
      {
        case DisplayMode.None:
        numLinks = 0;
        break;

        case DisplayMode.FirstHop:
        if (cnvessel.ControlState == VesselControlState.Probe || cnvessel.ControlState == VesselControlState.Kerbal || path == null || path.Count == 0)
        {
          numLinks = 0;
          break;
        }
        path.First.GetPoints(points);
        numLinks = 1;
        break;

        case DisplayMode.Path:
        if (cnvessel.ControlState == VesselControlState.Probe || cnvessel.ControlState == VesselControlState.Kerbal || path == null || path.Count == 0)
        {
          numLinks = 0;
          break;
        }
        path.GetPoints(points, true);
        numLinks = path.Count;
        break;

        case DisplayMode.VesselLinks:
        numLinks = node.Count;
        node.GetLinkPoints(points);
        break;

        case DisplayMode.Network:
        if (net.Links.Count == 0)
        {
          numLinks = 0;
          break;
        }
        net.GetLinkPoints(points);
        numLinks = net.Links.Count;
        break;
      }

      //check if nothing to draw
      if (numLinks == 0)
      {
        if (line != null) line.active = false;
        points.Clear();
        return;
      }
      else
      {
        if (line != null) line.active = true;
        else refreshLines = true;
        ScaledSpace.LocalToScaledSpace(points);
        if (refreshLines || MapView.Draw3DLines != draw3dLines || (count != points.Count || line == null))
        {
          CreateLine(ref line, points);
          draw3dLines = MapView.Draw3DLines;
          refreshLines = false;
        }

        //paint eligible connections
        switch (Mode)
        {
          case DisplayMode.FirstHop:
          {
            float lvl = Mathf.Pow((float)path.First.signalStrength, colorLerpPower);
            //  Color customHighColor = getConstellationColor(path.First.a, path.First.b);//  If I need custom color line
            if (swapHighLow)
              line.SetColor(Color.Lerp(colorHigh, colorLow, lvl), 0);                     //  If want custom color, alter colorHigh for customHighColor
            else
              line.SetColor(Color.Lerp(this.colorLow, colorHigh, lvl), 0);                //  If want custom color, alter colorHigh for customHighColor
            break;
          }

          case DisplayMode.Path:
          {
            int linkIndex = numLinks;
            for (int i = linkIndex - 1; i >= 0; i--)
            {
              float lvl = Mathf.Pow((float)path[i].signalStrength, this.colorLerpPower);
              //  Color customHighColor = getConstellationColor(path[i].a, path[i].b);    //  If I need custom color line
              if (this.swapHighLow)
                this.line.SetColor(Color.Lerp(colorHigh, this.colorLow, lvl), i);         //  If want custom color, alter colorHigh for customHighColor
              else
                this.line.SetColor(Color.Lerp(this.colorLow, colorHigh, lvl), i);         //  If want custom color, alter colorHigh for customHighColor
            }
            break;
          }

          case DisplayMode.VesselLinks:
          {
            var itr = node.Values.GetEnumerator();
            int linkIndex = 0;
            while (itr.MoveNext())
            {
              CommLink link = itr.Current;
              float lvl = Mathf.Pow((float)link.GetSignalStrength(link.a != node, link.b != node), colorLerpPower);
              //  Color customHighColor = getConstellationColor(link.a, link.b);          //  To custom color line
              if (swapHighLow)
                line.SetColor(Color.Lerp(colorHigh, colorLow, lvl), linkIndex++);
              else
                line.SetColor(Color.Lerp(colorLow, colorHigh, lvl), linkIndex++);         //  If want custom color, alter colorHigh for customHighColor
            }
            break;
          }

          case DisplayMode.Network:
          {
            int linkIndex = numLinks;
            while (linkIndex-- > 0)
            {
              CommLink commLink = net.Links[linkIndex];
              //float f = (float)net.Links[i].GetBestSignal();
              //float t = Mathf.Pow(f, this.colorLerpPower);
              //Color customHighColor = getConstellationColor(commLink.a, commLink.b);    //  To custom color line
              float t2 = Mathf.Pow((float)net.Links[linkIndex].GetBestSignal(), colorLerpPower);
              if (swapHighLow)
                line.SetColor(Color.Lerp(colorHigh, colorLow, t2), linkIndex);            //  If want custom color, alter colorHigh for customHighColor
              else
                line.SetColor(Color.Lerp(colorLow, colorHigh, t2), linkIndex);            //  If want custom color, alter colorHigh for customHighColor
            }
            break;
          }
        }
      }
    }
  }
}
