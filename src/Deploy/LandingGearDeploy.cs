﻿using ModuleWheels;

namespace KERBALISM
{
  public class LandingGearDeploy : DeployBase
  {
    ModuleWheelDeployment gear;

    public override void Start()
    {
      thisModule = "LandingGearDeploy";
      gear = part.FindModuleImplementing<ModuleWheelDeployment>();
    }

    public override bool IsActive
    {
      get
      {
        if (gear != null)
        {
          if (!Features.Deploy)
          {
            gear.isEnabled = true;
            return false;
          }
          if (hasEC)
          {
            gear.isEnabled = gear.Events["EventToggle"].active = gear.Events["EventToggle"].guiActive = gear.Events["EventToggle"].guiActiveUnfocused = true;

            if (gear.stateString == "Deploying..." || gear.stateString == "Retracting...")
            {
              actualECCost = ecCost;
              return true;
            }
          }
          else
          {
            gear.isEnabled = gear.Events["EventToggle"].active = gear.Events["EventToggle"].guiActive = gear.Events["EventToggle"].guiActiveUnfocused = false;
          }
        }
        return false;
      }
    }
  }
}
