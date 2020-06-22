using NLog;
using System;

namespace Common
{
    public class NLogConfigManager
    {
        public static void CreateFileConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var currentDate = DateTime.Now.ToString("dd.MM-HH.mm");
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "C:\\Users\\sebod\\Desktop\\randoop_output\\log" + currentDate + ".txt" };

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }
    }
}
