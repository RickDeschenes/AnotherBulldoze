﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

/// <summary>
/// Updated from SkylinesBulldoze added global preferences
/// added Additional selections
/// Added new to remove warning for reused m_
/// </summary>
namespace AnotherBulldoze
{
    public class AnotherBulldozeTool : DefaultTool
    {

        private object m_dataLock = new object();
        
        private bool m_active;
        private Vector3 m_startPosition;
        private Vector3 m_startDirection;
        private new Vector3 m_mousePosition;
        private Vector3 m_mouseDirection;
        private new bool m_mouseRayValid;
        private new Ray m_mouseRay;
        private new float m_mouseRayLength;
        private Vector3 m_cameraDirection;
        public List<ushort> segmentsToDelete;
        public float m_maxArea = 400f;

        public UIButton mainButton;
        public UIPanel marqueeBulldozePanel;

        private UICheckBox cbRoads;
        private UICheckBox cbRailroads;
        private UICheckBox cbHighways;
        private UICheckBox cbBuildings;
        private UICheckBox cbTrees;
        private UICheckBox cbPowerLines;
        private UICheckBox cbPipes;
        private UICheckBox cbProps;

        protected override void Awake()
        {
            //this.m_dataLock = new object();
            m_active = false;
            base.Awake();
        }

        public void InitGui(LoadMode mode)
        {
            mainButton = UIView.GetAView().FindUIComponent<UIButton>("MarqueeBulldozer");
            
            if(mainButton == null)
            {
                var bulldozeButton = UIView.GetAView().FindUIComponent<UIMultiStateButton>("BulldozerButton");

                mainButton = bulldozeButton.parent.AddUIComponent<UIButton>();
                mainButton.name = "MarqueeBulldozer";
                mainButton.size = new Vector2(36, 36);
                mainButton.tooltip = "Another Bulldozer";
                mainButton.relativePosition = new Vector2
                (
                    bulldozeButton.relativePosition.x + bulldozeButton.width / 2.0f - mainButton.width - bulldozeButton.width,
                    bulldozeButton.relativePosition.y + bulldozeButton.height / 2.0f - mainButton.height / 2.0f
                );
                if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
                {
                    mainButton.normalBgSprite = "ZoningOptionMarquee";
                    mainButton.focusedFgSprite = "ToolbarIconGroup6Focused";
                    mainButton.hoveredFgSprite = "ToolbarIconGroup6Hovered";
                }
                else { 
                    mainButton.normalFgSprite = bulldozeButton.normalFgSprite;
                    mainButton.focusedFgSprite = bulldozeButton.focusedFgSprite;
                    mainButton.hoveredFgSprite = bulldozeButton.hoveredFgSprite;
                }

                mainButton.eventClick += buttonClicked;

                marqueeBulldozePanel = UIView.GetAView().FindUIComponent("TSBar").AddUIComponent<UIPanel>();
                marqueeBulldozePanel.backgroundSprite = "SubcategoriesPanel";
                marqueeBulldozePanel.isVisible = false;
                marqueeBulldozePanel.name = "MarqueeBulldozerSettings";
                marqueeBulldozePanel.size = new Vector2(150, 220);

                marqueeBulldozePanel.relativePosition = new Vector2
                (
                    bulldozeButton.relativePosition.x + bulldozeButton.width / 2.0f - marqueeBulldozePanel.width ,
                    bulldozeButton.relativePosition.y - marqueeBulldozePanel.height 
                );
                marqueeBulldozePanel.isVisible = true;

                cbRoads = addCheckbox(marqueeBulldozePanel, 20, "Roads");
                cbRailroads = addCheckbox(marqueeBulldozePanel, 70, "Railroads");
                cbHighways = addCheckbox(marqueeBulldozePanel, 120, "Highways");
                cbBuildings = addCheckbox(marqueeBulldozePanel, 45, "Buildings");
                cbTrees = addCheckbox(marqueeBulldozePanel, 95, "Trees");
                cbPowerLines = addCheckbox(marqueeBulldozePanel, 195, "PowerLines");
                cbProps = addCheckbox(marqueeBulldozePanel, 145, "Props");
                cbPipes = addCheckbox(marqueeBulldozePanel, 170, "Pipes");

                cbRoads.isChecked = G._Roads;
                cbRailroads.isChecked = G._Railroads;
                cbHighways.isChecked = G._Highways;
                cbBuildings.isChecked = G._Buildings;
                cbTrees.isChecked = G._Trees;
                cbPowerLines.isChecked = G._PowerLines;
                cbProps.isChecked = G._Props;
                cbPipes.isChecked = G._Pipes;

            }
        }

