using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Object;
using Object = UnityEngine.Object;

/// <summary>
/// Updated from SkylinesBulldoze added global preferences
/// added Additional selections
/// </summary>
namespace AnotherBulldoze
{
    public class Mod : IUserMod
    {
        public string Description
        {
            get { return "Another bulldoze tool"; }
        }

        public string Name
        {
            get { return "Another bulldoze Tool"; }
        }
        
        /// <summary>
        /// Global Options
        /// </summary>
        /// <param name="helper"></param>
        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Another Bulldoze Settings");
            group.AddGroup("Lines");
            group.AddCheckbox("Roads", Properties.Settings.Default.Roads, RoadsCheck);
            group.AddCheckbox("Railroads", Properties.Settings.Default.Railroads, RailroadsCheck);
            group.AddCheckbox("Highways", Properties.Settings.Default.Highways, HighwaysCheck);
            group.AddCheckbox("PowerLines", Properties.Settings.Default.PowerLines, PowerLinesCheck);
            group.AddCheckbox("Pipes", Properties.Settings.Default.Pipes, PipesCheck);
            group.AddSpace(20);
            group.AddGroup("Line Options");
            group.AddCheckbox("Ground", Properties.Settings.Default.Ground, GroundCheck);
            group.AddCheckbox("Tunnel", Properties.Settings.Default.Tunnel, TunnelCheck);
            group.AddCheckbox("Bridge", Properties.Settings.Default.Bridge, BridgeCheck);
            group.AddSpace(20);
            group.AddGroup("Properties");
            group.AddCheckbox("Buildings", Properties.Settings.Default.Buildings, BuildingsCheck);
            group.AddCheckbox("Trees", Properties.Settings.Default.Trees, TreesCheck);
            group.AddCheckbox("Props", Properties.Settings.Default.Props, PropsCheck);
        }

        private void RoadsCheck(bool c)
        {
            Properties.Settings.Default.Roads = c;
        }
        private void RailroadsCheck(bool c)
        {
            Properties.Settings.Default.Railroads = c;
        }
        private void HighwaysCheck(bool c)
        {
            Properties.Settings.Default.Highways = c;
        }
        private void BuildingsCheck(bool c)
        {
            Properties.Settings.Default.Buildings = c;
        }
        private void TreesCheck(bool c)
        {
            Properties.Settings.Default.Trees = c;
        }
        private void PowerLinesCheck(bool c)
        {
            Properties.Settings.Default.PowerLines = c;
        }
        private void PipesCheck(bool c)
        {
            Properties.Settings.Default.Pipes = c;
        }
        private void PropsCheck(bool c)
        {
            Properties.Settings.Default.Props = c;
        }
        private void GroundCheck(bool c)
        {
            Properties.Settings.Default.Ground = c;
        }
        private void TunnelCheck(bool c)
        {
            Properties.Settings.Default.Tunnel = c;
        }
        private void BridgeCheck(bool c)
        {
            Properties.Settings.Default.Bridge = c;
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        public AnotherBulldozeTool bulldozeTool;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            bulldozeTool = FindObjectOfType<AnotherBulldozeTool>();
            if(bulldozeTool == null)
            {
                GameObject gameController = GameObject.FindWithTag("GameController");
                bulldozeTool = gameController.AddComponent<AnotherBulldozeTool>();
            }
            bulldozeTool.InitGui(mode);
            bulldozeTool.enabled = false;
        }
    }

}
