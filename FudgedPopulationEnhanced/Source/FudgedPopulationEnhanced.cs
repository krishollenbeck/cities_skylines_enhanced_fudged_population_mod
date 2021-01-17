using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.IO;
using System.Globalization;

namespace FudgedPopulationEnhanced.Source
{
    public class Options
    {
        public int Index;
        public int LinearValue;
        public int FudgedStartValue;

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public class Data
   {
        public static string GetJsonString()
        {
            string path = @"FudgedPopulationEnhancedConfig.json";
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            return "";
        }

        public static void SaveOptions(Options options)
        {
            string file = @"FudgedPopulationEnhancedConfig.json";
            string jsonString = options.SaveToString();
            File.WriteAllText(file, jsonString);
        }

        public static int GetIndex()
        {
            if (Data.FileExists())
            {
                // Debug.Log("Get Index");
                string savedData = Data.GetJsonString();
                var json = JsonUtility.FromJson<Options>(savedData);

                return Int32.Parse(json.Index.ToString());
            }


            return 0;
        }

        public static int GetSavedLinearData() 
        {
            if (Data.FileExists())
            {
                string savedData = Data.GetJsonString();
                var json = JsonUtility.FromJson<Options>(savedData);

                return Int32.Parse(json.LinearValue.ToString());
            }

            return 0;
        }

        public static int GetSavedStartingFudgeValue()
        {
            if (Data.FileExists())
            {
                string savedData = Data.GetJsonString();
                var json = JsonUtility.FromJson<Options>(savedData);

                return Int32.Parse(json.FudgedStartValue.ToString());
            }

            return 0;
        }

        public static bool FileExists() 
        {
            string path = @"FudgedPopulationEnhancedConfig.json";
            return File.Exists(path);
        }
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

        public void OnSettingsUI(UIHelperBase helper)
        {
            Options options = new Options();
            string placeholderValue = "";
            int initialSelection = 0;

            if (Data.GetIndex() == 0)
            {
                options.FudgedStartValue = Data.GetSavedStartingFudgeValue();
                placeholderValue = Data.GetSavedStartingFudgeValue().ToString();
                initialSelection = 0;

                // Debug.Log("Set Fudged");
            }

            if (Data.GetIndex() == 1)
            {
                options.LinearValue = Data.GetSavedLinearData();
                placeholderValue = Data.GetSavedLinearData().ToString();
                initialSelection = 1;

                // Debug.Log("Set Linear");
            }

            // Debug.Log("initial selection " + initialSelection);
            // Debug.Log("placeholder " + placeholderValue);

            var group = helper.AddGroup("Settings");

            string[] selections = new string[] { "Fudge Start Value", "Linear" };
            var uIDropDown = (UIDropDown)group.AddDropdown("Select One", selections, initialSelection, (v) =>
            {
                options.Index = v;

                // Debug.Log("Selection made -> " + v);
                Data.SaveOptions(options);
            });

            var uiTextField = (UITextField)group.AddTextfield("Value", placeholderValue, (v) => {
                if (Data.GetIndex() == 0)
                {
                    options.FudgedStartValue = String.IsNullOrEmpty(v.ToString()) ? 0 : Int32.Parse(v);
                    options.LinearValue = 0;
                }

                if (Data.GetIndex() == 1)
                {
                    options.LinearValue = String.IsNullOrEmpty(v.ToString()) ? 0 : Int32.Parse(v);
                    options.FudgedStartValue = 0;
                }

                Data.SaveOptions(options);
            });

            uIDropDown.tooltip = @"-INFO-
                LINEAR: Multiply the number of agents (vanilla population) by this number. If zero then original fudged algorithm is used
                FUDGE START: The starting population value for at what point the numbers will be fudged. For example default fudge is 500. Max value is 40845. If greater than 40845, value will default to 500";

            var submitButton = (UIButton)group.AddButton("Save Changes", () =>
            {
                Data.SaveOptions(options);
            });
        }
    }
    
    public class PopFudgeLoadingExtension : ILoadingExtension
    {
		// Thread: Main
		public void OnCreated(ILoading loading) 
        {
            string path = @"FudgedPopulationEnhancedConfig.json";
            if (!File.Exists(path))
            {
                // Debug.Log("FudgedPopulationEnhancedConfig.json does not exist");
                Options options = new Options();
                options.FudgedStartValue = 0;
                options.Index = 0;
                options.LinearValue = 0;

                Data.SaveOptions(options);
            }
        }

		// Thread: Main
		public void OnReleased() {}

		public void OnLevelLoaded(LoadMode mode)
		{
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
        private int GetFudgedPopulation(int currentPopulation)
        {
            string savedData = Data.GetJsonString();
            var json = JsonUtility.FromJson<Options>(savedData);

            int endValue = 40845;
            int startingFudgeValue = json.FudgedStartValue != 0 && json.FudgedStartValue < endValue ? json.FudgedStartValue : 500;

            // Debug.Log(startingFudgeValue.ToString());

            if (startingFudgeValue >= currentPopulation)
            {
                return currentPopulation;
            }
                
            if (endValue < currentPopulation)
            {
                return (int)Math.Floor(8.25 * currentPopulation);
            }
                
            var b = Math.Pow(currentPopulation - 500, 1.2) + 500;
            int fudgedPopulation = (int)Math.Floor(b);

            return fudgedPopulation;
        }

        private int GetFudgedPopulationLinear(int a)
        {
            string savedData = Data.GetJsonString();
            var json = JsonUtility.FromJson<Options>(savedData);

            int linearPopulation = json.LinearValue > 0 ? Data.GetSavedLinearData() : json.LinearValue;

            return a * linearPopulation;
        }

        public override void Start()
        {
            this.transformPosition = new Vector3(.59f, -0.9460f);
            this.width = 200;
            this.horizontalAlignment = UIHorizontalAlignment.Left;
        }

        public override void Update()
        {
            PopulationInfoViewPanel panel = new PopulationInfoViewPanel();
            int population = panel.population;

            // Debug.Log("Current index is " + Data.GetIndex());

            if (Data.GetIndex() == 0)
            {
                // Debug.Log("Update Fudged Data");
                // Updated UI to use default fudged population logic
                this.text = GetFudgedPopulation(population).ToString("n0", CultureInfo.InvariantCulture);
            }
            
            if (Data.GetIndex() == 1)
            {
                // Debug.Log("Update Linear Data");
                // Update UI value in game
                this.text = GetFudgedPopulationLinear(population).ToString("n0", CultureInfo.InvariantCulture);
            }
        }
    }
}