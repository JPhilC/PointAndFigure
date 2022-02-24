using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using PnFDesktop.Config;

namespace PnFDesktop.Classes
{
    public class ApplicationOptionsManager
    {

        [Serializable]
        public class ApplicationOptions : ObservableObject
        {
            #region Cart surface preferences ...
            private double _defaultSheetWidth = double.NaN;

            /// <summary>
            /// Sets and gets the DefaultSheetWidth property.
            /// Changes to that property's value raise the PropertyChanged event. 
            /// </summary>
            public double DefaultSheetWidth
            {
                get => _defaultSheetWidth;
                set => SetProperty(ref _defaultSheetWidth, value);
            }

            private double _defaultSheetHeight = double.NaN;

            /// <summary>
            /// Sets and gets the DefaultSheetHeight property.
            /// Changes to that property's value raise the PropertyChanged event. 
            /// </summary>
            public double DefaultSheetHeight
            {
                get => _defaultSheetHeight;
                set => SetProperty(ref _defaultSheetHeight, value);
            }


            private bool _saveFormPositions = true;

            /// <summary>
            /// Sets and gets the SaveFormsPosition property.
            /// Changes to that property's value raise the PropertyChanged event. 
            /// </summary>
            public bool SaveFormPositions
            {
                get => _saveFormPositions;
                set => SetProperty(ref _saveFormPositions, value);
            }

            #endregion

            public ApplicationOptions()
            {
                // --- Design surface options --- //
                DefaultSheetWidth = 3000;
                DefaultSheetHeight = 2000;

            }

        }

        private JsonSerializerSettings _jsonSettings;

        private string _optionsFile = Path.Combine(PnFDesktopConfig.UserSettingsPath, "options.user");
        private ApplicationOptions _options = new ApplicationOptions();

        private static ApplicationOptionsManager _currentManager = null;
        private static ApplicationOptionsManager CurrentManager
        {
            get
            {
                if (_currentManager == null)
                {
                    _currentManager = new ApplicationOptionsManager();
                }
                return _currentManager;
            }
        }

        private ApplicationOptionsManager()
        {
            _jsonSettings = new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                DateParseHandling = DateParseHandling.DateTime,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            Load();
        }

        /// <summary>
        /// Static property giving access to the current options.
        /// </summary>
        public static ApplicationOptions Options
        {
            get
            {
                return ApplicationOptionsManager.CurrentManager._options;
            }
        }

        /// <summary>
        /// Static method to save the current options.
        /// </summary>
        public static void SaveOptions()
        {
            ApplicationOptionsManager.CurrentManager.Save();
        }

        /// <summary>
        /// Saves the current settings to user storage
        /// </summary>
        private void Save()
        {
            using (StreamWriter file = new StreamWriter(_optionsFile, false, Encoding.ASCII))
            {
                string json = JsonConvert.SerializeObject(_options, _jsonSettings);
                file.Write(json);
            }
        }

        /// <summary>
        /// Loads any previously saved settings
        /// </summary>
        private void Load()
        {
            if (File.Exists(_optionsFile))
            {
                using (StreamReader reader = File.OpenText(_optionsFile))
                {
                    string json = reader.ReadToEnd();
                    _options = JsonConvert.DeserializeObject<ApplicationOptions>(json, _jsonSettings);
                }
            }
        }



    }
}