        void buttonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            this.enabled = true;
            marqueeBulldozePanel.isVisible = true;
        }

        protected override void OnEnable()
        {
            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            if (marqueeBulldozePanel != null)
                marqueeBulldozePanel.isVisible = false;
            base.OnDisable();
        }

        private UICheckBox addCheckbox(UIPanel panel, int yPos, string text)
        {

            var checkBox = marqueeBulldozePanel.AddUIComponent<UICheckBox>();
            checkBox.relativePosition = new Vector3(20, yPos);
            checkBox.height = 20;
            checkBox.width = 20;

            var label = marqueeBulldozePanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(45, yPos+3);
            checkBox.label = label;
            checkBox.text = text;
            UISprite uncheckSprite = checkBox.AddUIComponent<UISprite>();
            uncheckSprite.height = 20;
            uncheckSprite.width = 20;
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            UISprite checkSprite = checkBox.AddUIComponent<UISprite>();
            checkSprite.height = 20;
            checkSprite.width = 20;
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            checkBox.checkedBoxObject = checkSprite;
            checkBox.isChecked = true;
            return checkBox;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            Vector3 startPosition;
            Vector3 mousePosition;
            Vector3 startDirection;
            Vector3 mouseDirection;
            bool active;

            try
            {
                active = this.m_active;

                startPosition = this.m_startPosition;
                mousePosition = this.m_mousePosition;
                startDirection = this.m_startDirection;
                mouseDirection = this.m_mouseDirection;
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }

            var color = Color.red;

            if (!active)
            {
                base.RenderOverlay(cameraInfo);
                return;
            }

            Vector3 a = (!active) ? mousePosition : startPosition;
            Vector3 vector = mousePosition;
            Vector3 a2 = (!active) ? mouseDirection : startDirection;
            Vector3 a3 = new Vector3(a2.z, 0f, -a2.x);

            float num = Mathf.Round(((vector.x - a.x) * a2.x + (vector.z - a.z) * a2.z) * 0.125f) * 8f;
            float num2 = Mathf.Round(((vector.x - a.x) * a3.x + (vector.z - a.z) * a3.z) * 0.125f) * 8f;

            float num3 = (num < 0f) ? -4f : 4f;
            float num4 = (num2 < 0f) ? -4f : 4f;

            Quad3 quad = default(Quad3);
            quad.a = a - a2 * num3 - a3 * num4;
            quad.b = a - a2 * num3 + a3 * (num2 + num4);
            quad.c = a + a2 * (num + num3) + a3 * (num2 + num4);
            quad.d = a + a2 * (num + num3) - a3 * num4;

            if (num3 != num4)
            {
                Vector3 b = quad.b;
                quad.b = quad.d;
                quad.d = b;
            }
            ToolManager toolManager = ToolManager.instance;
            toolManager.m_drawCallData.m_overlayCalls++;
            RenderManager.instance.OverlayEffect.DrawQuad(cameraInfo, color, quad, -1f, 1025f, false, true);
            base.RenderOverlay(cameraInfo);
            return;
        }

