using CommNet;
using KSP.UI.Screens.Mapview;
using System.Collections.Generic;
using UnityEngine;

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
      base.UpdateDisplay();
      updateView();
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

    // Update the MapNode object of each CommNet vessel
    private void OnMapNodeUpdateVisible(MapNode node, MapNode.IconData iconData)
    {
      KCommNetVessel thisVessel = (KCommNetVessel)node.mapObject.vessel.connection;

      if (thisVessel != null && node.mapObject.type == MapObject.ObjectType.Vessel)
      {
        iconData.color = colorHigh;
      }
    }

    // Render the CommNet presentation
    // This method has been created to modify line color
    private void updateView()
    {
      CommNetwork net = CommNetNetwork.Instance.CommNet;
      CommNetVessel cnvessel = null;
      CommNode node = null;
      CommPath path = null;

      if (vessel != null && vessel.connection != null && vessel.connection.Comm.Net != null)
      {
        cnvessel = vessel.connection;
        node = cnvessel.Comm;
        path = cnvessel.ControlPath;
      }

      // Work out how many connections to paint
      int numLinks = 0;
      int count = points.Count;

      switch (Mode)
      {
        case DisplayMode.None:
        {
          numLinks = 0;
          break;
        }
        case DisplayMode.FirstHop:
        {
          if (cnvessel.ControlState == VesselControlState.Probe || cnvessel.ControlState == VesselControlState.Kerbal || path == null || path.Count == 0)
          {
            numLinks = 0;
          }
          else
          {
            path.First.GetPoints(points);
            numLinks = 1;
          }
          break;
        }
        case DisplayMode.Path:
        {
          if (cnvessel.ControlState == VesselControlState.Probe || cnvessel.ControlState == VesselControlState.Kerbal || path == null || path.Count == 0)
          {
            numLinks = 0;
          }
          else
          {
            path.GetPoints(points, true);
            numLinks = path.Count;
          }
          break;
        }
        case DisplayMode.VesselLinks:
        {
          numLinks = node.Count;
          node.GetLinkPoints(points);
          break;
        }
        case DisplayMode.Network:
        {
          if (net.Links.Count == 0)
          {
            numLinks = 0;
          }
          else
          {
            numLinks = net.Links.Count;
            net.GetLinkPoints(points);
          }
          break;
        }
      }

      // Check if nothing to draw
      if (numLinks == 0)
      {
        if (line != null) line.active = false;
        points.Clear();
        return;
      }
      else
      {
        // TODO: I'm not sure if I need the commented part below.
        //if (line != null) line.active = true;
        //else refreshLines = true;
        //ScaledSpace.LocalToScaledSpace(points);
        //if (refreshLines || MapView.Draw3DLines != draw3dLines || (count != points.Count || line == null))
        //{
        //  CreateLine(ref line, points);
        //  draw3dLines = MapView.Draw3DLines;
        //  refreshLines = false;
        //}

        //paint eligible connections

        // Sample to create a customHighColor :  
        //    Color customHighColor = getConstellationColor(path.First.a, path.First.b);
        //      If want custom color, alter colorHigh for customHighColor
        switch (Mode)
        {
          case DisplayMode.FirstHop:
          {
            float lvl = Mathf.Pow((float)path.First.signalStrength, colorLerpPower);
            if (swapHighLow)
              line.SetColor(Color.Lerp(colorHigh, colorLow, lvl), 0);
            else
              line.SetColor(Color.Lerp(colorLow, colorHigh, lvl), 0);
            break;
          }
          case DisplayMode.Path:
          {
            int linkIndex = numLinks;
            for (int i = linkIndex - 1; i >= 0; i--)
            {
              float lvl = Mathf.Pow((float)path[i].signalStrength, colorLerpPower);
              if (swapHighLow)
                line.SetColor(Color.Lerp(colorHigh, colorLow, lvl), i);
              else
                line.SetColor(Color.Lerp(colorLow, colorHigh, lvl), i);
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
              if (swapHighLow)
                line.SetColor(Color.Lerp(colorHigh, colorLow, lvl), linkIndex++);
              else
                line.SetColor(Color.Lerp(colorLow, colorHigh, lvl), linkIndex++);
            }
            break;
          }
          case DisplayMode.Network:
          {
            int linkIndex = numLinks;
            while (linkIndex-- > 0)
            {
              CommLink commLink = net.Links[linkIndex];
              float t2 = Mathf.Pow((float)net.Links[linkIndex].GetBestSignal(), colorLerpPower);
              if (swapHighLow)
                line.SetColor(Color.Lerp(colorHigh, colorLow, t2), linkIndex);
              else
                line.SetColor(Color.Lerp(colorLow, colorHigh, t2), linkIndex);
            }
            break;
          }
        }
      }
    }
  }
}
