using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace YH.ICMS.Common
{

    public class ConfigHelper:MarshalByRefObject
    {

        static public Configuration OpenCustomConfigFile()
        {
            return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
            {
                ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory + "DialysisTreatmentInterface\\" + "custom.config"
            }, ConfigurationUserLevel.None);
        }

        static public string GetConfigValue(Configuration configuration, string key)
        {
            return configuration.AppSettings.Settings[key]?.Value;
        }

        static public void SetConfigValue(Configuration configuration, string key, string value)
        {
            if (configuration.AppSettings.Settings[key] == null)
            {
                configuration.AppSettings.Settings.Add(key, value);
            }
            else
            {
                configuration.AppSettings.Settings[key].Value = value;
            }
        }

        static public void SaveAllConfigValue(Configuration configuration)
        {
            configuration.Save(ConfigurationSaveMode.Modified);
        }

        static public string ReadConfig(string str)
        {
            //Console.Write("调用DllConfigFile.ReadConfig()得到：");
            return ConfigurationManager.AppSettings[str];
        }

        static public  string WriteConfig(string key,string val)
        {
            ExeConfigurationFileMap file = new ExeConfigurationFileMap();
           file.ExeConfigFilename = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "App.config";
           Configuration config = ConfigurationManager.OpenMappedExeConfiguration(file, ConfigurationUserLevel.None);

            var myApp = (AppSettingsSection)config.GetSection("appSettings");
            myApp.Settings[key].Value = val;
            config.Save();
            return val;

        }


        public static String GetConfigFile()
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "MSConfig.xml";
            return path;
        }

        

    }
}
