using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApe.Models.Configuration;

namespace TerminalApe.UI.Services;

public class Logger
{
    private MainAppForm mainAppForm;
    private RichTextBox logBox;

    private AppSettings appSettings;

    private string stringToLog;
    private int logCounter = 0;

    public Logger(MainAppForm form)
    {
        mainAppForm = form;
        logBox = (RichTextBox)mainAppForm.Controls["PanelBottom"].Controls["PanelConsole"].Controls["groupBoxConsole"].Controls["logBox"];
        appSettings = new AppSettings().Default();
    }

    public void Log(dynamic data)
    {
        stringToLog = data.ToString();

        // If it's first log line,
        // Initialize log filename for autosave.
        // It will reset everytime logbox is reset
        if (logCounter == 0)
        {
            logCounter++;
            InitializeLogFolderAndFilename();

            logBox.AppendText($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}\r\n");
        }
        // If it's not first line but also not 10.000 line
        // Print to logBox and auto save every X lines
        else if (logCounter > 0 && logCounter <= appSettings.LogMaxEntriesBeforeReset)
        {
            // Print to logbox and scroll down
            logCounter++;
            logBox.AppendText($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}\r\n");

            // Autosave every 10 lines
            if (logCounter % appSettings.LogAutosaveInterval == 0)
            {
                if (!Directory.Exists(appSettings.LogDirectory)) { Directory.CreateDirectory(appSettings.LogDirectory); }
                System.IO.File.WriteAllText(appSettings.LogFilePath, logBox.Text.ToString());
            }
        }
        // If it's 10.000 line reset counter and clear logbox
        else
        {
            logCounter = 0;

            logBox.AppendText($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}\r\n");
            logBox.AppendText($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - Log file full, creating new log file.\r\n");

            logBox.Clear();
        }
    }
    public void InitializeLogFolderAndFilename()
    {
        if (!Directory.Exists(appSettings.LogDirectory)) { Directory.CreateDirectory(appSettings.LogDirectory); }
        appSettings.LogFilePath = Path.Combine(appSettings.LogDirectory, $"{DateTime.Now.ToString("[dd-MM-yy]-[hh-mm-sstt]")}.log");
    }

}
