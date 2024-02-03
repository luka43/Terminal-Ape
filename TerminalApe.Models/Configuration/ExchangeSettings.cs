namespace TerminalApe.Models.Configuration;

public interface IExchangeSettings
{
    string Name { get; }
    string APIKey { get; set; }
    string SECRETKey { get; set; }
    int TIMEOUT { get; set; }
    string APIUrlBase { get; set; }
    string APIUrlPing { get; set; }
    string APIUrlPairs { get; set; }
    string APIUrlOrder { get; set; }
}

public class ExchangeSettings : IExchangeSettings
{
    public string Name { get; set; }
    public string APIKey { get; set; }
    public string SECRETKey { get; set; }
    public int TIMEOUT { get; set; }
    public string APIUrlBase { get; set; }
    public string APIUrlPing { get; set; }
    public string APIUrlPairs { get; set; }
    public string APIUrlOrder { get; set; }

    public static Dictionary<string, IExchangeSettings> Default() 
    {
        Dictionary<string, IExchangeSettings> result = new Dictionary<string, IExchangeSettings>()
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


