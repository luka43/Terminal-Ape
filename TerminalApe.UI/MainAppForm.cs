using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

using TerminalApe.DAL;
using TerminalApe.Models.Configuration;
using TerminalApe.Models.Exchange;
using TerminalApe.Services.Connection;
using TerminalApe.Services.Exchanges;
using TerminalApe.UI.Services;

namespace TerminalApe.UI;

public partial class MainAppForm : Form
{
    private Timers _timer;
    public AllControls AllControls { get; private set; }
    public UpdateUi UpdateUi { get; private set; }
    private Logger logger;
    private FileDatabase fileDatabase;
    private PairService pairService;

    private AppSettings appSettings;
    private Dictionary<string, ExchangeSettings> exchangeSettings;

    private Dictionary<string, dynamic> marketCacheRaw;
    private Dictionary<string, MarketCache> marketCacheOld;
    private Dictionary<string, MarketCache> marketCacheNew;
    private Dictionary<string, MarketCache> newPairsCache;

    public MainAppForm()
    {
        InitializeComponent();
        logger = new Logger(this);
        logger.Log("Hi!");
        logger.Log("Initializing application...");

        InitializeServices();
        InitializeSettings();
        InitializeMarketCache();
        InitializeTimers();

        logger.Log("Loading market database from file...");

        foreach (string exchange in appSettings.Exchanges)
        {
            logger.Log($"Loading {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");

            marketCacheOld.Add(exchange, fileDatabase.LoadExchangeCache("marketCacheOld.json", exchange));

            logger.Log($"Found {marketCacheOld[exchange].AllPairs.Count} pairs in {char.ToUpper(exchange[0]) + exchange.Substring(1)} database.");
            logger.Log($"{char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols added.");
        }

        logger.Log("Getting new market data from servers...");

        foreach (string exchange in appSettings.Exchanges)
        {
            GetUpdate(exchange);
        }
    }
    private void InitializeMarketCache()
    {
        marketCacheRaw = new Dictionary<string, dynamic>();
        marketCacheOld = new Dictionary<string, MarketCache>();
        marketCacheNew = new Dictionary<string, MarketCache>();
        newPairsCache = new Dictionary<string, MarketCache>();
    }
    private void InitializeSettings()
    {
        appSettings = new AppSettings().Default();
        exchangeSettings = new ExchangeSettings().Default();
    }
    private void InitializeServices()
    {
        AllControls = new AllControls(this);
        UpdateUi = new UpdateUi(this);
        fileDatabase = new FileDatabase();
        pairService = new PairService();
    }
    private void InitializeTimers()
    {
        _timer = new Timers(this);
    }

    private async void GetUpdate(string exchange)
    {
        logger.Log($"Getting new {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");

        marketCacheRaw.Add(exchange, await pairService.GetPairs(exchange));

        logger.Log("Done.");
        logger.Log($"Sorting new {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");

        marketCacheNew.Add(exchange, pairService.SortPairs(exchange, marketCacheRaw[exchange].result));

        logger.Log($"Got {marketCacheNew[exchange].AllPairs.Count} pairs in {char.ToUpper(exchange[0]) + exchange.Substring(1)} database.");
        logger.Log("Done.");
        logger.Log($"Comparing new {char.ToUpper(exchange[0]) + exchange.Substring(1)}data with old data...");

        newPairsCache.Add(exchange, pairService.Compare(exchange, marketCacheOld[exchange], marketCacheNew[exchange]));

        logger.Log($"Got {newPairsCache[exchange].AllPairs.Count} pairs in {char.ToUpper(exchange[0]) + exchange.Substring(1)} database : ");
        logger.Log($"[{string.Join(", ", newPairsCache[exchange].AllPairs)}]");
        logger.Log($"Done.");

        if (marketCacheRaw[exchange].result != null)
        {
            UpdateUi.ExchangePairsAndSymbols(exchange, marketCacheOld[exchange], marketCacheNew[exchange], newPairsCache[exchange]);
            UpdateUi.CreateOrderUI(marketCacheNew[exchange]);
        }       
    }
    private void LogBoxScrolLToBottom(object sender, EventArgs e)
    {
        logBox.SelectionStart = logBox.Text.Length;
        logBox.ScrollToCaret();
    }
}
