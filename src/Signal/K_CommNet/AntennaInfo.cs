using CommNet;
using System;
using System.Collections.Generic;

namespace KCOMMNET
{
  public sealed class AntennaInfo
  {
    public static float rangeModifier = HighLogic.fetch.currentGame.Parameters.CustomParams<CommNetParams>().rangeModifier;   //	CommNet Range Modifier

    public double AntennaRange;
    public double RelayRange;
    public double MaxDistanceTarget;                                                                              //	Max distance to the Target
    public double AntennaRate = 0;                                                                                //	Kbps

    public List<ModuleDataTransmitter> ListAntenna = new List<ModuleDataTransmitter>();                           //	List of antennas

    public AntennaInfo(CommNode node)
    {
      double mainAntenna = 0;                                                                                     //	Strongest Antenna
      double AntennaSum = 0;                                                                                      //	Sum of all Antennas
      double AntennaComb = 0;                                                                                     //	Sum of all Antennas Combinability
      double AntennaCombExponent = 0;                                                                             //	Average Weighted Combinability Exponent

      double mainRelay = 0;                                                                                       //	Strongest Relay
      double RelaySum = 0;                                                                                        //	Sum of all Relay
      double RelayComb = 0;                                                                                       //	Sum of all Relay Combinability
      double RelayCombExponent = 0;                                                                               //	Average Weighted Combinability Exponent

      bool extended = false;                                                                                      //	Fixed and deployable antennas
      bool allAntennaEqual = true;                                                                                //	If all antennas is Equal, antennaCombinableExponent = 1
      bool allRelayEqual = true;                                                                                  //	If all relay is Equal, antennaCombinableExponent = 1

      string antennaT = null;                                                                                     //	Only one temporary antenna for compare
      string relayT = null;                                                                                       //	Only one temporary relay for compare

      double AntennaXModified;                                                                                    //	(AntennaPower || RelayPower) * rangeModifier

      if (node.isHome)
      {
        RelayRange = node.antennaRelay.power;
      }
      else
      {
        Vessel v = KERBALISM.Lib.KCOMMNET.CommNodeToVessel(node);
        if (v != null)
        {
          if (v.loaded)
          {
            foreach (ModuleDataTransmitter antenna in v.FindPartModulesImplementing<ModuleDataTransmitter>())
            {
              ModuleDeployableAntenna anim = antenna.part.FindModuleImplementing<ModuleDeployableAntenna>();

              //	Assume extended if there is no animator or If statys is Extended
              if (anim != null)
              {
                extended = (anim.deployState == ModuleDeployablePart.DeployState.EXTENDED);
              }
              else extended = true;

              //	Calc range only for Antennas that is working
              if (extended)
              {
                ListAntenna.Add(antenna);

                NetworkAdaptor ant = antenna.part.FindModuleImplementing<NetworkAdaptor>();
                if (ant != null)
                {
                  AntennaRate += ant.rate;
                }

                //	Set first value
                if (antennaT == null) antennaT = antenna.name;

                allAntennaEqual = (allAntennaEqual && antennaT == antenna.name);

                if (antenna.antennaType == AntennaType.RELAY)
                {
                  if (relayT == null) relayT = antenna.name;
                  allRelayEqual = (allRelayEqual && relayT == antenna.name);
                }
              }
            }

            foreach (ModuleDataTransmitter antenna in ListAntenna)
            {
              //	Have to confirm, I put to arround up
              //	Antenna Power X CommNet rangeModifier
              AntennaXModified = Math.Round(antenna.antennaPower * rangeModifier, 1);

              //	Strongest antenna
              mainAntenna = (mainAntenna < AntennaXModified ? AntennaXModified : mainAntenna);

              //	Total Antenna Power
              AntennaSum += AntennaXModified;

              //	Average Weighted Combinability
              if (antenna.antennaCombinable) AntennaComb += Math.Round(AntennaXModified * ((allAntennaEqual) ? 1 : antenna.antennaCombinableExponent), 1);

              //	If connected through Relay, accept only AntennaType = Relay to calc
              if (antenna.CommType == AntennaType.RELAY)
              {
                //	Strongest antenna
                mainRelay = (mainRelay < AntennaXModified ? AntennaXModified : mainRelay);

                //	Total Antenna Power
                RelaySum += AntennaXModified;

                //	Average Weighted Combinability
                if (antenna.CommCombinable) RelayComb += Math.Round(AntennaXModified * ((allRelayEqual) ? 1 : antenna.antennaCombinableExponent), 1);
              }
            }

            //	Average Weighted Combinability Exponent 
            AntennaCombExponent = AntennaComb / AntennaSum;
            RelayCombExponent = RelayComb / RelaySum;

            //	Formula source = https://wiki.kerbalspaceprogram.com/wiki/CommNet
            //	Vessel Antenna Power = Strongest Antenna Power * ( Sum of Antenna's Powers / Strongest Antenna Power ) ^ ( Average Weighted Combinability Exponent for Vessel )
            //	Vessel Antenna Power = mainAntenna * (AntennaComb/AntennaSum) ^ AntennaCombExponent;
            AntennaRange = Math.Ceiling(mainAntenna * Math.Pow((AntennaSum / mainAntenna), AntennaCombExponent));
            RelayRange = Math.Ceiling(mainRelay * Math.Pow((RelaySum / mainRelay), RelayCombExponent));
          }
        }
      }
    }
  }
}
