using TerminalApe.DAL;
using TerminalApe.Models.Configuration;
using TerminalApe.Models.Exchange;
using TerminalApe.Services.Connection;
using TerminalApe.Services.Exchanges;
using TerminalApe.UI.Services;

namespace TerminalApe.UI.Services;

public class Timers
{
    private FileDatabase fileDatabase;
    private PairService pairService;
    private UpdateUi updateUi;

    private AppSettings appSettings { get; set; } = new AppSettings().Default();
    private Dictionary<string, ExchangeSettings> exchangeSettings { get; set; } = new ExchangeSettings().Default();
    private MarketCache oldMarketCache { get; set; } = new MarketCache();
    private MarketCache newMarketCache { get; set; } = new MarketCache();
    private MarketCache newPairsCache { get; set; } = new MarketCache();
    private dynamic rawMarketCache { get; set; }


    public System.Windows.Forms.Timer connectionCheckTimer { get; set; } = new System.Windows.Forms.Timer();
    public Dictionary<string, System.Windows.Forms.Timer> exchangeTimers { get; set; } = new Dictionary<string, System.Windows.Forms.Timer>();


    public Timers(MainAppForm form)
    {
        fileDatabase = new FileDatabase();
        pairService = new PairService();
        updateUi = new UpdateUi(form);

        connectionCheckTimer.Interval = 1500;
        connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
        connectionCheckTimer.Start();

        exchangeTimers.Clear();

        foreach (string exchange in appSettings.Exchanges)
        {
            exchangeTimers[exchange] = new System.Windows.Forms.Timer();
            exchangeTimers[exchange].Interval = exchangeSettings[exchange].TIMEOUT;
            exchangeTimers[exchange].Tick += ApeTimer_Tick;
            exchangeTimers[exchange].Start();
        }
    }

    private void ConnectionCheckTimer_Tick(object sender, EventArgs e)
    {
        updateUi.InternetLatencyStatus();
        
        foreach (string exchange in appSettings.Exchanges)
        {
            updateUi.ExchangeLatencyStatus(exchange);
        }        
    }
    private void ApeTimer_Tick(object sender, EventArgs e)
    {
        foreach(string exchange in appSettings.Exchanges)
        {
            ApeTimerService(exchange);
        }
    }
    private async void ApeTimerService(string exchange)
    {
        oldMarketCache = fileDatabase.LoadExchangeCache(exchange, Path.Combine(appSettings.DatabaseDirectory, appSettings.DatabaseFile));
        rawMarketCache = await pairService.GetPairs(exchange);
        newMarketCache = pairService.SortPairs(exchange, rawMarketCache.result);
        newPairsCache = pairService.Compare(exchange, oldMarketCache, newMarketCache);

        updateUi.ExchangePairsAndSymbols(exchange, oldMarketCache, newMarketCache, newPairsCache);
    }
}
