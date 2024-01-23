using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DegenApe
{
    public partial class SettingsForm : Form
    {
        private MainAppForm MainApp;

        public SettingsForm(MainAppForm mainApp)
        {
            InitializeComponent();
            MainApp = mainApp;
            UpdateUI();
        }
        private static string CreateSignature(string secretKey, string data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        public async void UpdateUI()
        {
            //ENDPOINTS
            //Binance
            BoxBinanceAPIBaseUrl.Text = MainApp.ExchangeSettings["Binance"].APIUrlBase;
            BoxBinanceAPIPingUrl.Text = MainApp.ExchangeSettings["Binance"].APIUrlPing;
            BoxBinanceAPIPairlistUrl.Text = MainApp.ExchangeSettings["Binance"].APIUrlPairs;
            BoxBinanceAPICreateOrderUrl.Text = MainApp.ExchangeSettings["Binance"].APIUrlOrder;

            //Bybit
            BoxBybitAPIBaseUrl.Text = MainApp.ExchangeSettings["Bybit"].APIUrlBase;
            BoxBybitAPIPingUrl.Text = MainApp.ExchangeSettings["Bybit"].APIUrlPing;
            BoxBybitAPIPairlistUrl.Text = MainApp.ExchangeSettings["Bybit"].APIUrlPairs;
            BoxBybitAPICreateOrderUrl.Text = MainApp.ExchangeSettings["Bybit"].APIUrlOrder;

            //Coinbase
            BoxCoinbaseAPIBaseUrl.Text = MainApp.ExchangeSettings["Coinbase"].APIUrlBase;
            BoxCoinbaseAPIPingUrl.Text = MainApp.ExchangeSettings["Coinbase"].APIUrlPing;
            BoxCoinbaseAPIPairlistUrl.Text = MainApp.ExchangeSettings["Coinbase"].APIUrlPairs;
            BoxCoinbaseAPICreateOrderUrl.Text = MainApp.ExchangeSettings["Coinbase"].APIUrlOrder;

            //KEYS
            // Binance
            BoxBinanceAPIKey.Text = MainApp.ExchangeSettings["Binance"].APIKey;
            BoxBinanceSECRETKey.Text = MainApp.ExchangeSettings["Binance"].SECRETKey;
            BoxBinanceTimeout.Text = MainApp.ExchangeSettings["Binance"].TIMEOUT.ToString();

            //Bybit
            BoxBybitAPIKey.Text = MainApp.ExchangeSettings["Bybit"].APIKey;
            BoxBybitSECRETKey.Text = MainApp.ExchangeSettings["Bybit"].SECRETKey;
            BoxBybitTimeout.Text = MainApp.ExchangeSettings["Bybit"].TIMEOUT.ToString();

            //Coinbase
            BoxCoinbaseAPIKey.Text = MainApp.ExchangeSettings["Coinbase"].APIKey;
            BoxCoinbaseSECRETKey.Text = MainApp.ExchangeSettings["Coinbase"].SECRETKey;
            BoxCoinbaseTimeout.Text = MainApp.ExchangeSettings["Coinbase"].TIMEOUT.ToString();

            //Settings
            BoxLogAutosaveInterval.Text = MainApp.AppSettings.LogAutosaveInterval.ToString();

            btnCancel.Text = "Close";
            btnApply.Enabled = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            MainApp.ExchangeSettings["Binance"].APIKey = BoxBinanceAPIKey.Text;
            MainApp.ExchangeSettings["Binance"].SECRETKey = BoxBinanceSECRETKey.Text;
            MainApp.ExchangeSettings["Binance"].TIMEOUT = int.Parse(BoxBinanceTimeout.Text);

            MainApp.ExchangeSettings["Bybit"].APIKey = BoxBybitAPIKey.Text;
            MainApp.ExchangeSettings["Bybit"].SECRETKey = BoxBybitSECRETKey.Text;
            MainApp.ExchangeSettings["Bybit"].TIMEOUT = int.Parse(BoxBybitTimeout.Text);


            MainApp.Log($"Timeout changed to {BoxBinanceTimeout.Text}");

            btnCancel.Text = "Close";
            btnApply.Enabled = false;
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {

            this.Close();
        }

        private void SettingChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = true;
            btnCancel.Text = "Cancel";
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("Are you sure you want to save modified settings?", "Apply changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question); ;
            if(answer == DialogResult.Yes)
            {
                //ENDPOINTS
                //Binance
                MainApp.ExchangeSettings["Binance"].APIUrlBase = BoxBinanceAPIBaseUrl.Text;
                MainApp.ExchangeSettings["Binance"].APIUrlPing = BoxBinanceAPIPingUrl.Text;
                MainApp.ExchangeSettings["Binance"].APIUrlPairs = BoxBinanceAPIPairlistUrl.Text;
                MainApp.ExchangeSettings["Binance"].APIUrlOrder = BoxBinanceAPICreateOrderUrl.Text;

                //Bybit
                MainApp.ExchangeSettings["Bybit"].APIUrlBase = BoxBybitAPIBaseUrl.Text;
                MainApp.ExchangeSettings["Bybit"].APIUrlPing = BoxBybitAPIPingUrl.Text;
                MainApp.ExchangeSettings["Bybit"].APIUrlPairs = BoxBybitAPIPairlistUrl.Text;
                MainApp.ExchangeSettings["Bybit"].APIUrlOrder = BoxBybitAPICreateOrderUrl.Text;

                //Coinbase
                MainApp.ExchangeSettings["Coinbase"].APIUrlBase = BoxCoinbaseAPIBaseUrl.Text;
                MainApp.ExchangeSettings["Coinbase"].APIUrlPing = BoxCoinbaseAPIPingUrl.Text;
                MainApp.ExchangeSettings["Coinbase"].APIUrlPairs = BoxCoinbaseAPIPairlistUrl.Text;
                MainApp.ExchangeSettings["Coinbase"].APIUrlOrder = BoxCoinbaseAPICreateOrderUrl.Text;

                //KEYS
                // Binance
                MainApp.ExchangeSettings["Binance"].APIKey = BoxBinanceAPIKey.Text;
                MainApp.ExchangeSettings["Binance"].SECRETKey = BoxBinanceSECRETKey.Text;
                MainApp.ExchangeSettings["Binance"].TIMEOUT = int.Parse(BoxBinanceTimeout.Text);

                //Bybit
                MainApp.ExchangeSettings["Bybit"].APIKey = BoxBybitAPIKey.Text;
                MainApp.ExchangeSettings["Bybit"].SECRETKey = BoxBybitSECRETKey.Text;
                MainApp.ExchangeSettings["Bybit"].TIMEOUT = int.Parse(BoxBybitTimeout.Text);

                //Coinbase
                MainApp.ExchangeSettings["Coinbase"].APIKey = BoxCoinbaseAPIKey.Text;
                MainApp.ExchangeSettings["Coinbase"].SECRETKey = BoxCoinbaseSECRETKey.Text;
                MainApp.ExchangeSettings["Coinbase"].TIMEOUT = int.Parse(BoxCoinbaseTimeout.Text);

                MainApp.AppSettings.LogAutosaveInterval = int.Parse(BoxLogAutosaveInterval.Text);

                // SAVE SETTINGS
                string jsonString = JsonConvert.SerializeObject(MainApp.AppSettings, Formatting.Indented);
                File.WriteAllText(Path.Combine(MainApp.AppSettings.Directory, MainApp.AppSettings.SettingsFile), jsonString);

                jsonString = JsonConvert.SerializeObject(MainApp.ExchangeSettings, Formatting.Indented);
                File.WriteAllText(Path.Combine(MainApp.AppSettings.Directory, MainApp.AppSettings.ExchangeFile), jsonString);

                this.Close();
            }
            else
            {

            }
        }


    }
}
