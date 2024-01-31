using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalApe.Models.Configuration;

public class ExchangeSettings
{

    public string APIKey { get; set; }
    public string SECRETKey { get; set; }
    public int TIMEOUT { get; set; }
    public string APIUrlBase { get; set; }
    public string APIUrlPing { get; set; }
    public string APIUrlPairs { get; set; }
    public string APIUrlOrder { get; set; }


    public Dictionary<string, ExchangeSettings> Default() 
    {
        Dictionary<string, ExchangeSettings> result = new Dictionary<string, ExchangeSettings>()
        {
            { "binance", new ExchangeSettings()
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
            { "bybit", new ExchangeSettings()
            {
                APIKey = "ENTER APY KEY HERE",
                SECRETKey = "ENTER SECRET KEY HERE",
                TIMEOUT = 6000,

                APIUrlBase = "https://api.bybit.com",
                APIUrlPing = "/v5/market/time",
                APIUrlPairs = "/v5/market/instruments-info?category=spot",
                APIUrlOrder = "/api/v3/order",
            }
            },
            {
            "coinbase", new ExchangeSettings()
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
        return result;            
    }
}


