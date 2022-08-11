using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData
{

    /*
     * Sample Json for Authentication.Config file which should be placed in the
     * PnFData folder and set to Copy if Newer.
    {
        "EmailServiceSmptServer": "in-v3.mailjet.com",
        "EmailServiceSmptPort" : 587,
        "EmailServiceUsername": "demo",
        "EmailServicePassword": "demo",
        "AlertServiceEmailFrom": "demo",
        "AlertServiceEmailTo" : "demo",
        "AlphaVantageAPIKey" : "demo",
        "RapidAPIKey" : "demo"
    } 
     */
    public class AuthenticationConfig
    {
        public string EmailServiceSmptServer { get; set; }  = "";
        public int EmailServiceSmptPort { get; set; } = 0;
        public string EmailServiceUsername { get; set; } = "";

        public string EmailServicePassword { get; set; } = "";

        public string AlertServiceEmailFrom { get; set; } = "";

        public string AlertServiceEmailTo { get; set; } = "";

        public string AlphaVantageAPIKey { get; set; } = "";

        public string RapidAPIKey { get; set; } = "";

    }

    public class AuthenticationSettings
    {
        private static AuthenticationSettings _current = null;

        public static AuthenticationSettings Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AuthenticationSettings();
                    _current.Load();
                }
                return _current;
            }
        }

        public AuthenticationConfig? Config { get; private set;} = new AuthenticationConfig();

        private AuthenticationSettings() { }

        private void Load()
        {
            string configFile = Path.Combine(Environment.CurrentDirectory, "Authentication.Config");
            this.Config = JsonConvert.DeserializeObject<AuthenticationConfig>(File.ReadAllText(configFile));
        }
    }

}

