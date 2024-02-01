using TerminalApe.Models.Configuration;
using TerminalApe.Services.Connection;
using TerminalApe.UI.Services;

namespace TerminalApe.UI.Services;

public class Timers
{
    private UpdateUi updateUi;
    private AppSettings appSettings;
    private System.Windows.Forms.Timer connectionCheckTimer;

    public Timers(MainAppForm form)
    {
        updateUi = new UpdateUi(form);
        appSettings = new AppSettings().Default();

        connectionCheckTimer = new System.Windows.Forms.Timer();
        connectionCheckTimer.Interval = 1500;
        connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
        connectionCheckTimer.Start();
    }

    private void ConnectionCheckTimer_Tick(object sender, EventArgs e)
    {
        updateUi.InternetLatencyStatus();
        
        foreach (string exchange in appSettings.Exchanges)
        {
            updateUi.ExchangeLatencyStatus(exchange);
        }        
    }
}
