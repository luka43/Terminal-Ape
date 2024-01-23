using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;

using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DegenApe
{
    public partial class MainAppForm : Form
    {
        public LoadingForm loadingForm;
        public MainAppForm(LoadingForm loadingForm)
        {
            InitializeComponent();
            this.loadingForm = loadingForm;
            InitializeApp();
        }

        #region Define - Classes:
        public class ClassAppSettings
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
            public string ExchangeFile {  get; set; }
        }
        public class ClassExchangeSettings
        {
            public string APIKey { get; set; }
            public string SECRETKey { get; set; }
            public int TIMEOUT { get; set; }

            public string APIUrlBase { get; set; }
            public string APIUrlPing { get; set; }
            public string APIUrlPairs { get; set; }
            public string APIUrlOrder { get; set; }
        }
        public class Symbol
        {
            public List<string> Quotes { get; set; }
            public List<string> Bases { get; set; }
        }
        public class Pair
        {
            public string Base { get; set; }
            public string Quote { get; set; }
            public string Status { get; set; }
            public long Timestamp { get; set; }
        }
        public class MarketData
        {
            public Dictionary<string, Pair> TradingPairs { get; set; } = new Dictionary<string, Pair>();
            public Dictionary<string, Pair> HaltedPairs { get; set; } = new Dictionary<string, Pair>();
            public List<string> AllPairs { get; set; } = new List<string>();

            public Dictionary<string, Symbol> TradingBases { get; set; } = new Dictionary<string, Symbol>();
            public Dictionary<string, Symbol> HaltedBases { get; set; } = new Dictionary<string, Symbol>();

            public Dictionary<string, Symbol> TradingQuotes { get; set; } = new Dictionary<string, Symbol>();
            public Dictionary<string, Symbol> HaltedQuotes { get; set; } = new Dictionary<string, Symbol>();

            public long Timestamp { get; set; }
        }
        public class Order
        {
            public string Base { set; get; }
            public string Quote { set; get; }
            public string Side { set; get; }
            public string Type { set; get; }
            public string Size { set; get; }
            public string Stoploss { set; get; }
            public string Takeprofit { set; get; }
            public string Reenter { set; get; }
            public string NumberOfExec { set; get; }
            public List<string> Exchange { set; get; }
        }
        #endregion

        #region Define - AppSettings:

        private System.Drawing.Icon logoenabled = new System.Drawing.Icon("logoenabled.ico");
        private System.Drawing.Icon logodisabled = new System.Drawing.Icon("logodisabled.ico");

        public NotifyIcon AppTrayIcon = new NotifyIcon();

        public ClassAppSettings AppSettings = new ClassAppSettings()
        {
            Exchanges = { "Binance", "Bybit", "Coinbase" },

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
        public Dictionary<string, ClassExchangeSettings> ExchangeSettings = new Dictionary<string, ClassExchangeSettings>()
        {
            { "Binance", new ClassExchangeSettings()
                {
                    APIKey = "ENTER APY KEY HERE",
                    SECRETKey = "ENTER SECRET KEY HERE",
                    TIMEOUT = 6000,

                    APIUrlBase = "https://api.binance.com",
                    APIUrlPing = "/api/v3/time",
                    APIUrlPairs = "/api/v3/exchangeInfo?permissions=SPOT",
                    APIUrlOrder = "/api/v3/order",
                }
             },
            { "Bybit", new ClassExchangeSettings()
                {
                    APIKey = "ENTER APY KEY HERE",
                    SECRETKey = "ENTER SECRET KEY HERE",
                    TIMEOUT = 6000,

                    APIUrlBase = "https://api.bybit.com",
                    APIUrlPing = "/v5/market/time",
                    APIUrlPairs = "/v2/public/symbols?limit=2500",
                    APIUrlOrder = "/api/v3/order",
                }
            },
            { "Coinbase", new ClassExchangeSettings()
                {
                    APIKey = "ENTER APY KEY HERE",
                    SECRETKey = "ENTER SECRET KEY HERE",
                    TIMEOUT = 6000,

                    APIUrlBase = "https://api.exchange.coinbase.com",
                    APIUrlPing = "/time",
                    APIUrlPairs = "/products",
                    APIUrlOrder = "/api/v3/order",

                    // https://api.exchange.coinbase.com/v2/public/symbols
                }
            }
        };
        #endregion
        
        #region Define - Databases:
        public Dictionary<string, dynamic> oldMarketDataRaw = new Dictionary<string, dynamic>();
        public Dictionary<string, MarketData> oldMarketData = new Dictionary<string, MarketData>();

        public Dictionary<string, dynamic> newMarketDataRaw = new Dictionary<string, dynamic>();
        public Dictionary<string, MarketData> newMarketData = new Dictionary<string, MarketData>();

        public Dictionary<string, MarketData> newPairs = new Dictionary<string, MarketData>();

        public List<Order> orderList = new List<Order>();
        #endregion

        #region Define - Misc. Memory:
       
        public int logCounter = 0;
        public List<string> selectedExchanges = new List<string>();
        // TIMERS
        public Timer Ape = new Timer();
        #endregion

        #region LOG:

        public void Log(dynamic data)
        {
            string stringToLog = data.ToString();
            // If it's first log line,
            // Initialize log filename for autosave.
            // It will reset everytime logbox is reset
            if (logCounter == 0)
            {
                logCounter++;
                Directory.CreateDirectory(AppSettings.LogDirectory);
                AppSettings.LogFilePath = Path.Combine(AppSettings.LogDirectory, $"{DateTime.Now.ToString("[dd-MM-yy]-[hh-mm-sstt]")}.log");

                logBox.AppendText($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}\r\n");
            }
            // If it's not first line but also not 10.000 line
            // Print to logBox and auto save every X lines
            else if (logCounter > 0 && logCounter <= AppSettings.LogMaxEntriesBeforeReset)
            {
                // Print to logbox and scroll down
                logCounter++;
                logBox.AppendText($"{DateTime.Now.ToString("[dd.MM.yy]-[hh:mm:sstt]")} - {stringToLog}\r\n");

                // Autosave every 10 lines
                if (logCounter % AppSettings.LogAutosaveInterval == 0)
                {
                    System.IO.File.WriteAllText(AppSettings.LogFilePath, logBox.Text.ToString());
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

        #endregion

        #region Check Connection:
        public async Task<bool> CheckInternetConnection()
        {
            try
            {
                // Ping a well-known and stable server, such as Google's public DNS server (8.8.8.8)
                Ping ping = new Ping();
                PingReply reply = ping.Send("8.8.8.8", 1000); // 1000 milliseconds timeout

                return reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                return false;
            }
        }
        #endregion

        #region File Handling:

        public async Task<Dictionary<string, MarketData>> LoadDatabaseFrom(string filepath = null)
        {
            Dictionary<string, MarketData> result = null;            

            // Create default filepath
            if (filepath == null)
            {
                filepath = Path.Combine(AppSettings.DatabaseDirectory, AppSettings.DatabaseFile);
            }

            try
            {
                Log($"Loading database from: \"{filepath}\"");
                string jsonString = System.IO.File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<Dictionary<string, MarketData>>(jsonString);

                Log($"Database found.");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
            return result;
        }
        public async Task OpenDatabase()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Open Database",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + AppSettings.DatabaseDirectory, // Set initial directory to the app folder
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                oldMarketData.Clear();
                newPairs.Clear();

                oldMarketData = await LoadDatabaseFrom(openFileDialog.FileName);
                Log($"Database loaded from: \"{openFileDialog.FileName}\"");
                foreach(string exchange in AppSettings.Exchanges) 
                {
                    UpdateDatabaseDataUI(exchange);

                    newPairs.Add(exchange, await Compare(exchange));
                    UpdateNewPairsUI(exchange);
                }
            }
        }
        public void SaveExchangeDatabase(string exchange)
        {
            DialogResult answer = MessageBox.Show($"Are you sure you want to overwrite {exchange} data on this system with currently collected data?", $"Save {exchange} data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer == DialogResult.Yes)
            {
                oldMarketData.Remove(exchange);
                oldMarketData.Add(exchange, newMarketData[exchange]);

                string jsonString = JsonConvert.SerializeObject(oldMarketData, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppSettings.DatabaseDirectory, AppSettings.DatabaseFile), jsonString);

                UpdateDatabaseDataUI(exchange);

                newPairs.Remove(exchange);
                newPairs.Add(exchange, new MarketData());

                UpdateNewPairsUI(exchange);
            }
        }
        public void SaveDatabaseAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save As",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppSettings.DatabaseDirectory),
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string jsonString = JsonConvert.SerializeObject(newMarketData, Formatting.Indented);
                File.WriteAllText(saveFileDialog.FileName, jsonString);
                Log($"New market data saved at: \"{saveFileDialog.FileName}\"");
            }
        }


        #endregion

        #region INITIALIZE:
        public void InitializeSettings()
        {
            // LOAD APPLICATION SETTINGS FROM FILE
            if (File.Exists(Path.Combine(AppSettings.Directory, AppSettings.SettingsFile)))
            {
                Log($"Loading app settings from: \"{Path.Combine(AppSettings.Directory, AppSettings.SettingsFile)}\" ...");

                // Load settings to AppSettings
                try
                {
                    string jsonString = System.IO.File.ReadAllText(Path.Combine(AppSettings.Directory, AppSettings.SettingsFile));
                    AppSettings = JsonConvert.DeserializeObject<ClassAppSettings>(jsonString);
                    AppSettings.LogFilePath = Path.Combine(AppSettings.LogDirectory, $"{DateTime.Now.ToString("[dd-MM-yy]-[hh-mm-sstt]")}.log");
                    Log("App settings loaded.");
                }
                catch (Exception ex)
                {
                    Log($"{ ex.Message }");
                }
            }
            // LOAD DEFAULT APPLICATION SETTINGS AND CREATE FILE
            else
            {
                Log("Loading default app settings...");

                // Create default Application settings file to Default Filepath
                try
                {
                    Log($"Creating settings folder at: \"{Path.Combine(AppSettings.Directory, AppSettings.SettingsFile)}\" ...");
                    Directory.CreateDirectory(AppSettings.Directory);
                    string jsonString = JsonConvert.SerializeObject(AppSettings, Formatting.Indented);
                    File.WriteAllText(Path.Combine(AppSettings.Directory, AppSettings.SettingsFile), jsonString);
                    Log("Folder created.");
                }
                catch (Exception ex)
                {
                    Log($"{ex.Message}");
                }
            }            

            // LOAD EXCHANGE SETTINGS FROM FILE AND CREATE FILE
            if (File.Exists(Path.Combine(AppSettings.Directory, AppSettings.ExchangeFile)))
            {
                Log($"Loading exchange settings from: \"{Path.Combine(AppSettings.Directory, AppSettings.ExchangeFile)}\" ...");

                try
                {
                    // Load settings to ExchangeSettings
                    string jsonString = System.IO.File.ReadAllText(Path.Combine(AppSettings.Directory, AppSettings.ExchangeFile));
                    ExchangeSettings.Clear();
                    ExchangeSettings = JsonConvert.DeserializeObject<Dictionary<string, ClassExchangeSettings>>(jsonString);

                    Log("Exchange settings loaded.");
                }
                catch (Exception ex)
                {
                    Log($"{ex.Message}");
                }
            }
            // LOAD DEFAULT EXCHANGE SETTINGS FROM FILE AND CREATE FILE
            else
            {
                try
                {
                    Log($"Creating settings folder at: \"{Path.Combine(AppSettings.Directory, AppSettings.SettingsFile)}\" ...");

                    // Create default settings file to Default Filepath
                    Directory.CreateDirectory(AppSettings.Directory);
                    string jsonString = JsonConvert.SerializeObject(ExchangeSettings, Formatting.Indented);
                    File.WriteAllText(Path.Combine(AppSettings.Directory, AppSettings.ExchangeFile), jsonString);

                    Log("Loading default exchange settings");
                }
                catch (Exception ex)
                {
                    Log($"{ex.Message}");
                }
            }
        }
        public void InitializeTrayIcon()
        {
            // Initialize the NotifyIcon            
            AppTrayIcon.Icon = logodisabled;
            AppTrayIcon.Text = "TerminalApe v1.0";
            AppTrayIcon.Visible = false;

            // Attach the context menu for the NotifyIcon (optional)
            AppTrayIcon.ContextMenuStrip = new ContextMenuStrip();

            AppTrayIcon.ContextMenuStrip.Items.Add("Open", null, TrayIconOpenMenu_Click);

            AppTrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            AppTrayIcon.ContextMenuStrip.Items.Add("Start", null, TrayIconOpenMenu_Click);
            AppTrayIcon.ContextMenuStrip.Items.Add("Stop", null, TrayIconOpenMenu_Click);

            AppTrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            AppTrayIcon.ContextMenuStrip.Items.Add("Exit", null, TrayIconExitMenu_Click);

            // Attach the double-click event for the NotifyIcon
            AppTrayIcon.MouseDoubleClick += TrayIcon_DoubleClick;
        }
        public void InitializeCreateOrderUI()
        {
            UpdateOrderCreateUI();
            CheckBoxAnyBase.Checked = true;
            CheckBoxAnyBase.Enabled = false;

            DropdownBase.Enabled = true;
            DropdownQuote.Enabled = true;
            CheckBoxAnyBase.Enabled = true;
            DropdownSide.Enabled = true;
            DropdownType.Enabled = true;
            BoxSize.Enabled = true;
            DropdownSizeQuote.Enabled = true;
            BoxStoploss.Enabled = true;
            BoxTakeprofit.Enabled = true;
            ButtonAddOrder.Enabled = true;
            ButtonRemoveorder.Enabled = false;
        }
        public async Task InitializeApp()
        {
            InitializeTrayIcon();

            // Initialize LOG folder and filename
            Directory.CreateDirectory(AppSettings.LogDirectory);
            AppSettings.LogFilePath = Path.Combine(AppSettings.LogDirectory, $"{DateTime.Now.ToString("[dd-MM-yy]-[hh-mm-sstt]")}.log");

            // Initialize folders of Database, Logs and Settings
            Log($"Initializing Folders: \\{AppSettings.DatabaseDirectory}\\, \\{AppSettings.Directory}\\, \\{AppSettings.LogDirectory}\\ ...");
            loadingForm.LabelStatus.Text = "Initializing files and folders...";

            InitializeSettings();

            loadingForm.ProgressBar.Value += 5;

            bool isConnected;

            do
            {
                isConnected = await CheckInternetConnection();

                if (!await CheckInternetConnection())
                {
                    DialogResult result = MessageBox.Show("No internet Connection! Retry?", "ERROR", MessageBoxButtons.RetryCancel);

                    if (result != DialogResult.Retry)
                    {
                        // User chose to cancel or closed the dialog, gracefully exit the application
                        Application.Exit();
                        return; // Optional, depending on your application flow
                    }
                }
            } while (!isConnected);

            // Get new data for new database if given filepath doesn't exist
            if (!File.Exists(Path.Combine(AppSettings.DatabaseDirectory, AppSettings.DatabaseFile)))
            {
                oldMarketData.Clear();

                try { Directory.CreateDirectory(AppSettings.DatabaseDirectory); }
                catch (Exception ex) { Log($"{ex.Message}"); }

                // Get new market data
                foreach (string exchange in AppSettings.Exchanges)
                {
                    Log($"Getting {exchange} data and storing it in {Path.Combine(AppSettings.DatabaseDirectory, AppSettings.DatabaseFile)}...");
                    loadingForm.LabelStatus.Text = $"Getting {exchange} data and saving it as old...";

                    newMarketDataRaw[exchange] = await GetExchangeData(exchange);
                    oldMarketData[exchange] = await SortRawData(exchange, newMarketDataRaw);
                }

                newMarketData = oldMarketData;

            }
            else
            {
                Log($"Loading database from file...");
                loadingForm.LabelStatus.Text = "Loading database from file...";
                oldMarketData = await LoadDatabaseFrom();
            }

            loadingForm.ProgressBar.Value += 5;

            foreach (string exchange in AppSettings.Exchanges)
            {
                loadingForm.ProgressBar.Value += 5;           

                // Update UI with old database data
                Log($"Updating {exchange} database UI...");
                loadingForm.LabelStatus.Text = $"Updating {exchange} database UI...";
                UpdateDatabaseDataUI(exchange);

                loadingForm.ProgressBar.Value += 5;

                // Get new market data
                if (!newMarketData.ContainsKey(exchange))
                {
                    Log($"Getting new {exchange} data...");
                    loadingForm.LabelStatus.Text = $"Getting new {exchange} data...";

                    newMarketDataRaw.Add(exchange, await GetExchangeData(exchange));

                    Log($"Sorting {exchange} data...");
                    loadingForm.LabelStatus.Text = $"Getting {exchange} data...";
                    newMarketData.Add(exchange, await SortRawData(exchange, newMarketDataRaw[exchange]));

                    loadingForm.ProgressBar.Value += 5;
                }

                // Update UI with new market data
                Log($"Updating UI with new {exchange} data...");
                loadingForm.LabelStatus.Text = $"Updating UI with new {exchange} data...";
                UpdateExchangeDataUI(exchange);

                loadingForm.ProgressBar.Value += 5;


                // Get new pairs and update UI with data
                Log($"Comparing data from {exchange}...");
                loadingForm.LabelStatus.Text = $"Comparing data from {exchange}...";
                newPairs.Add(exchange, await Compare(exchange));
                loadingForm.ProgressBar.Value += 5;

                // Update UI with new found pairs
                Log($"Updating UI from {exchange}...");
                loadingForm.LabelStatus.Text = $"Updating UI from {exchange}...";
                UpdateNewPairsUI(exchange);

                loadingForm.ProgressBar.Value += 3;
            }

            Log($"Updating UI controls...");
            loadingForm.LabelStatus.Text = "Updating UI controls...";

            // Initialize Create Order UI:
            
            InitializeCreateOrderUI();
            loadingForm.ProgressBar.Value += 5;

            Log($"Done.");
            loadingForm.LabelStatus.Text = "Done";
            statusBox.Text = "Ready";

            loadingForm.ProgressBar.Value = loadingForm.ProgressBar.Maximum;

            //await Task.Delay(200);

            loadingForm.Hide();
            this.Show();
            ScrollToBottom(null, EventArgs.Empty);


            // Check internet connection



        }
        #endregion

        #region Market Data Handling:

        // Exchange data handling and sorting:
        public async Task<dynamic> GetExchangeData(string exchange)
        {
            dynamic responseData = null;
            try
            {                
                Log($"Getting response from: {exchange}");
                // SEND API REQUEST AND GET RESPONSE
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "DegenApe");
                    HttpResponseMessage response = await client.GetAsync(ExchangeSettings[exchange].APIUrlBase + ExchangeSettings[exchange].APIUrlPairs);

                    // RESPONSE SUCCESS
                    if (response.IsSuccessStatusCode)
                    {
                        Log($"Success! Sorting {exchange} data..");

                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (exchange == "Binance" || exchange == "Bybit") { responseData = JObject.Parse(responseContent); }
                        else if (exchange == "Coinbase") { responseData = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent); }                        
                    }
                    // PRINT ERROR GETTING RESPONSE
                    else
                    {
                        Log($"Error getting response from the {exchange} server!");
                        Log($"Response status code: {response.IsSuccessStatusCode}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"{ex.Message}");
            }

            return responseData;
        }
        public async Task<MarketData> SortRawData(string exchange, dynamic data)
        {

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            MarketData result = new MarketData();
            await Task.Run(() =>
            {
                if (exchange == "Binance")
                {
                    foreach (var item in data.symbols)
                    {
                        // Add pair to ALLPAIRS
                        if (!result.AllPairs.Contains(item.symbol.ToString()))
                        {
                            result.AllPairs.Add(item.symbol.ToString());
                        }
                        // TRADING
                        if (item.status.ToString() == "TRADING")
                        {
                            // TRADING PAIRS DOES NOT COINTAIN GIVEN PAIR
                            if (!result.TradingPairs.ContainsKey(item.symbol.ToString()))
                            {
                                // ADD TO TRADING PAIRS
                                result.TradingPairs.Add(item.symbol.ToString(), new Pair());
                                result.TradingPairs[item.symbol.ToString()].Base = item.baseAsset.ToString();
                                result.TradingPairs[item.symbol.ToString()].Quote = item.quoteAsset.ToString();
                                result.TradingPairs[item.symbol.ToString()].Status = "TRADING";
                                result.TradingPairs[item.symbol.ToString()].Timestamp = timestamp;
                            }
                            // TRADINGBASES DOES NOT COINTAIN GIVEN SYMBOL
                            if (!result.TradingBases.ContainsKey(item.baseAsset.ToString()))
                            {
                                // CREATE NEW SYMBOL IN TRADINGBASE
                                result.TradingBases.Add(item.baseAsset.ToString(), new Symbol()
                                {
                                    Quotes = new List<string>(),
                                    Bases = new List<string>()
                                });
                                result.TradingBases[item.baseAsset.ToString()].Bases.Add(item.baseAsset.ToString());
                                result.TradingBases[item.baseAsset.ToString()].Quotes.Add(item.quoteAsset.ToString());
                            }
                            // TRADINGBASES COINTAINS GIVEN SYMBOL
                            else
                            {
                                // TRADINGBASES DOES NOT COINTAIN GIVEN QUOTE
                                if (!result.TradingBases[item.baseAsset.ToString()].Quotes.Contains(item.quoteAsset.ToString()))
                                {
                                    // ADD NEW QUOTE TO QUOTES
                                    result.TradingBases[item.baseAsset.ToString()].Quotes.Add(item.quoteAsset.ToString());
                                }
                            }
                            // TRADINGQUOTES DOES NOT COINTAIN GIVEN SYMBOL
                            if (!result.TradingQuotes.ContainsKey(item.quoteAsset.ToString()))
                            {
                                // CREATE NEW SYMBOL IN TRADINGQUOTES
                                result.TradingQuotes.Add(item.quoteAsset.ToString(), new Symbol()
                                {
                                    Quotes = new List<string>(),
                                    Bases = new List<string>()
                                });
                                result.TradingQuotes[item.quoteAsset.ToString()].Bases.Add(item.baseAsset.ToString());
                                result.TradingQuotes[item.quoteAsset.ToString()].Quotes.Add(item.quoteAsset.ToString());
                            }
                            // TRADINGQUOTES COINTAINS GIVEN SYMBOL
                            else
                            {
                                // TRADINGQUOTES DOES NOT COINTAIN GIVEN QUOTE
                                if (!result.TradingQuotes[item.quoteAsset.ToString()].Bases.Contains(item.baseAsset.ToString()))
                                {
                                    // ADD NEW BASE TO BASES
                                    result.TradingQuotes[item.quoteAsset.ToString()].Bases.Add(item.baseAsset.ToString());
                                }
                            }
                        }
                        // HALTED PAIR
                        else
                        {
                            // HALTED PAIRS DOES NOT COINTAIN GIVEN PAIR
                            if (!result.HaltedPairs.ContainsKey(item.symbol.ToString()))
                            {
                                // ADD TO TRADING PAIRS
                                result.HaltedPairs.Add(item.symbol.ToString(), new Pair());
                                result.HaltedPairs[item.symbol.ToString()].Base = item.baseAsset.ToString();
                                result.HaltedPairs[item.symbol.ToString()].Quote = item.quoteAsset.ToString();
                                result.HaltedPairs[item.symbol.ToString()].Status = "HALTED";
                                result.HaltedPairs[item.symbol.ToString()].Timestamp = timestamp;
                            }
                            // HALTEDBASES DOES NOT COINTAIN GIVEN SYMBOL
                            if (!result.HaltedBases.ContainsKey(item.baseAsset.ToString()))
                            {
                                // CREATE NEW SYMBOL IN HALTEDBASES
                                result.HaltedBases.Add(item.baseAsset.ToString(), new Symbol()
                                {
                                    Quotes = new List<string>(),
                                    Bases = new List<string>()
                                });
                                result.HaltedBases[item.baseAsset.ToString()].Bases.Add(item.baseAsset.ToString());
                                result.HaltedBases[item.baseAsset.ToString()].Quotes.Add(item.quoteAsset.ToString());
                            }
                            // TRADINGBASES COINTAINS GIVEN SYMBOL
                            else
                            {
                                // TRADINGBASES DOES NOT COINTAIN GIVEN QUOTE
                                if (!result.HaltedBases[item.baseAsset.ToString()].Quotes.Contains(item.quoteAsset.ToString()))
                                {
                                    // ADD NEW QUOTE TO QUOTES
                                    result.HaltedBases[item.baseAsset.ToString()].Quotes.Add(item.quoteAsset.ToString());
                                }
                            }
                            // TRADINGQUOTES DOES NOT COINTAIN GIVEN SYMBOL
                            if (!result.HaltedQuotes.ContainsKey(item.quoteAsset.ToString()))
                            {
                                // CREATE NEW SYMBOL IN TRADINGQUOTES
                                result.HaltedQuotes.Add(item.quoteAsset.ToString(), new Symbol()
                                {
                                    Quotes = new List<string>(),
                                    Bases = new List<string>()
                                });
                                result.HaltedQuotes[item.quoteAsset.ToString()].Bases.Add(item.baseAsset.ToString());
                                result.HaltedQuotes[item.quoteAsset.ToString()].Quotes.Add(item.quoteAsset.ToString());
                            }
                            // TRADINGQUOTES COINTAINS GIVEN SYMBOL
                            else
                            {
                                // TRADINGQUOTES DOES NOT COINTAIN GIVEN QUOTE
                                if (!result.HaltedQuotes[item.quoteAsset.ToString()].Bases.Contains(item.baseAsset.ToString()))
                                {
                                    // ADD NEW BASE TO BASES
                                    result.HaltedQuotes[item.quoteAsset.ToString()].Bases.Add(item.baseAsset.ToString());
                                }
                            }
                        }
                    }
                }
                else if (exchange == "Bybit")
                {
                    foreach (var item in data.result)
                    {
                        // Add pair to ALLPAIRS
                        if (!result.AllPairs.Contains(item.name.ToString()))
                        {
                            result.AllPairs.Add(item.name.ToString());
                        }
                        // TRADING
                        if (item.status.ToString().ToUpper() == "TRADING")
                        {
                            // TRADING PAIRS DOES NOT COINTAIN GIVEN PAIR
                            if (!result.TradingPairs.ContainsKey(item.base_currency.ToString()))
                            {
                                // ADD TO TRADING PAIRS
                                result.TradingPairs.Add(item.name.ToString(), new Pair());
                                result.TradingPairs[item.name.ToString()].Base = item.base_currency.ToString();
                                result.TradingPairs[item.name.ToString()].Quote = item.quote_currency.ToString();
                                result.TradingPairs[item.name.ToString()].Status = "TRADING";
                                result.TradingPairs[item.name.ToString()].Timestamp = timestamp;
                            }
                            // TRADINGBASES DOES NOT COINTAIN GIVEN SYMBOL
                            if (!result.TradingBases.ContainsKey(item.base_currency.ToString()))
                            {
                                // CREATE NEW SYMBOL IN TRADINGBASE
                                result.TradingBases.Add(item.base_currency.ToString(), new Symbol()
                                {
                                    Quotes = new List<string>(),
                                    Bases = new List<string>()
                                });
                                result.TradingBases[item.base_currency.ToString()].Quotes.Add(item.quote_currency.ToString());
                                result.TradingBases[item.base_currency.ToString()].Bases.Add(item.base_currency.ToString());
                            }
                            // TRADINGBASES COINTAINS GIVEN SYMBOL
                            else
                            {
                                // TRADINGBASES DOES NOT COINTAIN GIVEN QUOTE
                                if (!result.TradingBases[item.base_currency.ToString()].Quotes.Contains(item.quote_currency.ToString()))
                                {
                                    // ADD NEW QUOTE TO QUOTES
                                    result.TradingBases[item.base_currency.ToString()].Quotes.Add(item.quote_currency.ToString());
                                }
                            }

                            if (!result.TradingQuotes.ContainsKey(item.quote_currency.ToString()))
                            {
                                // CREATE NEW SYMBOL IN TRADINGQUOTES
                                result.TradingQuotes.Add(item.quote_currency.ToString(), new Symbol()
                                {
                                    Quotes = new List<string>(),
                                    Bases = new List<string>()
                                });
                                result.TradingQuotes[item.quote_currency.ToString()].Bases.Add(item.base_currency.ToString());
                                result.TradingQuotes[item.quote_currency.ToString()].Quotes.Add(item.quote_currency.ToString());
                            }
                            // TRADINGQUOTES COINTAINS GIVEN SYMBOL
                            else
                            {
                                // TRADINGQUOTES DOES NOT COINTAIN GIVEN QUOTE
                                if (!result.TradingQuotes[item.quote_currency.ToString()].Bases.Contains(item.base_currency.ToString()))
                                {
                                    // ADD NEW BASE TO BASES
                                    result.TradingQuotes[item.quote_currency.ToString()].Bases.Add(item.base_currency.ToString());
                                }
                            }
                        }
                        // HALTED
                        else
                        {
                            if (!result.HaltedPairs.ContainsKey(item.base_currency.ToString()))
                            {
                                result.HaltedPairs.Add(item.base_currency.ToString(), new Pair());
                                result.HaltedPairs[item.name.ToString()].BaseAsset = item.base_currency.ToString();
                                result.HaltedPairs[item.name.ToString()].QuoteAssets.Add(item.quote_currency.ToString());
                                result.HaltedPairs[item.name.ToString()].Status = "HALTED";
                                result.HaltedPairs[item.name.ToString()].Timestamp = timestamp;

                            }
                            else
                            {
                                if (!result.HaltedPairs[item.base_currency.ToString()].QuoteAssets.Contains(item.quote_currency.ToString()))
                                {
                                    result.HaltedPairs[item.base_currency.ToString()].QuoteAssets.Add(item.quote_currency.ToString());
                                }
                            }
                        }
                    }
                }
                else if (exchange == "Coinbase")
                {
                    foreach (var item in data)
                    {
                        if (!result.AllPairs.Contains(item.id.ToString().Replace("-", "")))
                        {
                            result.AllPairs.Add(item.id.ToString().Replace("-", ""));
                        }

                        if (item.trading_disabled == false)
                        {
                            if (!result.TradingPairs.ContainsKey(item.id.ToString().Replace("-", "")))
                            {
                                result.TradingPairs.Add(item.id.ToString().Replace("-", ""), new Pair());
                                result.TradingPairs[item.id.ToString().Replace("-", "")].Base = item.base_currency.ToString();
                                result.TradingPairs[item.id.ToString().Replace("-", "")].Quote = item.quote_currency.ToString();
                                result.TradingPairs[item.id.ToString().Replace("-", "")].Status = "TRADING";
                                result.TradingPairs[item.id.ToString().Replace("-", "")].Timestamp = timestamp;
                            }
                            // TRADING BASE SYMBOL DOES NOT COINTAIN GIVEN ITEM
                            // NEW BASE SYMBOL DETECTED
                            if (!result.TradingBases.ContainsKey(item.base_currency.ToString()))
                            {
                                // ADD TO TRADINGBASE
                                // ADD QUOTE ASSET
                                result.TradingBases.Add(item.base_currency.ToString(), new Symbol()
                                {
                                    Quotes = new List<string>()
                                });
                                result.TradingBases[item.base_currency.ToString()].Quotes.Add(item.quote_currency.ToString());
                            }
                            // TRADING QUOTE SYMBOL DOES NOT COINTAIN GIVEN ITEM
                            // NEW QUOTE SYMBOL DETECTED
                            if (!result.TradingQuotes.ContainsKey(item.quote_currency.ToString()))
                            {
                                // ADD TO TRADINGQUOTES
                                // ADD BASE ASSET
                                result.TradingQuotes.Add(item.quote_currency.ToString(), new Symbol()
                                {
                                    Bases = new List<string>()
                                });
                                result.TradingQuotes[item.quote_currency.ToString()].Bases.Add(item.base_currency.ToString());
                            }
                        }

                        else
                        {
                            if (!result.HaltedPairs.ContainsKey(item.base_currency.ToString()))
                            {
                                result.HaltedPairs.Add(item.id.ToString().Replace("-", ""), new Pair());
                                result.HaltedPairs[item.id.ToString().Replace("-", "")].Base = item.base_currency.ToString();
                                result.HaltedPairs[item.id.ToString().Replace("-", "")].Quote = item.quote_currency.ToString();
                                result.HaltedPairs[item.id.ToString().Replace("-", "")].Status = "HALTED";
                                result.HaltedPairs[item.id.ToString().Replace("-", "")].Timestamp = timestamp;
                            }
                            else
                            {
                                if (!result.HaltedPairs[item.base_currency.ToString()].Quote.Contains(item.quote_currency.ToString()))
                                {
                                    result.HaltedPairs[item.base_currency.ToString()].Quote = item.quote_currency.ToString();
                                }
                            }
                        }
                    }
                }

                result.AllPairs.Sort();
                result.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            });
            return result;

        }
        public async Task<MarketData> Compare(string exchange)
        {
            MarketData data = newMarketData[exchange];
            MarketData result = new MarketData();

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // GET ALL INTRUDERS AND ADD THEM TO ALLPAIRS
            // ADD ALL INTRUDERS TO ALLPAIRS
            await Task.Run(() =>
            {
                result.AllPairs = data.AllPairs.Except(oldMarketData[exchange].AllPairs).ToList();
                result.AllPairs.Sort();

                foreach (string item in result.AllPairs)
                {
                    // TRADING
                    if (data.TradingPairs[item].Status.ToUpper() == "TRADING")
                    {
                        // ADD INTRUDERS TO TRADINGPAIRS
                        result.TradingPairs.Add(item, new Pair()
                        {
                            Base = data.TradingPairs[item].Base.ToString(),
                            Quote = data.TradingPairs[item].Quote.ToString(),
                            Status = "TRADING",
                            Timestamp = timestamp
                        });
                        // TRADINGBASES DOES NOT COINTAIN GIVEN SYMBOL
                        if (!result.TradingBases.ContainsKey(data.TradingPairs[item].Base))
                        {
                            // ADD TRADINGBASES
                            result.TradingBases.Add(data.TradingPairs[item].Base, new Symbol()
                            {
                                Quotes = new List<string>(),
                                Bases = new List<string>(),
                            });
                            result.TradingBases[data.TradingPairs[item].Base].Bases.Add(data.TradingPairs[item].Base);
                            result.TradingBases[data.TradingPairs[item].Base].Quotes.Add(data.TradingPairs[item].Quote);
                        }
                        else
                        {
                            if (!result.TradingBases[result.TradingPairs[item].Base].Quotes.Contains(result.TradingPairs[item].Quote))
                            {
                                result.TradingBases[data.TradingPairs[item].Base].Quotes.Add(data.TradingPairs[item].Quote);
                            }
                        }
                    }
                    // ADD INTRUDERS TO HALTEDPAIRS
                    else
                    {
                        result.HaltedPairs.Add(item, new Pair()
                        {
                            Base = data.HaltedPairs[item].Base.ToString(),
                            Quote = data.HaltedPairs[item].Quote.ToString(),
                            Status = "HALTED",
                            Timestamp = timestamp
                        });
                    }
                }
            });

            return result;
        }

        #endregion

        #region UI Updating:

        // UI data updating
        public void UpdateExchangeDataUI(string exchange)
        {
            Log($"{exchange} is ONLINE. Timestamp: {newMarketData[exchange].Timestamp}");
            if (exchange == "Binance")
            {

                LabelBinanceExchangeStatus.ForeColor = Color.Green;
                LabelBinanceExchangeStatus.Text = "Online";

                LabelBinanceExchangeTradingPairs.Text = newMarketData[exchange].TradingPairs.Keys.Count.ToString();
                LabelBinanceExchangeHaltedPairs.Text = newMarketData[exchange].HaltedPairs.Keys.Count.ToString();
                LabelBinanceExchangeAllPairs.Text = newMarketData[exchange].AllPairs.Count.ToString();

                LabelBinanceExchangeTradingBases.Text = newMarketData[exchange].TradingBases.Keys.Count.ToString();
                LabelBinanceExchangeHaltedBases.Text = newMarketData[exchange].HaltedBases.Keys.Count.ToString();
                LabelBinanceExchangeTradingQuotes.Text = newMarketData[exchange].TradingQuotes.Keys.Count.ToString();
                LabelBinanceExchangeHaltedQuotes.Text = newMarketData[exchange].HaltedQuotes.Keys.Count.ToString();

            }
            else if (exchange == "Bybit")
            {

                LabelBybitExchangeStatus.ForeColor = Color.Green;
                LabelBybitExchangeStatus.Text = "Online";

                LabelBybitExchangeTradingPairs.Text = newMarketData[exchange].TradingPairs.Keys.Count.ToString();
                LabelBybitExchangeHaltedPairs.Text = newMarketData[exchange].HaltedPairs.Keys.Count.ToString();
                LabelBybitExchangeAllPairs.Text = newMarketData[exchange].AllPairs.Count.ToString();

                LabelBybitExchangeTradingBases.Text = newMarketData[exchange].TradingBases.Keys.Count.ToString();
                LabeBybitExchangeHaltedBases.Text = newMarketData[exchange].HaltedBases.Keys.Count.ToString();
                LabelBybitExchangeTradingQuotes.Text = newMarketData[exchange].TradingQuotes.Keys.Count.ToString();
                LabelBybitExchangeHaltedQuotes.Text = newMarketData[exchange].HaltedQuotes.Keys.Count.ToString();
            }
            else if (exchange == "Coinbase")
            {

                LabelCoinbaseStatus.ForeColor = Color.Green;
                LabelCoinbaseStatus.Text = "Online";

                LabelCoinbaseExchangeTradingPairs.Text = newMarketData[exchange].TradingPairs.Keys.Count.ToString();
                LabelCoinbaseExchangeHaltedPairs.Text = newMarketData[exchange].HaltedPairs.Keys.Count.ToString();
                LabelCoinbaseExchangeAllPairs.Text = newMarketData[exchange].AllPairs.Count.ToString();

                LabelCoinbaseExchangeTradingBases.Text = newMarketData[exchange].TradingBases.Keys.Count.ToString();
                LabeCoinbaseExchangeHaltedBases.Text = newMarketData[exchange].HaltedBases.Keys.Count.ToString();
                LabelCoinbaseExchangeTradingQuotes.Text = newMarketData[exchange].TradingQuotes.Keys.Count.ToString();
            }
        }
        public void UpdateDatabaseDataUI(string exchange)
        {
            if (exchange == "Binance")
            {
                LabelBinanceDatabaseTradingPairs.Text = oldMarketData[exchange].TradingPairs.Keys.Count.ToString();
                LabelBinanceDatabaseHaltedPairs.Text = oldMarketData[exchange].HaltedPairs.Keys.Count.ToString();
                LabelBinanceDatabaseAllPairs.Text = oldMarketData[exchange].AllPairs.Count.ToString();

                LabelBinanceDatabaseTradingBases.Text = oldMarketData[exchange].TradingBases.Count.ToString();
                LabelBinanceDatabaseHaltedBases.Text = oldMarketData[exchange].HaltedBases.Count.ToString();
                LabelBinanceDatabaseTradingQuotes.Text = oldMarketData[exchange].TradingQuotes.Count.ToString();
                LabelBinanceDatabaseHaltedQuotes.Text = oldMarketData[exchange].HaltedQuotes.Count.ToString();
            }
            else if (exchange == "Bybit")
            {
                LabelBybitDatabaseTradingPairs.Text = oldMarketData[exchange].TradingPairs.Keys.Count.ToString();
                LabelBybitDatabaseHaltedPairs.Text = oldMarketData[exchange].HaltedPairs.Keys.Count.ToString();
                LabelBybitDatabaseAllPairs.Text = oldMarketData[exchange].AllPairs.Count.ToString();

                LabelBybitDatabaseTradingBases.Text = oldMarketData[exchange].TradingBases.Count.ToString();
                LabelBybitDatabaseHaltedBases.Text = oldMarketData[exchange].HaltedBases.Count.ToString();
                LabelBybitDatabaseTradingQuotes.Text = oldMarketData[exchange].TradingQuotes.Count.ToString();
                LabelBybitDatabaseHaltedQuotes.Text = oldMarketData[exchange].HaltedQuotes.Count.ToString();
            }
            else if (exchange == "Coinbase")
            {
                LabelCoinbaseTradingPairs.Text = oldMarketData[exchange].TradingPairs.Keys.Count.ToString();
                LabelCoinbaseHaltedPairs.Text = oldMarketData[exchange].HaltedPairs.Keys.Count.ToString();
                LabelCoinbaseAllPairs.Text = oldMarketData[exchange].AllPairs.Count.ToString();

                LabelCoinbaseDatabaseTradingBases.Text = oldMarketData[exchange].TradingBases.Count.ToString();
                LabelCoinbaseDatabaseHaltedBases.Text = oldMarketData[exchange].HaltedBases.Count.ToString();
                LabelCoinbaseDatabaseTradingQuotes.Text = oldMarketData[exchange].TradingQuotes.Count.ToString();
                LabelCoinbaseDatabaseHaltedQuotes.Text = oldMarketData[exchange].HaltedQuotes.Count.ToString();
            }
        }
        public void UpdateNewPairsUI(string exchange)
        {

            var data = newPairs[exchange];
            if (exchange == "Binance")
            {
                if (DropdownBinanceNewpairs.Items.Count > 0 && !DropdownBinanceNewpairs.Items.Cast<string>().ToList().SequenceEqual(data.AllPairs)) { DropdownBinanceNewpairs.Items.Clear(); }
                if (DropdownBinanceNewBase.Items.Count > 0) { DropdownBinanceNewBase.Items.Clear(); }
                if (DropdownBinanceNewQuote.Items.Count > 0) { DropdownBinanceNewQuote.Items.Clear(); }

                foreach (string item in data.AllPairs)
                {
                    DropdownBinanceNewpairs.Items.Add(item);

                    if (!DropdownBinanceNewBase.Items.Contains(data.TradingPairs[item].Base))
                    {
                        DropdownBinanceNewBase.Items.Add(data.TradingPairs[item].Base);
                    }
                }

                if (DropdownBinanceNewpairs.Items.Count > 0 && !DropdownBinanceNewpairs.Items.Cast<string>().ToList().SequenceEqual(data.AllPairs)) { DropdownBinanceNewpairs.SelectedItem = DropdownBinanceNewpairs.Items[0]; }
                if (DropdownBinanceNewBase.Items.Count > 0) { DropdownBinanceNewBase.SelectedItem = DropdownBinanceNewBase.Items[0]; }
                if (DropdownBinanceNewQuote.Items.Count > 0) { DropdownBinanceNewQuote.SelectedItem = DropdownBinanceNewBase.Items[0]; }

                LabelBinanceExchangeNewPairs.Text = data.AllPairs.Count.ToString();
            }
            else if (exchange == "Bybit")
            {
                if (DropdownBybitNewpairs.Items.Count > 0 && !DropdownBybitNewpairs.Items.Cast<string>().ToList().SequenceEqual(data.AllPairs)) { DropdownBybitNewpairs.Items.Clear(); }
                if (DropdownBybitNewBase.Items.Count > 0) { DropdownBybitNewBase.Items.Clear(); }
                if (DropdownBybitNewQuote.Items.Count > 0) { DropdownBybitNewQuote.Items.Clear(); }

                foreach (string item in data.AllPairs)
                {
                    DropdownBybitNewpairs.Items.Add(item);

                    if (!DropdownBybitNewBase.Items.Contains(data.TradingPairs[item].Base))
                    {
                        DropdownBybitNewBase.Items.Add(data.TradingPairs[item].Base);
                    }
                }
                if (DropdownBybitNewpairs.Items.Count > 0 && !DropdownBybitNewpairs.Items.Cast<string>().ToList().SequenceEqual(data.AllPairs)) { DropdownBybitNewpairs.SelectedItem = DropdownBybitNewpairs.Items[0]; }
                if (DropdownBybitNewBase.Items.Count > 0) { DropdownBybitNewBase.SelectedItem = DropdownBybitNewBase.Items[0]; }
                if (DropdownBybitNewQuote.Items.Count > 0) { DropdownBybitNewQuote.SelectedItem = DropdownBybitNewBase.Items[0]; }

                LabelBybitExchangeNewPairs.Text = data.AllPairs.Count().ToString();
            }
            else if (exchange == "Coinbase")
            {
                if (DropdownCoinbaseNewpairs.Items.Count > 0 && !DropdownCoinbaseNewpairs.Items.Cast<string>().ToList().SequenceEqual(data.AllPairs)) { DropdownCoinbaseNewpairs.Items.Clear(); }
                if (DropdownCoinbaseNewBase.Items.Count > 0) { DropdownCoinbaseNewBase.Items.Clear(); }
                if (DropdownCoinbaseNewQuote.Items.Count > 0) { DropdownCoinbaseNewQuote.Items.Clear(); }

                foreach (string item in data.AllPairs)
                {
                    DropdownCoinbaseNewpairs.Items.Add(item);

                    if (!DropdownCoinbaseNewBase.Items.Contains(data.TradingPairs[item].Base))
                    {
                        DropdownCoinbaseNewBase.Items.Add(data.TradingPairs[item].Base);
                    }
                }

                if (DropdownCoinbaseNewpairs.Items.Count > 0 && !DropdownCoinbaseNewpairs.Items.Cast<string>().ToList().SequenceEqual(data.AllPairs)) { DropdownCoinbaseNewBase.SelectedItem = DropdownCoinbaseNewBase.Items[0]; }
                if (DropdownCoinbaseNewBase.Items.Count > 0) { DropdownCoinbaseNewQuote.SelectedItem = DropdownCoinbaseNewBase.Items[0]; }
                if (DropdownCoinbaseNewQuote.Items.Count > 0) { DropdownCoinbaseNewpairs.SelectedItem = DropdownCoinbaseNewpairs.Items[0]; }

                LabelCoinbaseExchangeNewPairs.Text = data.AllPairs.Count.ToString();
            }
        }

        public void UpdateOrderCreateUI()
        {
            MarketData data = new MarketData();
            List<string> bases = new List<string>();
            List<string> quotes = new List<string>();

            foreach (string exchange in AppSettings.Exchanges)
            {
                foreach (string item in newMarketData[exchange].TradingBases.Keys)
                {
                    if (!bases.Contains(item))
                    {
                        bases.Add(item.ToString());
                    }
                }

                foreach (string item in newMarketData[exchange].TradingQuotes.Keys)
                {
                    if (!quotes.Contains(item))
                    {
                        quotes.Add(item.ToString());
                    }
                }
            }

            bases.Sort();
            quotes.Sort();

            DropdownBase.Items.Clear();
            foreach (string item in bases)
            {
                DropdownBase.Items.Add(item);
            }
            DropdownQuote.Items.Clear();
            foreach (string item in quotes)
            {
                DropdownQuote.Items.Add(item);
            }

            DropdownBase.SelectedItem = DropdownBase.Items[0];
            DropdownQuote.SelectedItem = DropdownQuote.Items[0];

        }

        private void ScrollToBottom(object sender, EventArgs e)
        {
            logBox.Invoke((MethodInvoker)(() =>
            {
                logBox.SelectionStart = logBox.Text.Length;
                logBox.ScrollToCaret();
            }));
        }

        #endregion

        #region UI Toolstrip Controls:

        // UI controls and others
        private void BinanceSelectedPairChanged(object sender, EventArgs e)
        {
            string item = DropdownBinanceNewpairs.SelectedItem.ToString();

            DropdownBinanceNewQuote.Items.Clear();

            // DropdownBinanceNewBase.SelectedItem = newPairsList["Binance"].TradingPairs[item].Base;

            // DropdownBinanceNewQuote.Items.Add(newPairsList["Binance"].TradingPairs[item].Quote);
            // DropdownBinanceNewQuote.SelectedItem = DropdownBinanceNewQuote.Items[0];

        }
        private void BybitSelectedPairChanged(object sender, EventArgs e)
        {
            DropdownBybitNewQuote.Items.Clear();
            DropdownBybitNewQuote.SelectedItem = DropdownBybitNewQuote.Items[0];

            //DropdownBybitNewBase.SelectedItem = newPairsList["Bybit"].TradingPairs[item].Base;
            //DropdownBybitNewQuote.Items.Add(newPairsList["Bybit"].TradingPairs[item].Quote);

        }

        // TOOLSTRIP - FILE -> 
        //
        //          ----------- Load Database
        private void ToolstripFileLoadDatabase_Click(object sender, EventArgs e)
        {
            _ = OpenDatabase();
        }
        //          ----------- Save Database (Binance, Bybit, Coinbase...)
        private void SaveNewExchangeData(object sender, EventArgs e)
        {
            string exchange = (sender as ToolStripMenuItem)?.Name.Replace("ToolStripMenuItem", ""); ;
            SaveExchangeDatabase(exchange);
        }
        //          ------------------------ Save All
        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("Are you sure you want to overwrite all market data on this system with currently collected data?", "Save all market data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer == DialogResult.Yes)
            {
                string jsonString = JsonConvert.SerializeObject(newMarketData, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppSettings.DatabaseDirectory, AppSettings.DatabaseFile), jsonString);
                newPairs.Clear();
                foreach (string exchange in AppSettings.Exchanges)
                {
                    UpdateDatabaseDataUI(exchange);

                    newPairs.Add(exchange, new MarketData());

                    UpdateNewPairsUI(exchange);
                }

            }
        }

        //          ----------- Save As...
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveDatabaseAs();
        }
        //          ----------- Minimize to tray
        private void minimizeToTrayToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            AppTrayIcon.Visible = true;
            this.Hide();
        }
        //          ----------- Close
        private void ToolstripFileClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        // TOOLSTRIP - SETTINGS ->
        private void ToolstripSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm(this);
            settingsForm.ShowDialog();
        }


        // TOOLSTRIP - VIEW ->
        //
        //          ----------- Console
        private void ToolstripViewConsole_Click(object sender, EventArgs e)
        {
            ToolstripViewConsole.Checked = !ToolstripViewConsole.Checked;
            if (ToolstripViewConsole.Checked)
            {
                this.Height += PanelBottom.Height;
                PanelBottom.Show();
            }
            else
            {
                PanelBottom.Hide();
                this.Height -= PanelBottom.Height;
            }
        }
        //          ----------- Order Create
        private void ToolstripViewOrderCreate_Click(object sender, EventArgs e)
        {
            ToolstripViewOrderCreate.Checked = !ToolstripViewOrderCreate.Checked;

            if(ToolstripViewOrderCreate.Checked)
            {
                PanelOrderCreate.Show();
                PanelTop.Height += PanelOrderCreate.Height;
                this.Height += PanelOrderCreate.Height;

            }
            else 
            {
                PanelOrderCreate.Hide();
                PanelTop.Height -= PanelOrderCreate.Height;
                this.Height -= PanelOrderCreate.Height;
            }
        }


        // TOOLSTRIP - ABOUT ->
        private void ToolstripAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Degen Ape v1.0\n" +
                "Be Careful...", "About");
        }

        #endregion

        #region UI Create Order Controls:
        // CREATE ORDER CONTROLS
        private void UpdateSizeQuoteDropdown(object sender, EventArgs e)
        {

            DropdownSizeQuote.Items.Clear();
            List<string> list = new List<string>();
            if (DropdownBase.SelectedItem != null) { list.Add(DropdownBase.SelectedItem.ToString()); }
            if (DropdownQuote.SelectedItem != null) { list.Add(DropdownQuote.SelectedItem.ToString()); }
            list.Sort();

            DropdownSizeQuote.Items.AddRange(list.ToArray());
            DropdownSizeQuote.SelectedItem = DropdownSizeQuote.Items[0];
        }
        private void CheckBoxAnyBase_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAnyBase.Checked)
            {
                DropdownBase.Items.Add("!ANY");
                DropdownBase.SelectedItem = "!ANY";
                DropdownBase.Enabled = false;
            }
            else if (!CheckBoxAnyBase.Checked)
            {
                DropdownBase.Items.Remove("!ANY");
                DropdownBase.SelectedItem = DropdownBase.Items[0];
                DropdownBase.Enabled = true;
            }
        }
        private void SelectorDeselectAll(object sender, EventArgs e)
        {
            // SELECT ALL control
            if (CheckboxBinance.Checked && CheckboxBybit.Checked && CheckboxCoinbase.Checked)
            {
                CheckboxSelectAll.Checked = true;
            }
            else
            {
                CheckboxSelectAll.Checked = false;
            }

            // ADD SELECTED EXCHANGE TO LIST
            if (CheckboxBinance.Checked)
            {
                if (!selectedExchanges.Contains("Binance"))
                {
                    selectedExchanges.Add("Binance");
                }
            }
            else
            {
                selectedExchanges.Remove("Binance");
            }
            if (CheckboxBybit.Checked)
            {
                if (!selectedExchanges.Contains("Bybit"))
                {
                    selectedExchanges.Add("Bybit");
                }
            }
            else
            {
                selectedExchanges.Remove("Bybit");
            }
            if (CheckboxCoinbase.Checked)
            {
                if (!selectedExchanges.Contains("Coinbase"))
                {
                    selectedExchanges.Add("Coinbase");
                }
            }
            else
            {
                selectedExchanges.Remove("Coinbase");
            }
        }
        private void CheckboxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckboxSelectAll.Checked == true)
            {
                CheckboxBinance.Checked = true;
                CheckboxBybit.Checked = true;
                CheckboxCoinbase.Checked = true;
            }
            else
            {
                CheckboxBinance.Checked = false;
                CheckboxBybit.Checked = false;
                CheckboxCoinbase.Checked = false;
            }
        }
        private void AddOrder(object sender, EventArgs e)
        {
            // Check if All dropdowns are checked
            if (DropdownBase.SelectedItem != null && DropdownQuote.SelectedItem != null
                && DropdownSide.SelectedItem != null && DropdownType.SelectedItem != null)
            {
                if (decimal.TryParse(BoxSize.Text, out decimal size)
                    && decimal.TryParse(BoxStoploss.Text, out decimal stoploss)
                    && decimal.TryParse(BoxTakeprofit.Text, out decimal takeprofit)
                    && decimal.TryParse(BoxReenter.Text, out decimal reenter)
                    && decimal.TryParse(BoxNumofexec.Text, out decimal numofexec))
                {
                    orderList.Add(new Order
                    {
                        Base = DropdownBase.SelectedItem.ToString(),
                        Quote = DropdownQuote.SelectedItem.ToString(),
                        Side = DropdownSide.SelectedItem.ToString(),
                        Type = DropdownType.SelectedItem.ToString(),
                        Size = size.ToString(),
                        Stoploss = stoploss.ToString(),
                        Takeprofit = takeprofit.ToString(),
                        Reenter = reenter.ToString(),
                        NumberOfExec = numofexec.ToString(),
                        Exchange = selectedExchanges
                    });
                    GridOrders.Rows.Add(
                        DropdownBase.SelectedItem,
                        DropdownQuote.SelectedItem,
                        DropdownSide.SelectedItem,
                        DropdownType.SelectedItem,
                        $"{size} {DropdownSizeQuote.SelectedItem}",
                        stoploss,
                        takeprofit,
                        reenter,
                        numofexec,
                        string.Join(", ", selectedExchanges)
                        );

                    lblOrderException.Visible = false;
                    lblOrderException.Text = "";
                }
                else
                {
                    lblOrderException.Visible = true;
                    lblOrderException.Text = "Size is not number!";
                }
            }
            else
            {
                lblOrderException.Visible = true;
                lblOrderException.Text = "Please fill out all fields!";
            }
        }
        private void ButtonReset_Click(object sender, EventArgs e)
        {
            DropdownBase.SelectedIndex = 0;
            DropdownQuote.SelectedIndex = 0;
            CheckBoxAnyBase.Checked = false;
            DropdownSide.SelectedIndex = 0;
            DropdownType.SelectedIndex = 0;
            BoxSize.Text = string.Empty;
            // DropdownSizeQuote.SelectedIndex = 0;
            BoxStoploss.Text = string.Empty;
            BoxTakeprofit.Text = string.Empty;
            BoxReenter.Text = string.Empty;
            BoxNumofexec.Text = string.Empty;
            CheckboxBinance.Checked = false;
            CheckboxBybit.Checked = false;
            CheckboxCoinbase.Checked = false;

        }
        private void ButtonRemoveorder_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow selectedRow in GridOrders.SelectedRows)
            {
                // Remove the selected row from the DataGridView
                GridOrders.Rows.RemoveAt(selectedRow.Index);
            }
        }
        private void GridOrders_Changed(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (GridOrders.RowCount == 0) { ButtonRemoveorder.Enabled = false; }
            else { ButtonRemoveorder.Enabled = true; }
        }

        private void DisableControls_CreateOrder()
        {

        }
        #endregion

        #region UI Tray Icon Controls:
        private void TrayIcon_DoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            AppTrayIcon.Visible = false;
        }
        private void TrayIconOpenMenu_Click(object sender, EventArgs e)
        {
            // Restore the form from the system tray
            this.Show();
            this.WindowState = FormWindowState.Normal;
            AppTrayIcon.Visible = false;
        }
        private void TrayIconExitMenu_Click(object sender, EventArgs e)
        {
            // Close the application
            Application.Exit();
        }
        #endregion

        // FIX THIS :
        private void ButtonStartApe_Click(object sender, EventArgs e)
        {
            if (!Ape.Enabled)
            {
                pictureBox1.Image = DegenApe.Properties.Resources.apeenabledsmall;
                Icon = logoenabled;
                AppTrayIcon.Icon = logoenabled;
                InitializeApe();
                ApeTickGetCompare(null, EventArgs.Empty);
                Ape.Start();
                ButtonStartApe.Text = "STOP APE";
                statusBox.Text = "APE WORKING";
            }
            else
            {
                pictureBox1.Image = DegenApe.Properties.Resources.apedisabledsmall;
                Icon = logodisabled;
                AppTrayIcon.Icon = logodisabled;
                Ape.Stop();

                ButtonStartApe.Text = "START APE";
                statusBox.Text = "Add orders and start apeing";
            }
        }

        #region OVERRIDE Default Methods:

        // EXIT APPLICATION
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Handle form closing event to minimize to tray instead of closing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                AppTrayIcon.Visible = true;
                this.Hide();
            }
            
            base.OnFormClosing(e);
        }

        #endregion

        #region APE:
        // APE
        public void InitializeApe()
        {
            Ape.Interval = ExchangeSettings["Binance"].TIMEOUT;
            Ape.Tick += ApeTickGetCompare;
        }
        public void ApeTickGetCompare(object sender, EventArgs e)
        {
            foreach (string exchange in AppSettings.Exchanges)
            {
                UpdateNewData(exchange);
            }
        }
        public async Task UpdateNewData(string exchange)
        {
            newMarketDataRaw[exchange] = await GetExchangeData(exchange);
            newMarketData[exchange] = newMarketDataRaw[exchange];
            newPairs[exchange] = await Compare(exchange);
            UpdateNewPairsUI(exchange);
        }

        #endregion

        #region Market Execution:

        private async void CreateOrder(string symbol, string side, string type, string quoteOrderQty)
        {
            // Sign order
            string Sign(string apiSecret, string data)
            {
                using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string queryString = $"symbol={symbol}&side={side}&type={type}&quoteOrderQty={quoteOrderQty}&timestamp={timestamp}";

            string signature = Sign(ExchangeSettings["Binance"].SECRETKey, queryString);

            using (HttpClient client = new HttpClient())
            {
                // Set the base URL for the Binance API
                client.BaseAddress = new Uri(ExchangeSettings["Binance"].APIUrlBase);

                // Add the API key and signature to the request headers
                client.DefaultRequestHeaders.Add("X-MBX-APIKEY", ExchangeSettings["Binance"].APIKey);
                queryString += $"&signature={signature}";

                // Send the POST request to create a market order
                HttpResponseMessage response = await client.PostAsync($"{ExchangeSettings["Binance"].APIUrlOrder}?{queryString}", null);

                // Read the response content
                string jsonContent = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(jsonContent);
                try
                {
                    if (data.status.ToString() == "FILLED")
                    {
                        logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Order {data["status"]}: \r\n";
                        logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Symbol: {data.symbol}: \r\n";
                        logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Side: {data.side}: \r\n";
                        logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Type: {data.type}: \r\n";
                        logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Base Quantity: {data.executedQty}: \r\n";
                        logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Price: {data.fills[0].price}: \r\n";

                        logBox.SelectionStart = logBox.Text.Length;
                        logBox.ScrollToCaret();
                    }
                }
                catch
                {
                    logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Error code: {data.code}\r\n";
                    logBox.Text += $"[{DateTime.Now.ToString("hh:mm:ss tt")}] - Error code: {data.msg}\r\n";
                    logBox.SelectionStart = logBox.Text.Length;
                    logBox.ScrollToCaret();
                }
            }
        }

        #endregion

    }
}