﻿using UnityEngine;
using System.Diagnostics;

public abstract class AbstractBuilding : UnitOptions
{
    public BuildingSetting SettingFile;
    private EnumProvider.ORDERSLIST unitState;

    public uint MaxLevel
    {
        get
        {
            return (uint)this.SettingFile.LevelResource.Count;
        }
    }

    protected uint CurrentResource
    {
        get
        {
            return (uint)this.SettingFile.LevelResource[(int)SettingFile.Level];
        }
    }

    protected uint CurrentWorkSpeed
    {
        get
        {
            return (uint)this.SettingFile.WorkSpeedLevel[this.SettingFile.Level];
        }
    }

    public void BuildingCost()
    {
        ResourceManager.SubtractResouce(ResourceManager.Resource.MATTER, this.SettingFile.MatterCost);
        ResourceManager.SubtractResouce(ResourceManager.Resource.NANITEN, this.SettingFile.NaniteCost);
    }

    public override System.Enum UnitState
    {
        get
        {
            return unitState;
        } 
        set
        {
            //switch ((EnumProvider.ORDERSLIST)value)
            //{    
            //}
            unitState = (EnumProvider.ORDERSLIST)value;
        }
    }

    public abstract void BuildFinished();

    //public void UpgradeBuilding()
    //{
    //    if (this.Level == 1 && this.Level != this.MaxLevel && ResourceManager.Resource.MATTER - (ResourceManager.Resource)this.MatterUpgradeCostLvl1 >= 0 && ResourceManager.Resource.NANITEN - (ResourceManager.Resource) this.NaniteUpgradeCostLvl1 >= 0 /*&&Button Upgrade pressed*/
    //    {
    //        this.Level = 2;
    //        ResourceManager.SubtractResouce(ResourceManager.Resource.MATTER, this.MatterUpgradeCostLvl1);
    //        ResourceManager.SubtractResouce(ResourceManager.Resource.NANITEN, this.NaniteUpgradeCostLvl1);
    //    }
    //}
}