        protected override void OnToolLateUpdate()
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 cameraDirection = Vector3.Cross(Camera.main.transform.right, Vector3.up);
            cameraDirection.Normalize();
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                this.m_mouseRay = Camera.main.ScreenPointToRay(mousePosition);
                this.m_mouseRayLength = Camera.main.farClipPlane;
                this.m_cameraDirection = cameraDirection;
                this.m_mouseRayValid = (!this.m_toolController.IsInsideUI && UnityEngine.Cursor.visible);
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }
        }

        /// <summary>
        /// Updated to set max range
        /// </summary>
        /// <param name="newMousePosition"></param>
        /// <returns></returns>
        private bool checkMaxArea(Vector3 newMousePosition)
        {
            if ((m_startPosition -newMousePosition).sqrMagnitude > m_maxArea * 100000)
            {
                return false;
            }
            return true;
        }

        public override void SimulationStep()
        {
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            Ray mouseRay;
            Vector3 cameraDirection;
            bool mouseRayValid;
            try
            {
                mouseRay = this.m_mouseRay;
                cameraDirection = this.m_cameraDirection;
                mouseRayValid = this.m_mouseRayValid;
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }

            ToolBase.RaycastInput input = new ToolBase.RaycastInput(mouseRay, m_mouseRayLength);
            ToolBase.RaycastOutput raycastOutput;
            if (mouseRayValid && ToolBase.RayCast(input, out raycastOutput))
            {
                if (!m_active)
                {

                    while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        this.m_mouseDirection = cameraDirection;
                        this.m_mousePosition = raycastOutput.m_hitPos;

                    }
                    finally
                    {
                        Monitor.Exit(this.m_dataLock);
                    }

                }
                else
                {
                    while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        if (checkMaxArea(raycastOutput.m_hitPos))
                        {
                            this.m_mousePosition = raycastOutput.m_hitPos;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this.m_dataLock);
                    }
                }

            }
        }

        /// <summary>
        /// Updated to handle Roads, RailRoads, highways, Piles and Power lines
        /// </summary>
        protected void BulldozeRoads()
        {
            segmentsToDelete = new List<ushort>();

            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int gridMinX = Mathf.Max((int)((minX - 16f) / 64f + 135f), 0);
            int gridMinZ = Mathf.Max((int)((minZ - 16f) / 64f + 135f), 0);
            int gridMaxX = Mathf.Min((int)((maxX + 16f) / 64f + 135f), 269);
            int gridMaxZ = Mathf.Min((int)((maxZ + 16f) / 64f + 135f), 269);
            
            for (int i = gridMinZ; i <= gridMaxZ; i++)
            {
                for (int j = gridMinX; j <= gridMaxX; j++)
                {
                    ushort num5 = NetManager.instance.m_segmentGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0u)
                    {
                        var segment = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)];

                        Vector3 position = segment.m_middlePosition;
                        float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));
                        
                        if (positionDiff < 0f && segment.Info.name!= "Airplane Path" && segment.Info.name != "Ship Path")
                        {
                            // we are going to handle Roads, RailRoads, highways, Piles and Power lines
                            if (cbRailroads.isChecked == true && segment.Info.name.Contains("Train") )
                                segmentsToDelete.Add(num5);
                            if (cbRoads.isChecked == true && segment.Info.name.Contains("Road") )
                                segmentsToDelete.Add(num5);
                            if (cbHighways.isChecked == true && segment.Info.name.Contains("Highway") )
                                segmentsToDelete.Add(num5);
                            if (cbPipes.isChecked == true && segment.Info.name.Contains("Pipe"))
                                segmentsToDelete.Add(num5);
                            if (cbPowerLines.isChecked == true && segment.Info.name.Contains("Power"))
                                segmentsToDelete.Add(num5);
                        }
                        num5 = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)].m_nextGridSegment;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            foreach (var segment in segmentsToDelete)
            {
                SimulationManager.instance.AddAction(this.ReleaseSegment(segment));
            }
            NetManager.instance.m_nodesUpdated = true;
        }



        private IEnumerator ReleaseSegment(ushort segment)
        {
            ToolBase.ToolErrors errors = ToolErrors.None;
            if (CheckSegment(segment, ref errors))
            {
                NetManager.instance.ReleaseSegment(segment, false);
            }
            yield return null;
        }


        protected void BulldozeBuildings()
        {
            List<ushort> buildingsToDelete = new List<ushort>();

            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int gridMinX = Mathf.Max((int)((minX - 16f) / 64f + 135f), 0);
            int gridMinZ = Mathf.Max((int)((minZ - 16f) / 64f + 135f), 0);
            int gridMaxX = Mathf.Min((int)((maxX + 16f) / 64f + 135f), 269);
            int gridMaxZ = Mathf.Min((int)((maxZ + 16f) / 64f + 135f), 269);
            
            for (int i = gridMinZ; i <= gridMaxZ; i++)
            {
                for (int j = gridMinX; j <= gridMaxX; j++)
                {
                    ushort num5 = BuildingManager.instance.m_buildingGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0u)
                    {
                        var building = BuildingManager.instance.m_buildings.m_buffer[(int)((UIntPtr)num5)];

                        Vector3 position = building.m_position;
                        float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));
                        if (positionDiff < 0f && building.m_parentBuilding <= 0)
                        {
                            buildingsToDelete.Add(num5);
                        }
                        num5 = BuildingManager.instance.m_buildings.m_buffer[(int)((UIntPtr)num5)].m_nextGridBuilding;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            foreach(ushort building in buildingsToDelete)
            {                
                SimulationManager.instance.AddAction(this.ReleaseBuilding(building));
            }
        }

        private IEnumerator ReleaseBuilding(ushort building)
        {
            BuildingManager.instance.ReleaseBuilding(building);
            yield return null;
        }

        protected void BulldozeTrees()
        {
            List<uint> treesToDelete = new List<uint>();
            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int num = Mathf.Max((int)((minX - 8f) / 32f + 270f), 0);
            int num2 = Mathf.Max((int)((minZ - 8f) / 32f + 270f), 0);
            int num3 = Mathf.Min((int)((maxX + 8f) / 32f + 270f), 539);
            int num4 = Mathf.Min((int)((maxZ + 8f) / 32f + 270f), 539);
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    uint num5 = TreeManager.instance.m_treeGrid[i * 540 + j];
                    int num6 = 0;
                    while (num5 != 0u)
                    {
                        var tree = TreeManager.instance.m_trees.m_buffer[(int)((UIntPtr)num5)];
                        Vector3 position = tree.Position;
                        float num7 = Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max(position.x - maxX - 8f, position.z - maxZ - 8f));
                        if (num7 < 0f)
                        {

                            treesToDelete.Add(num5);
                        }
                        num5 = TreeManager.instance.m_trees.m_buffer[(int)((UIntPtr)num5)].m_nextGridTree;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            foreach (uint tree in treesToDelete)
            {
                TreeManager.instance.ReleaseTree(tree);
            }
            TreeManager.instance.m_treesUpdated = true;

        }


        protected void BulldozeProps()
        {
            List<ushort> propsToDelete = new List<ushort>();
            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int num = Mathf.Max((int)((minX - 16f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((minZ - 16f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((maxX + 16f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((maxZ + 16f) / 64f + 135f), 269);
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = PropManager.instance.m_propGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0u)
                    {
                        var prop = PropManager.instance.m_props.m_buffer[(int)((UIntPtr)num5)];
                        Vector3 position = prop.Position;
                        float num7 = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

                        if (num7 < 0f)
                        {
                            propsToDelete.Add(num5);
                        }
                        num5 = PropManager.instance.m_props.m_buffer[(int)((UIntPtr)num5)].m_nextGridProp;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            foreach (ushort prop in propsToDelete)
            {
                PropManager.instance.ReleaseProp(prop);
            }
           PropManager.instance.m_propsUpdated = true;

        }

        protected void ApplyBulldoze()
        {
            if(cbTrees.isChecked)
                BulldozeTrees();
            try
            {
                if (cbRoads.isChecked || cbRailroads.isChecked || cbHighways.isChecked || cbPipes.isChecked || cbPowerLines.isChecked)
                {
                    BulldozeRoads();
                }
            }
            catch (Exception)
            {
                throw;
            }
                
            if(cbBuildings.isChecked)
                BulldozeBuildings();
            if (cbProps.isChecked)
                BulldozeProps();
        }

        protected override void OnToolGUI(Event e)
        {
            if (e.type == EventType.MouseDown && m_mouseRayValid)
            {
                if (e.button == 0)
                {
                    m_active = true;
                    this.m_startPosition = this.m_mousePosition;
                    this.m_startDirection = Vector3.forward;
                }
                if (e.button == 1)
                {
                    m_active = false;
                }
            }
            else if (e.type == EventType.MouseUp && m_active)
            {
                if (e.button == 0)
                {
                    ApplyBulldoze();
                    m_active = false;
                }
            }
        }
    }
}
