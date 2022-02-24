using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PnFDesktop.Config;

namespace PnFDesktop.Classes
{
    public class FormLayoutManager
    {
        [Serializable]
        public class FormParameters
        {
            public bool HasValues
            {
                get
                {
                    return (Top.HasValue && Left.HasValue && Height.HasValue && Width.HasValue && WindowState.HasValue);
                }
            }
            public string FormTitle { get; set; }
            public double? Top { get; set; }
            public double? Left { get; set; }
            public double? Height { get; set; }
            public double? Width { get; set; }
            public WindowState? WindowState { get; set; }

            public FormParameters() { }

            public FormParameters(string formTitle, double top, double left, double height, double width, WindowState state)
            {
                this.FormTitle = formTitle;
                this.Top = top;
                this.Left = left;
                this.Height = height;
                this.Width = width;
                this.WindowState = state;
            }
        }

        private string formPositions = Path.Combine(PnFDesktopConfig.UserSettingsPath, "formlayouts.user");
        private List<FormParameters> _Settings = new List<FormParameters>();

        private static FormLayoutManager _Current = null;
        public static FormLayoutManager Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new FormLayoutManager();
                }
                return _Current;
            }
        }

        private JsonSerializerSettings _JsonSettings;

        // Private constructor to prevent multiple instances being created
        private FormLayoutManager()
        {
            _JsonSettings = new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                DateParseHandling = DateParseHandling.DateTime,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        public void SaveSettings(System.Windows.Window window, string settingsKey = "")
        {
            if (!ApplicationOptionsManager.Options.SaveFormPositions)
            {
                return;
            }
            string titleToUse = (settingsKey != "" ? settingsKey : window.Title);
            FormParameters parameters = _Settings.Where(s => s.FormTitle == titleToUse).FirstOrDefault();
            if (parameters != null)
            {
                parameters.Top = window.Top;
                parameters.Left = window.Left;
                parameters.Height = window.Height;
                parameters.Width = window.Width;
                parameters.WindowState = window.WindowState;

            }
            else
            {
                _Settings.Add(new FormParameters(
                   titleToUse,
                   window.Top,
                   window.Left,
                   window.Height,
                   window.Width,
                   window.WindowState));
            }
        }


        public void RestoreSettings(System.Windows.Window window, string settingsKey = "")
        {
            if (!ApplicationOptionsManager.Options.SaveFormPositions)
            {
                return;
            }
            string titleToUse = (settingsKey != "" ? settingsKey : window.Title);
            FormParameters parameters = _Settings.Where(s => s.FormTitle == titleToUse).FirstOrDefault();
            if (parameters != null)
            {
                if (parameters.HasValues)
                {
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Top = parameters.Top.Value;
                    window.Left = parameters.Left.Value;
                    window.Height = parameters.Height.Value;
                    window.Width = parameters.Width.Value;
                    window.WindowState = parameters.WindowState.Value;
                }
            }
        }

        /// <summary>
        /// Saves the current settings to user storage
        /// </summary>
        public void Save()
        {
            using (StreamWriter file = new StreamWriter(formPositions, false, Encoding.ASCII))
            {
                string json = JsonConvert.SerializeObject(_Settings, _JsonSettings);
                file.Write(json);
            }
        }



        public void Load()
        {
            if (File.Exists(formPositions))
            {
                using (StreamReader reader = File.OpenText(formPositions))
                {
                    string json = reader.ReadToEnd();
                    _Settings = JsonConvert.DeserializeObject<List<FormParameters>>(json, _JsonSettings);
                }
            }
        }
    }
}
