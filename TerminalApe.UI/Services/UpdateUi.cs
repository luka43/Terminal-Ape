using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalApe.Models.Exchange;
using TerminalApe.Models.Configuration;
using TerminalApe.Services.Connection;

namespace TerminalApe.UI.Services;

public class UpdateUi
{
    private GetLatency getLatency;
    private MainAppForm mainAppForm;
    private AllControls allControls;
    private Label ImageSignalStatus;
    private Label LabelInternetConnectionStatus;
    private ComboBox DropdownBase;
    private ComboBox DropdownQuote;

    public UpdateUi(MainAppForm form)
    {
        mainAppForm = form;
        getLatency = new GetLatency();

        allControls = new AllControls(mainAppForm);
        ImageSignalStatus = (Label)allControls.GetControlByName("ImageSignalStatus");
        LabelInternetConnectionStatus = (Label)allControls.GetControlByName("LabelInternetConnectionStatus");
        DropdownBase = (ComboBox)allControls.GetControlByName("DropdownBase");
        DropdownQuote = (ComboBox)allControls.GetControlByName("DropdownQuote");
    }

    public async void InternetLatencyStatus()
    {
        long googleLatency = await getLatency.GoogleDNS();

        if (googleLatency > 300 && googleLatency < 501)
        {
            ImageSignalStatus.Image = TerminalApe.UI.Properties.Resources.signal_verypoor;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 200 && googleLatency < 301)
        {
            ImageSignalStatus.Image = TerminalApe.UI.Properties.Resources.signal_poor;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 100 && googleLatency < 201)
        {
            ImageSignalStatus.Image = TerminalApe.UI.Properties.Resources.signal_fair;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 50 && googleLatency < 101)
        {
            ImageSignalStatus.Image = TerminalApe.UI.Properties.Resources.signal_good;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else if (googleLatency > 0 && googleLatency < 51)
        {
            ImageSignalStatus.Image = TerminalApe.UI.Properties.Resources.signal_full;
            LabelInternetConnectionStatus.Text = "Online";
        }
        else
        {
            ImageSignalStatus.Image = TerminalApe.UI.Properties.Resources.signal_empty;
            LabelInternetConnectionStatus.Text = "Offline";
        }
    }
    public async void ExchangeLatencyStatus(string exchange)
    {
        long latency = await getLatency.ExchangeAPI(exchange);
        if (latency < 501)
        {
            Label ExchangeResponseStatus = (Label)allControls.GetControlByName($"Label{exchange}ExchangeStatus");
            ExchangeResponseStatus.ForeColor = Color.Green;
            ExchangeResponseStatus.Text = "Online";
        }
        else
        {
            Label ExchangeResponseStatus = (Label)allControls.GetControlByName($"Label{exchange}ExchangeStatus");
            ExchangeResponseStatus.ForeColor = Color.Red;
            ExchangeResponseStatus.Text = "Offline";
        }
    }
    public void ExchangePairsAndSymbols(string exchange, MarketCache oldExchangeData, MarketCache newExchangeData, MarketCache newPairsCache)
    {
        Label ExchangeResponseStatus = (Label)allControls.GetControlByName($"Label{exchange}ExchangeStatus");
        Label ExchangeTradingPairs = (Label)allControls.GetControlByName($"{exchange}NewCacheTradingPairs");
        Label ExchangeHaltedPairs = (Label)allControls.GetControlByName($"{exchange}NewCacheHaltedPairs");
        Label ExchangeAllPairs = (Label)allControls.GetControlByName($"{exchange}NewCacheAllPairs");
        Label ExchangeTradingBases = (Label)allControls.GetControlByName($"{exchange}NewCacheTradingBases");
        Label ExchangeTradingQuotes = (Label)allControls.GetControlByName($"{exchange}NewCacheTradingQuotes");
        Label ExchangeHaltedBases = (Label)allControls.GetControlByName($"{exchange}NewCacheHaltedBases");
        Label ExchangeHaltedQuotes = (Label)allControls.GetControlByName($"{exchange}NewCacheHaltedQuotes");
        
        Label CacheTradingPairs = (Label)allControls.GetControlByName($"{exchange}OldCacheTradingPairs");
        Label CacheHaltedPairs = (Label)allControls.GetControlByName($"{exchange}OldCacheHaltedPairs");
        Label CacheAllPairs = (Label)allControls.GetControlByName($"{exchange}OldCacheAllPairs");
        Label CacheTradingBases = (Label)allControls.GetControlByName($"{exchange}OldCacheTradingBases");
        Label CacheTradingQuotes = (Label)allControls.GetControlByName($"{exchange}OldCacheTradingQuotes");
        Label CacheHaltedBases = (Label)allControls.GetControlByName($"{exchange}OldCacheHaltedBases");
        Label CacheHaltedQuotes = (Label)allControls.GetControlByName($"{exchange}OldCacheHaltedQuotes");

        Label CacheNewPairs = (Label)allControls.GetControlByName($"{exchange}NewPairs");

        ExchangeResponseStatus.ForeColor = Color.Green;
        ExchangeResponseStatus.Text = "Online";
        
        ExchangeTradingPairs.Text = newExchangeData.TradingPairs.Keys.Count.ToString();
        ExchangeHaltedPairs.Text = newExchangeData.HaltedPairs.Keys.Count.ToString();
        ExchangeAllPairs.Text = newExchangeData.AllPairs.Count.ToString();

        ExchangeTradingBases.Text = newExchangeData.TradingBases.Keys.Count.ToString();
        ExchangeTradingQuotes.Text = newExchangeData.TradingQuotes.Keys.Count.ToString();
        ExchangeHaltedBases.Text = newExchangeData.HaltedBases.Keys.Count.ToString();
        ExchangeHaltedQuotes.Text = newExchangeData.HaltedQuotes.Keys.Count.ToString();

        CacheTradingPairs.Text = oldExchangeData.TradingPairs.Keys.Count.ToString();
        CacheHaltedPairs.Text = oldExchangeData.HaltedPairs.Keys.Count.ToString();
        CacheAllPairs.Text = oldExchangeData.AllPairs.Count.ToString();

        CacheTradingBases.Text = oldExchangeData.TradingBases.Keys.Count.ToString();
        CacheTradingQuotes.Text = oldExchangeData.TradingQuotes.Keys.Count.ToString();
        CacheHaltedBases.Text = oldExchangeData.HaltedBases.Keys.Count.ToString();
        CacheHaltedQuotes.Text = oldExchangeData.HaltedQuotes.Keys.Count.ToString();        

        CacheNewPairs.Text = newPairsCache.AllPairs.Count.ToString();
    }
    public void CreateOrderUI(MarketCache newMarketCache)
    {
        HashSet<string> baseCurrencies = new HashSet<string>();
        DropdownBase.Enabled = false;
        foreach (string baseCurrency in DropdownBase.Items)
        {
            baseCurrencies.Add(baseCurrency);
        }

        DropdownBase.Items.Clear();

        foreach (string baseCurrency in newMarketCache.TradingBases.Keys)
        {
            baseCurrencies.Add(baseCurrency);
        }
        foreach (string baseCurrency in newMarketCache.HaltedBases.Keys)
        {
            baseCurrencies.Add(baseCurrency);
        }

        DropdownBase.Items.AddRange(baseCurrencies.OrderBy(pair => pair).ToArray());
        DropdownBase.SelectedIndex = 0;
        DropdownBase.Enabled = true;

        HashSet<string> quoteCurrencies = new HashSet<string>();
        DropdownQuote.Enabled = false;

        foreach (string quoteCurrency in DropdownQuote.Items)
        {
            quoteCurrencies.Add(quoteCurrency);
        }

        DropdownQuote.Items.Clear();

        foreach (string quoteCurrency in newMarketCache.TradingQuotes.Keys)
        {
            quoteCurrencies.Add(quoteCurrency);
        }
        foreach (string quoteCurrency in newMarketCache.HaltedQuotes.Keys)
        {
            quoteCurrencies.Add(quoteCurrency);
        }

        DropdownQuote.Items.AddRange(quoteCurrencies.OrderBy(pair => pair).ToArray());
        DropdownQuote.SelectedIndex = 0;
        DropdownQuote.Enabled = true;

    }
}
