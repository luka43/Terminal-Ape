using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApe.Models.Configuration;

namespace TerminalApe.Services.App;
public class Logger
{
    public int logCounter = 0;
    public AppSettings appSettings = new AppSettings().Default();

    public void Log(dynamic data)
    {
        string stringToLog = data.ToString();

        if (logCounter == 0)
        {
            // If it's the first log line,
            // Initialize log filename for autosave.
            // It will reset every time log box is reset

            logCounter++;
            InitializeLogFolderAndFilename();

            Console.WriteLine($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}");
        }
        // If it's not the first line but also not 10,000 lines
        // Print to the console and auto-save every X lines
        else if (logCounter > 0 && logCounter <= appSettings.LogMaxEntriesBeforeReset)
        {
            // Print to the console
            logCounter++;
            Console.WriteLine($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}");

            // Autosave every 10 lines
            if (logCounter % appSettings.LogAutosaveInterval == 0)
            {
                if (!Directory.Exists(appSettings.LogDirectory)) { Directory.CreateDirectory(appSettings.LogDirectory); }
                System.IO.File.WriteAllText(appSettings.LogFilePath, $"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}");
            }
        }
        // If it's 10,000 lines, reset counter and clear the console
        else
        {
            logCounter = 0;

            Console.WriteLine($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}");
            Console.WriteLine($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - Log file full, creating a new log file.");

            Console.Clear();
        }
    }

    public void InitializeLogFolderAndFilename()
    {
        if (!Directory.Exists(appSettings.LogDirectory)) { Directory.CreateDirectory(appSettings.LogDirectory); }
        appSettings.LogFilePath = Path.Combine(appSettings.LogDirectory, $"{DateTime.Now.ToString("[dd-MM-yy]-[hh-mm-sstt]")}.log");
    }
}