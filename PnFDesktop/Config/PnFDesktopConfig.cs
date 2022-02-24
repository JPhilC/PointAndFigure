using System;
using System.IO;
using System.Reflection;

namespace PnFDesktop.Config
{

    public class PnFDesktopConfig
    {
        private static string _InstallConfigPath = null;
        public static string InstallConfigPath
        {
            get
            {
                if (_InstallConfigPath == null)
                {
                    //if (ApplicationDeployment.IsNetworkDeployed) {
                    _InstallConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config");
                    //}
                    //else {
                    //   _BasePath = System.AppDomain.CurrentDomain.BaseDirectory;
                    //}
                }
                return _InstallConfigPath;
            }
        }

        private static string _ProductName = null;
        public static string ProductName
        {
            get
            {
                if (_ProductName == null)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

                    AssemblyProductAttribute attribute = null;
                    if (attributes.Length > 0)
                    {
                        attribute = attributes[0] as AssemblyProductAttribute;
                        _ProductName = attribute.Product;
                    }
                    else
                    {
                        _ProductName = "Unknown";
                    }
                }
                return _ProductName;
            }
        }

        private static string _VersionBuild = null;
        public static string VersionBuild
        {
            get
            {
                if (_VersionBuild == null)
                {
                    Version version = Assembly.GetExecutingAssembly().GetName().Version;
                    _VersionBuild = version.Major.ToString() + "_" + version.Minor.ToString();
                }
                return _VersionBuild;
            }
        }


        public static string UserSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnitySoftware",
           PnFDesktopConfig.ProductName, PnFDesktopConfig.VersionBuild);
        public static string ErrorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnitySoftware",
           PnFDesktopConfig.ProductName, PnFDesktopConfig.VersionBuild, "Errors");

        static PnFDesktopConfig()
        {
            // When the application is ClickOnce deployed the config files are initially in
            // a read-only Config folder below the install location so they need to be copied into
            // the user data folder.
            InitialiseUserConfig();
        }

        private PnFDesktopConfig()
        {
        }

        private static void InitialiseUserConfig()
        {
            // Check the user settings location exists
            if (!Directory.Exists(PnFDesktopConfig.UserSettingsPath))
            {
                Directory.CreateDirectory(PnFDesktopConfig.UserSettingsPath);
            }

            if (!Directory.Exists(PnFDesktopConfig.ErrorLogPath))
            {
                Directory.CreateDirectory(PnFDesktopConfig.ErrorLogPath);
            }


            // ONLY REFRESH IF MISSING.
            string[] ifMissingFiles = new string[] { "default.layout" };
            foreach (string file in ifMissingFiles)
            {
                if (!File.Exists(Path.Combine(PnFDesktopConfig.UserSettingsPath, file)))
                {
                    string sourceFile = Path.Combine(PnFDesktopConfig.InstallConfigPath, file);
                    string targetFile = Path.Combine(PnFDesktopConfig.UserSettingsPath, file);
                    File.Copy(sourceFile, targetFile);
                }
            }
        }
    }
}
