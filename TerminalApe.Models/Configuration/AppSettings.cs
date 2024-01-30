using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.Models.Configuration;

public class AppSettings
{
        public List<string> Exchanges { get; set; } = new List<string>();

        public string DatabaseDirectory { get; set; }
        public string DatabaseFile { get; set; }
        public string DatabaseFilePath { get; set; }

        public int LogAutosaveInterval { get; set; }
        public int LogMaxEntriesBeforeReset { get; set; }
        public string LogDirectory { get; set; }
        public string LogFilePath { get; set; }

        public string Directory { get; set; }
        public string SettingsFile { get; set; }
        public string ExchangeFile { get; set; }
    
    public AppSettings Default()
    {
        AppSettings result = new AppSettings()
        {
            Exchanges = { "binance", "bybit", "coinbase" },
            DatabaseDirectory = "db",
            DatabaseFile = "marketdata.json",
            DatabaseFilePath = "",
            LogAutosaveInterval = 5,
            LogMaxEntriesBeforeReset = 10000,
            LogDirectory = "log",
            LogFilePath = "log/default.log",
            Directory = "settings",
            SettingsFile = "settings.json",
            ExchangeFile = "exchange.json"
        };

        return result;
    }
}

