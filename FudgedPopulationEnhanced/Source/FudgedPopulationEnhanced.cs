using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FudgedPopulationEnhanced.Source
{
    public static class Options
    {
        public static int MultiplierValue;
    }

    
    public class FudgedPopulationEnhanced : IUserMod
    {        
        public string Name
        {
            get { return "SimCity Fudged Population Enhanced"; }
        }

        public string Description
        {
            get
            {
                return "Click on the population icon in game to toggle fudged population vs actual simulated agents population!";
            }

        }

        /*public int PopulationMultiplier
        {
            get { return Options.MultiplierValue; }
            set { this._multiplierValue = value; }
        }*/

        public void OnSettingsUI(UIHelperBase helper)
        {
            var group = helper.AddGroup("Settings");
            var setMultiplier = (UITextField)group.AddTextfield("Population Multiplier", "", (value) => Options.MultiplierValue = Int32.Parse(value));
           
            setMultiplier.tooltip = "Multiply the number of agents (vanilla population) by this number. If zero then original fudged algorithim is used";
        }
    }
    
    public class PopFudgeLoadingExtension : ILoadingExtension
    {
		// Thread: Main
		public void OnCreated(ILoading loading) {}
		// Thread: Main
		public void OnReleased() {}

		public void OnLevelLoaded(LoadMode mode)
		{
            // DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "OnLevelLoaded");
            var uiView = UIView.GetAView();
		    var uiToggleButton = uiView.AddUIComponent(typeof(FudgedPopToggleButton));
		    
		    uiView.AddUIComponent(typeof(PopFieldMask));
            uiView.AddUIComponent(typeof(FudgedPopUiTextField));
		   	
		    uiToggleButton.eventClick += FudgedPopToggleEvent;
		}

        public void OnLevelUnloading()
        {
            
        }
        
        private void FudgedPopToggleEvent(UIComponent component, UIMouseEventParameter eventParam) {
            var uiView = UIView.GetAView();
            var popFieldMask = uiView.FindUIComponent("PopFieldMask");
            var fudgePopulationText = uiView.FindUIComponent("FudgedPopUiTextField");
                        
            if (popFieldMask.opacity <= 0)
            {
                popFieldMask.opacity = 1;
                fudgePopulationText.opacity = 1;
            }
            
            else if (popFieldMask.opacity >= 1)
            {
                popFieldMask.opacity = 0;
                fudgePopulationText.opacity = 0;
            }
        }
	 }
    
    
    public class SkylinesPopulationData
    {
        public int Population { get; set; }
        // public int Unemployed { get; set; }
        // public int Workers    { get; set; }
        // public int Workplaces { get; set; }
    }

    public class FudgedPopToggleButton : UISprite
    {
        public override void Start()
        {
            this.transformPosition = new Vector3(.55f, -0.9460f);
            this.width = 26;
            this.height = 28;
            this.color = new Color32(49,101,99,240);
            this.spriteName = "InfoPanelIconPopulation";
            this.opacity = .2f;
        }
    }

    public class PopFieldMask : UIPanel
    {
        public override void Start()
        {
            this.transformPosition = new Vector3(.57f, -0.93899f);
            this.backgroundSprite = "GenericPanel";
            this.color = new Color32(47,47,47,0);
            this.width = 218;
            this.height = 24;
            this.opacity = 1;
        }
    }
    
    public class FudgedPopUiTextField : UITextField
    {
                
        private SkylinesPopulationData GetPopData()
        {
            var populationInfo = Singleton<PopulationInfoViewPanel>.instance;

            return new SkylinesPopulationData
            {
                Population = populationInfo.population,
                // Unemployed = populationInfo.unemployed,
                // Workers = populationInfo.workers,
                // Workplaces = populationInfo.workplaces
            };
        }

        private int GetFudgedPopulation(int a)
        {
            if (500 >= a)
                return a;
            if (40845 < a)
                return (int)Math.Floor(8.25 * a);
            var b = Math.Pow(a - 500, 1.2) + 500;
            return (int)Math.Floor(b);
        }

        private int GetFudgedPopulationLinear(int a)
        {
            return a * Options.MultiplierValue;
        }

        public override void Start()
        {   
            this.transformPosition = new Vector3(.59f, -0.9460f);
            this.width = 200;
            this.horizontalAlignment = UIHorizontalAlignment.Left;
        }

        public override void Update()
        {
            var popData = GetPopData();
            if (Options.MultiplierValue > 0)
            {
                this.text = GetFudgedPopulationLinear(popData.Population).ToString("n0", CultureInfo.InvariantCulture);
                // DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Populaton Multiplier" + Options.MultiplierValue);
            }
            else
            {
                this.text = GetFudgedPopulation(popData.Population).ToString("n0", CultureInfo.InvariantCulture);

            }    
        }
    }
}