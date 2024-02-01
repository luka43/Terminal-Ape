using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalApe.DAL;
using TerminalApe.Models.Configuration;
using TerminalApe.Models.Exchange;
using TerminalApe.Services.Connection;
using TerminalApe.Services.Exchanges;
using TerminalApe.UI.Properties;
using TerminalApe.UI.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace TerminalApe.UI
{
    public partial class MainAppForm : Form
    {
        private Timers _timer;

        private UpdateUi UpdateUi { get; set; }
        private Logger logger;
        private FileDatabase fileDatabase;
        private PairService pairService;

        private AppSettings appSettings;
        private Dictionary<string, ExchangeSettings> exchangeSettings;

        private Dictionary<string, dynamic> marketCacheRaw;
        private Dictionary<string, MarketCache> marketCacheOld;
        private Dictionary<string, MarketCache> marketCacheNew;
        private Dictionary<string, MarketCache> newPairsCache;

        private Dictionary<string, Order> orderList = new Dictionary<string, Order>();

        public NotifyIcon AppTrayIcon = new NotifyIcon();

        public List<string> selectedExchanges = new List<string>();

        public MainAppForm()
        {
            InitializeComponent();
            InitializeLogger();

            logger.Log("Hi!");
            logger.Log("Initializing application...");

            InitializeTrayIcon();
            InitializeServices();
            InitializeSettings();
            InitializeMarketCache();
            InitializeTimers();
            LoadMarketDatabase();
        }

        #region Initialize methods
        private void InitializeLogger()
        {
            logger = new Logger(this);
        }
        private void InitializeTrayIcon()
        {
            AppTrayIcon.Icon = Properties.Resources.logodisabled;
            AppTrayIcon.Text = "TerminalApe v1.0";
            AppTrayIcon.Visible = false;
            AppTrayIcon.ContextMenuStrip = InitializeTrayContextMenu();
            AppTrayIcon.MouseDoubleClick += TrayIcon_DoubleClick;
        }
        private ContextMenuStrip InitializeTrayContextMenu()
        {            
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open", null, TrayIconOpenMenu_Click);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Start", null, TrayIconOpenMenu_Click);
            contextMenu.Items.Add("Stop", null, TrayIconOpenMenu_Click);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, TrayIconExitMenu_Click);

            return contextMenu;
        }
        private void InitializeServices()
        {
            UpdateUi = new UpdateUi(this);
            fileDatabase = new FileDatabase();
            pairService = new PairService();
        }
        private void InitializeSettings()
        {
            appSettings = new AppSettings().Default();
            exchangeSettings = new ExchangeSettings().Default();
        }
        private void InitializeMarketCache()
        {
            marketCacheRaw = new Dictionary<string, dynamic>();
            marketCacheOld = new Dictionary<string, MarketCache>();
            marketCacheNew = new Dictionary<string, MarketCache>();
            newPairsCache = new Dictionary<string, MarketCache>();
        }
        private void InitializeTimers()
        {
            _timer = new Timers(this);
        }

        private void LoadMarketDatabase()
        {
            logger.Log("Loading market database from file...");

            foreach (string exchange in appSettings.Exchanges)
            {
                logger.Log($"Loading {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");

                marketCacheOld.Add(exchange, fileDatabase.LoadExchangeCache(exchange, Path.Combine(appSettings.DatabaseDirectory, appSettings.DatabaseFile)));

                logger.Log($"Found {marketCacheOld[exchange].AllPairs.Count} pairs in {char.ToUpper(exchange[0]) + exchange.Substring(1)} database.");
                logger.Log($"{char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols added.");
            }

            logger.Log("Getting new market data from servers...");

            foreach (string exchange in appSettings.Exchanges)
            {
                GetUpdate(exchange);
            }
        }
        private async void GetUpdate(string exchange)
        {
            logger.Log($"Getting new {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");
            marketCacheRaw.Remove(exchange);
            marketCacheRaw.Add(exchange, await pairService.GetPairs(exchange));

            logger.Log("Done.");
            logger.Log($"Sorting new {char.ToUpper(exchange[0]) + exchange.Substring(1)} pairs and symbols...");
            marketCacheNew.Remove(exchange);
            marketCacheNew.Add(exchange, pairService.SortPairs(exchange, marketCacheRaw[exchange].result));

            logger.Log($"Got {marketCacheNew[exchange].AllPairs.Count} pairs in {char.ToUpper(exchange[0]) + exchange.Substring(1)} database.");
            logger.Log("Done.");
            logger.Log($"Comparing new {char.ToUpper(exchange[0]) + exchange.Substring(1)} data with old data...");
            newPairsCache.Remove(exchange);
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
        #endregion

        #region UI Tray Icon Controls:
        private void TrayIcon_DoubleClick(object sender, MouseEventArgs e)
        {
            ShowFormAndHideTrayIcon();
        }
        private void TrayIconOpenMenu_Click(object sender, EventArgs e)
        {
            ShowFormAndHideTrayIcon();
        }
        private void TrayIconExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void ShowFormAndHideTrayIcon()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            AppTrayIcon.Visible = false;
        }

        #endregion

        #region UI Updating, events and actions
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
        private void SelectorDeselectAllExchanges(object sender, EventArgs e)
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
        private void ButtonReset_Click(object sender, EventArgs e)
        {
            DropdownBase.SelectedIndex = 0;
            DropdownQuote.SelectedIndex = 0;
            CheckBoxAnyBase.Checked = false;
            DropdownSide.SelectedIndex = 0;
            DropdownType.SelectedIndex = 0;
            BoxSize.Text = "0";
            // DropdownSizeQuote.SelectedIndex = 0;
            BoxStoploss.Text = "0";
            BoxTakeprofit.Text = "0";
            BoxReenter.Text = "0";
            BoxNumofexec.Text = "0";
            CheckboxBinance.Checked = false;
            CheckboxBybit.Checked = false;
            CheckboxCoinbase.Checked = false;
            CheckboxSelectAll.Checked = false;

        }
        private void ButtonRemoveorder_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow selectedRow in GridOrders.SelectedRows)
            {
                orderList.Remove(selectedRow.Cells[0].Value.ToString());
                GridOrders.Rows.RemoveAt(selectedRow.Index);
            }
            string jsonString = JsonConvert.SerializeObject(orderList, Formatting.Indented);
            File.WriteAllText("orderList.json", jsonString);
        }
        private void AddOrder(object sender, EventArgs e)
        {
            bool orderOk = true;
            decimal size;
            decimal stoploss;
            decimal takeprofit;
            decimal reenter;
            decimal numofexec;

            // Check if All dropdowns are checked

            if (selectedExchanges.Count == 0)
            {
                orderOk = false;
                AddOrderException("Chose at least one exchange");
            }
            if (!decimal.TryParse(BoxNumofexec.Text, out numofexec))
            {
                orderOk = false;
                AddOrderException("Num. of exec. is not whole number");
            }
            else { decimal.TryParse(BoxNumofexec.Text, out numofexec); }
            if (!decimal.TryParse(BoxReenter.Text, out reenter))
            {
                orderOk = false;
                AddOrderException("Re-enter is not whole number");
            }
            else { decimal.TryParse(BoxReenter.Text, out reenter); }
            if (!decimal.TryParse(BoxTakeprofit.Text, out takeprofit))
            {
                orderOk = false;
                AddOrderException("Takeprofit is not decimal");
            }
            else { decimal.TryParse(BoxTakeprofit.Text, out takeprofit); }
            if (!decimal.TryParse(BoxStoploss.Text, out stoploss))
            {
                orderOk = false;
                AddOrderException("Stoploss is not decimal");
            }
            else { decimal.TryParse(BoxStoploss.Text, out stoploss); }
            if (!decimal.TryParse(BoxSize.Text, out size))
            {
                orderOk = false;
                AddOrderException("Size is not decimal");
            }
            else { decimal.TryParse(BoxSize.Text, out size); }
            if (DropdownBase.SelectedItem == null)
            {
                orderOk = false;
                AddOrderException("Chose base asset");
            }
            else if (DropdownQuote.SelectedItem == null)
            {
                orderOk = false;
                AddOrderException("Chose quote asset");
            }
            else if (DropdownSide.SelectedItem == null)
            {
                orderOk = false;
                AddOrderException("Chose side of the trade");
            }
            else if (DropdownType.SelectedItem == null)
            {
                orderOk = false;
                AddOrderException("Chose type of the trade");
            }

            if (orderOk)
            {
                string orderID;
                bool idOk = true;
                Order newOrder = new Order()
                {
                    Base = DropdownBase.SelectedItem.ToString(),
                    Quote = DropdownQuote.SelectedItem.ToString(),
                    Side = DropdownSide.SelectedItem.ToString(),
                    Type = DropdownType.SelectedItem.ToString(),
                    Size = size.ToString(),
                    SizeQuote = DropdownSizeQuote.SelectedItem.ToString(),
                    Stoploss = stoploss.ToString(),
                    Takeprofit = takeprofit.ToString(),
                    Reenter = reenter.ToString(),
                    NumberOfExec = numofexec.ToString(),
                    Exchange = selectedExchanges
                };

                orderList.Add(newOrder.ID.ToString(), newOrder);
                GridOrders.Rows.Add(
                    newOrder.ID.ToString(),
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

                string jsonString = JsonConvert.SerializeObject(orderList, Formatting.Indented);
                File.WriteAllText("orderList.json", jsonString);
            }
        }
        private void AddOrderException(string exceptionstring)
        {
            lblOrderException.Visible = true;
            lblOrderException.Text = exceptionstring;
        }
        private void GridOrdersChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (GridOrders.RowCount == 0) { ButtonRemoveorder.Enabled = false; }
            else { ButtonRemoveorder.Enabled = true; }
        }
        #endregion

        #region UI Toolstrip Controls:
        //
        // TOOLSTRIP - FILE -> 
        //
        // Load Database
        //
        private void ToolstripFileLoadDatabase_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            // Set the initial directory to the app folder
            openFileDialog.InitialDirectory = appSettings.Directory;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                marketCacheOld.Clear();

                foreach (string exchange in appSettings.Exchanges)
                {
                    marketCacheOld.Add(exchange, fileDatabase.LoadExchangeCache(exchange, filePath));
                    GetUpdate(exchange);
                }                
            }
        }
        //
        // Save Database (Binance, Bybit, Coinbase...)
        //
        private void ToolstripSaveExchangeCache_Click(object sender, EventArgs e)
        {
            string buttonName = (sender as ToolStripMenuItem)?.Text.Replace("ToolStripMenuItem", "").ToLower();
            DialogResult answer = new DialogResult();

            answer = MessageBox.Show($"Are you sure you want to overwrite { buttonName } cache on this system with currently collected data?", $"Save { buttonName } market cache", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer == DialogResult.Yes)
            {
                if (buttonName == "all")
                {
                    fileDatabase.SaveCache(Path.Combine(appSettings.DatabaseDirectory, appSettings.DatabaseFile), marketCacheNew);
                    marketCacheOld = marketCacheNew;
                    newPairsCache.Clear();
                    foreach (string exchange in appSettings.Exchanges)
                    {
                        GetUpdate(exchange);
                    }
                }
                else
                {
                    marketCacheOld.Remove(buttonName);
                    marketCacheOld[buttonName] = marketCacheNew[buttonName];
                    newPairsCache.Remove(buttonName);

                    fileDatabase.SaveCache(Path.Combine(appSettings.DatabaseDirectory, appSettings.DatabaseFile), marketCacheNew);

                    GetUpdate(buttonName);
                }
            }
        }
        //
        // Save As...
        //
        private void ToolstripSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog openFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                OverwritePrompt = true,
                InitialDirectory = appSettings.DatabaseDirectory
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                fileDatabase.SaveCache(filePath, marketCacheNew);
            }
        }
        //
        // Minimize to tray
        //
        private void ToolstripMinimizeToTray_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            AppTrayIcon.Visible = true;
            this.Hide();
        }
        //
        // Close
        //
        private void ToolstripClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //
        // TOOLSTRIP - SETTINGS ->
        //
        private void ToolstripSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }
        //
        // TOOLSTRIP - ABOUT ->
        //
        private void ToolstripAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Degen Ape v1.0\n" +
                "Be Careful...", "About");
        }
        #endregion

        private void MainAppForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                // Your custom code for minimizing goes here
                AppTrayIcon.Visible = true;
                this.Hide();
            }
        }
    }
}