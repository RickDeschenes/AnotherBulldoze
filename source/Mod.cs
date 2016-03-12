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
            group.AddCheckbox("Roads", false, RoadsCheck);
            group.AddCheckbox("Highways", false, HighwaysCheck);
            group.AddCheckbox("Railroads", false, RailroadsCheck);
            group.AddCheckbox("Buildings", false, BuildingsCheck);
            group.AddCheckbox("Trees", false, TreesCheck);
            group.AddCheckbox("PowerLines", false, PowerLinesCheck);
            group.AddCheckbox("Pipes", false, PipesCheck);
            group.AddCheckbox("Props", false, PropsCheck);
        }

        private void RoadsCheck(bool c)
        {
            G._Roads = c;
        }
        private void RailroadsCheck(bool c)
        {
            G._Railroads = c;
        }
        private void HighwaysCheck(bool c)
        {
            G._Highways = c;
        }
        private void BuildingsCheck(bool c)
        {
            G._Buildings = c;
        }
        private void TreesCheck(bool c)
        {
            G._Trees = c;
        }
        private void PowerLinesCheck(bool c)
        {
            G._PowerLines = c;
        }
        private void PipesCheck(bool c)
        {
            G._Pipes = c;
        }
        private void PropsCheck(bool c)
        {
            G._Props = c;
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
