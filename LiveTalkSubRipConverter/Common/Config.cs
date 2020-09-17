/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：Config
 * 概要      ：Config
*/
using System.Configuration;

namespace LiveTalkSubRipConverter.Common
{
    internal static class Config
    {
        private static Configuration ConfigManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        internal static void SetConfig(string name, string value)
        {
            ConfigManager.AppSettings.Settings[name].Value = value;
            ConfigManager.Save();
        }

        internal static string GetConfig(string name)
        {
            return ConfigManager.AppSettings.Settings[name].Value;
        }
    }
}
