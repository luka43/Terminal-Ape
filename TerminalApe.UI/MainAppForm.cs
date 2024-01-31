using Newtonsoft.Json;
using System.Reflection.Metadata;
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
    public AllControls AllControls;
    private Logger logger;
    private FileDatabase fileDatabase;

    private GetLatency checkConnection;
    private PairService pairService;

    private AppSettings appSettings;
    private Dictionary<string, ExchangeSettings> exchangeSettings;

    private Dictionary<string, dynamic> marketCacheRaw;
    private Dictionary<string, MarketCache> marketCacheOld;
    private Dictionary<string, MarketCache> marketCacheNew;

    private Dictionary<string, MarketCache> newPairsCache;

    private Dictionary<string, bool> exchangeConnectionStatus;

    private bool binanceExchangeStatus;
    private bool bybitExchangeStatus;
    private bool coinbaseExchangeStatus;

    public MainAppForm()
    {
        InitializeComponent();

        AllControls = new AllControls(this);

        Label ImageSignalStatus = (Label)AllControls.GetControlByName("ImageSignalStatus");
        Label LabelInternetConnectionStatus = (Label)AllControls.GetControlByName("LabelInternetConnectionStatus");
        Label BinanceExchangeResponseStatus = (Label)AllControls.GetControlByName($"LabelbinanceExchangeStatus");
        Label BybitExchangeResponseStatus = (Label)AllControls.GetControlByName($"LabelbybitExchangeStatus");
        Label CoinbaseExchangeResponseStatus = (Label)AllControls.GetControlByName($"LabelcoinbaseExchangeStatus");
        appSettings = new AppSettings().Default();
        exchangeSettings = new ExchangeSettings().Default();
        logger = new Logger(this);
        logger.Log("Hi!");
        logger.Log("Initializing application...");
        fileDatabase = new FileDatabase();

        checkConnection = new GetLatency();
        exchangeConnectionStatus = new Dictionary<string, bool>()
        {
            {
                "binance", false
            },
            {
                "bybit", false
            },
            {
                "coinbase", false
            }
        };
        _connectionCheck_Tick(EventArgs.Empty, null);

        pairService = new PairService();

        marketCacheRaw = new Dictionary<string, dynamic>();
        marketCacheOld = new Dictionary<string, MarketCache>();
        marketCacheNew = new Dictionary<string, MarketCache>();

        newPairsCache = new Dictionary<string, MarketCache>();
        
        logger.Log("Loading market database from file...");
        foreach (string exchange in appSettings.Exchanges)
        {
            logger.Log($"Loading {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");
            marketCacheOld.Add(exchange, fileDatabase.LoadExchangeCache("marketCacheOld.json", exchange));
            logger.Log($"{exchange} pairs and symbols added.");
        }
        logger.Log("Getting new market data from servers...");        
        
        foreach (string exchange in appSettings.Exchanges)
        {
            if (exchangeConnectionStatus[exchange])
            {
                GetUpdate(exchange);
            }
            else
            {
                logger.Log("Can't Connect to Binance, please check your connection!");
            }
        }
    }

    private async void GetUpdate(string exchange)
    {
        logger.Log($"Getting new {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");
        marketCacheRaw.Add(exchange, await pairService.GetPairs(exchange));
        logger.Log("Done.");

        logger.Log($"Sorting new {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");
        marketCacheNew.Add(exchange, pairService.SortPairs(exchange, marketCacheRaw[exchange].result));
        logger.Log("Done.");

        logger.Log($"Comparing new {char.ToUpper(exchange[0]) + exchange.Substring(1)}data with old data...");
        newPairsCache.Add(exchange, pairService.Compare(exchange, marketCacheOld[exchange], marketCacheNew[exchange]));
        logger.Log($"Done.");

        if (marketCacheRaw[exchange].result != null)
        {
            UpdateExchangePairsUI(exchange);
        }
    }
    private void UpdateExchangePairsUI(string exchange)
    {
        if (marketCacheRaw[exchange].result != null)
        {
            Label ExchangeResponseStatus = (Label)AllControls.GetControlByName($"Label{exchange}ExchangeStatus");
            Label ExchangeTradingPairs = (Label)AllControls.GetControlByName($"{exchange}NewCacheTradingPairs");
            Label ExchangeHaltedPairs = (Label)AllControls.GetControlByName($"{exchange}NewCacheHaltedPairs");
            Label ExchangeAllPairs = (Label)AllControls.GetControlByName($"{exchange}NewCacheAllPairs");
            Label ExchangeTradingBases = (Label)AllControls.GetControlByName($"{exchange}NewCacheTradingBases");
            Label ExchangeTradingQuotes = (Label)AllControls.GetControlByName($"{exchange}NewCacheTradingQuotes");
            Label ExchangeHaltedBases = (Label)AllControls.GetControlByName($"{exchange}NewCacheHaltedBases");
            Label ExchangeHaltedQuotes = (Label)AllControls.GetControlByName($"{exchange}NewCacheHaltedQuotes");

            ExchangeResponseStatus.ForeColor = Color.Green;
            ExchangeResponseStatus.Text = "Online";

            ExchangeTradingPairs.Text = marketCacheNew[exchange].TradingPairs.Keys.Count.ToString();

            ExchangeHaltedPairs.Text = marketCacheNew[exchange].HaltedPairs.Keys.Count.ToString();
            ExchangeAllPairs.Text = marketCacheNew[exchange].AllPairs.Count.ToString();

            ExchangeTradingBases.Text = marketCacheNew[exchange].TradingBases.Keys.Count.ToString();
            ExchangeTradingQuotes.Text = marketCacheNew[exchange].TradingQuotes.Keys.Count.ToString();
            ExchangeHaltedBases.Text = marketCacheNew[exchange].HaltedBases.Keys.Count.ToString();
            ExchangeHaltedQuotes.Text = marketCacheNew[exchange].HaltedQuotes.Keys.Count.ToString();


            Label CacheTradingPairs = (Label)AllControls.GetControlByName($"{exchange}OldCacheTradingPairs");
            Label CacheHaltedPairs = (Label)AllControls.GetControlByName($"{exchange}OldCacheHaltedPairs");
            Label CacheAllPairs = (Label)AllControls.GetControlByName($"{exchange}OldCacheAllPairs");
            Label CacheTradingBases = (Label)AllControls.GetControlByName($"{exchange}OldCacheTradingBases");
            Label CacheTradingQuotes = (Label)AllControls.GetControlByName($"{exchange}OldCacheTradingQuotes");
            Label CacheHaltedBases = (Label)AllControls.GetControlByName($"{exchange}OldCacheHaltedBases");
            Label CacheHaltedQuotes = (Label)AllControls.GetControlByName($"{exchange}OldCacheHaltedQuotes");

            CacheTradingPairs.Text = marketCacheOld[exchange].TradingPairs.Keys.Count.ToString();

            CacheHaltedPairs.Text = marketCacheOld[exchange].HaltedPairs.Keys.Count.ToString();
            CacheAllPairs.Text = marketCacheOld[exchange].AllPairs.Count.ToString();

            CacheTradingBases.Text = marketCacheOld[exchange].TradingBases.Keys.Count.ToString();
            CacheTradingQuotes.Text = marketCacheOld[exchange].TradingQuotes.Keys.Count.ToString();
            CacheHaltedBases.Text = marketCacheOld[exchange].HaltedBases.Keys.Count.ToString();
            CacheHaltedQuotes.Text = marketCacheOld[exchange].HaltedQuotes.Keys.Count.ToString();

            Label CacheNewPairs = (Label)AllControls.GetControlByName($"{exchange}NewPairs");

            CacheNewPairs.Text = newPairsCache[exchange].AllPairs.Count.ToString();
        }
    }
    private void _connectionCheck_Tick(object sender, EventArgs e)
    {
        CheckGoogleDnsLatency();
        foreach (string exchange in appSettings.Exchanges)
        {
            CheckExchangeLatency(exchange);
        }
    }
    private async void CheckGoogleDnsLatency()
    {
        long googleLatency = await checkConnection.GoogleDNS();

        if (googleLatency > 300 && googleLatency < 501)
        {
            ImageSignalStatus.Image = Properties.Resources.signal_verypoor;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 200 && googleLatency < 301)
        {
            ImageSignalStatus.Image = Properties.Resources.signal_poor;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 100 && googleLatency < 201)
        {
            ImageSignalStatus.Image = Properties.Resources.signal_fair;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 50 && googleLatency < 101)
        {
            ImageSignalStatus.Image = Properties.Resources.signal_good;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 0 && googleLatency < 51)
        {
            ImageSignalStatus.Image = Properties.Resources.signal_full;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else
        {
            ImageSignalStatus.Image = Properties.Resources.signal_empty;
            LabelInternetConnectionStatus.Text = "Offline";
        }
    }
    private async void CheckExchangeLatency(string exchange)
    {
        long latency = await checkConnection.ExchangeAPI(exchange);
        if (latency < 501)
        {
            exchangeConnectionStatus[exchange] = true;
            Label ExchangeResponseStatus = (Label)AllControls.GetControlByName($"Label{exchange}ExchangeStatus");
            ExchangeResponseStatus.ForeColor = Color.Green;
            ExchangeResponseStatus.Text = "Online";
        }
        else
        {
            exchangeConnectionStatus[exchange] = false;
            Label ExchangeResponseStatus = (Label)AllControls.GetControlByName($"Label{exchange}ExchangeStatus");
            ExchangeResponseStatus.ForeColor = Color.Red;
            ExchangeResponseStatus.Text = "Online";
        }
    }
}
