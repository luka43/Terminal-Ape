using Newtonsoft.Json.Linq;
using System.Dynamic;
using TerminalApe.Models.Configuration;
using TerminalApe.Models.Exchange;

namespace TerminalApe.Services.Exchanges;

public class PairService
{
    private readonly HttpClient NetworkClient = new HttpClient();
    public dynamic settings = new ExchangeSettings().Default();    

    public async Task<dynamic> GetPairs(string exchange)
    {
        dynamic responseData = new ExpandoObject(); ;
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        try
        {
            NetworkClient.DefaultRequestHeaders.Add("User-Agent", "DegenApe");
            HttpResponseMessage response = await NetworkClient.GetAsync(settings[exchange].APIUrlBase + settings[exchange].APIUrlPairs);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                responseData.result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
                responseData.timestamp = timestamp;
            }
            else
            {
                //Log($"Error getting response from the {exchange} server!");
                //Log($"Response status code: {response.IsSuccessStatusCode}.");
            }
        }
        catch (Exception ex)
        {
            //Log($"{ex.Message}");
        }

        return responseData;
    }
    public MarketCache SortPairs(string exchange, dynamic data)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        MarketCache result = new MarketCache();

        if (data != null)
        {
            if (exchange == "binance")
            {
                foreach (var item in data.symbols)
                {
                    // Add pair to ALLPAIRS
                    if (!result.AllPairs.Contains(item.symbol.ToString()))
                    {
                        result.AllPairs.Add(item.symbol.ToString());
                    }

                    // IF TRADING
                    if (item.status.ToString().ToLower() == "trading")
                    {
                        // TRADING PAIRS DOES NOT COINTAIN GIVEN PAIR
                        if (!result.TradingPairs.ContainsKey(item.symbol.ToString()))
                        {
                            // ADD TO TRADING PAIRS
                            result.TradingPairs.Add(item.symbol.ToString(), new Pair()
                            {
                                Base = item.baseAsset.ToString(),
                                Quote = item.quoteAsset.ToString(),
                                Trading = true,
                                Timestamp = timestamp,
                            });
                        }

                        // TRADINGBASES DOES NOT COINTAIN GIVEN SYMBOL
                        if (!result.TradingBases.ContainsKey(item.baseAsset.ToString()))
                        {
                            // CREATE NEW SYMBOL IN TRADINGBASE
                            result.TradingBases.Add(item.baseAsset.ToString(), new Token()
                            {
                                Quotes = new List<string>(),
                                Bases = new List<string>()
                            });
                            result.TradingBases[item.baseAsset.ToString()].Quotes.Add(item.quoteAsset.ToString());
                            result.TradingBases[item.baseAsset.ToString()].Bases.Add(item.baseAsset.ToString());
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
                            result.TradingQuotes.Add(item.quoteAsset.ToString(), new Token()
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
                            result.HaltedPairs.Add(item.symbol.ToString(), new Pair()
                            {
                                Base = item.baseAsset.ToString(),
                                Quote = item.quoteAsset.ToString(),
                                Trading = false,
                                Timestamp = timestamp
                            });
                        }
                        // HALTEDBASES DOES NOT COINTAIN GIVEN SYMBOL
                        if (!result.HaltedBases.ContainsKey(item.baseAsset.ToString()))
                        {
                            // CREATE NEW SYMBOL IN HALTEDBASES
                            result.HaltedBases.Add(item.baseAsset.ToString(), new Token()
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
                            result.HaltedQuotes.Add(item.quoteAsset.ToString(), new Token()
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
            else if (exchange == "bybit")
            {
                foreach (var item in data.result.list)
                {
                    // Add pair to ALLPAIRS
                    if (!result.AllPairs.Contains(item.symbol.ToString()))
                    {
                        result.AllPairs.Add(item.symbol.ToString());
                    }
                    // TRADING
                    if (item.status.ToString().ToLower() == "trading")
                    {
                        // TRADING PAIRS DOES NOT COINTAIN GIVEN PAIR
                        if (!result.TradingPairs.ContainsKey(item.symbol.ToString()))
                        {
                            // ADD TO TRADING PAIRS
                            result.TradingPairs.Add(item.symbol.ToString(), new Pair()
                            {
                                Base = item.baseCoin.ToString(),
                                Quote = item.quoteCoin.ToString(),
                                Trading = true,
                                Timestamp = timestamp
                            });

                        }
                        // TRADINGBASES DOES NOT COINTAIN GIVEN SYMBOL
                        if (!result.TradingBases.ContainsKey(item.baseCoin.ToString()))
                        {
                            // CREATE NEW SYMBOL IN TRADINGBASE
                            result.TradingBases.Add(item.baseCoin.ToString(), new Token()
                            {
                                Quotes = new List<string>(),
                                Bases = new List<string>()
                            });
                            result.TradingBases[item.baseCoin.ToString()].Quotes.Add(item.quoteCoin.ToString());
                            result.TradingBases[item.baseCoin.ToString()].Bases.Add(item.baseCoin.ToString());
                        }
                        // TRADINGBASES COINTAINS GIVEN SYMBOL
                        else
                        {
                            // TRADINGBASES DOES NOT COINTAIN GIVEN QUOTE
                            if (!result.TradingBases[item.baseCoin.ToString()].Quotes.Contains(item.quoteCoin.ToString()))
                            {
                                // ADD NEW QUOTE TO QUOTES
                                result.TradingBases[item.baseCoin.ToString()].Quotes.Add(item.quoteCoin.ToString());
                            }
                        }

                        if (!result.TradingQuotes.ContainsKey(item.quoteCoin.ToString()))
                        {
                            // CREATE NEW SYMBOL IN TRADINGQUOTES
                            result.TradingQuotes.Add(item.quoteCoin.ToString(), new Token()
                            {
                                Quotes = new List<string>(),
                                Bases = new List<string>()
                            });
                            result.TradingQuotes[item.quoteCoin.ToString()].Bases.Add(item.baseCoin.ToString());
                            result.TradingQuotes[item.quoteCoin.ToString()].Quotes.Add(item.quoteCoin.ToString());
                        }
                        // TRADINGQUOTES COINTAINS GIVEN SYMBOL
                        else
                        {
                            // TRADINGQUOTES DOES NOT COINTAIN GIVEN QUOTE
                            if (!result.TradingQuotes[item.quoteCoin.ToString()].Bases.Contains(item.baseCoin.ToString()))
                            {
                                // ADD NEW BASE TO BASES
                                result.TradingQuotes[item.quoteCoin.ToString()].Bases.Add(item.baseCoin.ToString());
                            }
                        }
                    }
                    // HALTED
                    else
                    {
                        // HALTED PAIRS DOES NOT COINTAIN GIVEN PAIR
                        if (!result.HaltedPairs.ContainsKey(item.symbol.ToString()))
                        {
                            // ADD TO TRADING PAIRS
                            result.TradingPairs.Add(item.symbol.ToString(), new Pair()
                            {
                                Base = item.baseCoin.ToString(),
                                Quote = item.quoteCoin.ToString(),
                                Trading = true,
                                Timestamp = timestamp
                            });

                        }
                        // TRADINGBASES DOES NOT COINTAIN GIVEN SYMBOL
                        if (!result.HaltedBases.ContainsKey(item.baseCoin.ToString()))
                        {
                            // CREATE NEW SYMBOL IN TRADINGBASE
                            result.HaltedBases.Add(item.baseCoin.ToString(), new Token()
                            {
                                Quotes = new List<string>(),
                                Bases = new List<string>()
                            });
                            result.HaltedBases[item.baseCoin.ToString()].Quotes.Add(item.quoteCoin.ToString());
                            result.HaltedBases[item.baseCoin.ToString()].Bases.Add(item.baseCoin.ToString());
                        }
                        // TRADINGBASES COINTAINS GIVEN SYMBOL
                        else
                        {
                            // TRADINGBASES DOES NOT COINTAIN GIVEN QUOTE
                            if (!result.HaltedBases[item.baseCoin.ToString()].Quotes.Contains(item.quoteCoin.ToString()))
                            {
                                // ADD NEW QUOTE TO QUOTES
                                result.HaltedBases[item.baseCoin.ToString()].Quotes.Add(item.quoteCoin.ToString());
                            }
                        }

                        if (!result.HaltedQuotes.ContainsKey(item.quoteCoin.ToString()))
                        {
                            // CREATE NEW SYMBOL IN TRADINGQUOTES
                            result.HaltedQuotes.Add(item.quoteCoin.ToString(), new Token()
                            {
                                Quotes = new List<string>(),
                                Bases = new List<string>()
                            });
                            result.HaltedQuotes[item.quoteCoin.ToString()].Bases.Add(item.baseCoin.ToString());
                            result.HaltedQuotes[item.quoteCoin.ToString()].Quotes.Add(item.quoteCoin.ToString());
                        }
                        // TRADINGQUOTES COINTAINS GIVEN SYMBOL
                        else
                        {
                            // TRADINGQUOTES DOES NOT COINTAIN GIVEN QUOTE
                            if (!result.HaltedQuotes[item.quoteCoin.ToString()].Bases.Contains(item.baseCoin.ToString()))
                            {
                                // ADD NEW BASE TO BASES
                                result.HaltedQuotes[item.quoteCoin.ToString()].Bases.Add(item.baseCoin.ToString());
                            }
                        }
                    }
                }
            }
            else if (exchange == "coinbase")
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
                            result.TradingPairs.Add(item.id.ToString().Replace("-", ""), new Pair()
                            {
                                Base = item.base_currency.ToString(),
                                Quote = item.quote_currency.ToString(),
                                Trading = true,
                                Timestamp = timestamp
                            });
                        }
                        // TRADING BASE SYMBOL DOES NOT COINTAIN GIVEN ITEM
                        // NEW BASE SYMBOL DETECTED
                        if (!result.TradingBases.ContainsKey(item.base_currency.ToString()))
                        {
                            // ADD TO TRADINGBASE
                            // ADD QUOTE ASSET
                            result.TradingBases.Add(item.base_currency.ToString(), new Token()
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
                            result.TradingQuotes.Add(item.quote_currency.ToString(), new Token()
                            {
                                Bases = new List<string>()
                            });
                            result.TradingQuotes[item.quote_currency.ToString()].Bases.Add(item.base_currency.ToString());
                        }
                    }

                    else
                    {

                        if (!result.HaltedPairs.ContainsKey(item.id.ToString().Replace("-", "")))
                        {
                            result.HaltedPairs.Add(item.id.ToString().Replace("-", ""), new Pair()
                            {
                                Base = item.base_currency.ToString(),
                                Quote = item.quote_currency.ToString(),
                                Trading = true,
                                Timestamp = timestamp
                            });
                        }
                        // TRADING BASE SYMBOL DOES NOT COINTAIN GIVEN ITEM
                        // NEW BASE SYMBOL DETECTED
                        if (!result.HaltedBases.ContainsKey(item.base_currency.ToString()))
                        {
                            // ADD TO TRADINGBASE
                            // ADD QUOTE ASSET
                            result.HaltedBases.Add(item.base_currency.ToString(), new Token()
                            {
                                Quotes = new List<string>()
                            });
                            result.HaltedBases[item.base_currency.ToString()].Quotes.Add(item.quote_currency.ToString());
                        }
                        // TRADING QUOTE SYMBOL DOES NOT COINTAIN GIVEN ITEM
                        // NEW QUOTE SYMBOL DETECTED
                        if (!result.HaltedQuotes.ContainsKey(item.quote_currency.ToString()))
                        {
                            // ADD TO TRADINGQUOTES
                            // ADD BASE ASSET
                            result.HaltedQuotes.Add(item.quote_currency.ToString(), new Token()
                            {
                                Bases = new List<string>()
                            });
                            result.HaltedQuotes[item.quote_currency.ToString()].Bases.Add(item.base_currency.ToString());
                        }
                    }
                }
            }
            else
            {
                // ERROR: WRONG EXCHANGE STRING GIVEN
                return null;
            }
            result.AllPairs.Sort();
            result.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return result;
        }
        else
        {
            // ERROR: DATA = NULL
            return null;
        }
    }
    public MarketCache Compare(string exchange, MarketCache dataOld, MarketCache dataNew)
    {
        MarketCache result = new MarketCache();
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // GET ALL INTRUDERS AND ADD THEM TO ALLPAIRS
        // ADD ALL INTRUDERS TO ALLPAIRS
        if (dataOld != null && dataNew != null)
        {
                result.AllPairs = dataNew.AllPairs.Except(dataOld.AllPairs).ToList();
                result.AllPairs.Sort();

                foreach (string item in result.AllPairs)
                {
                    try
                    {
                    // TRADING
                    if (dataNew.TradingPairs[item].Trading)
                        {
                            // ADD INTRUDERS TO TRADINGPAIRS
                            result.TradingPairs.Add(item, new Pair()
                            {
                                Base = dataNew.TradingPairs[item].Base.ToString(),
                                Quote = dataNew.TradingPairs[item].Quote.ToString(),
                                Trading = true,
                                Timestamp = timestamp
                            });
                            // TRADINGBASES DOES NOT COINTAIN GIVEN SYMBOL
                            if (!result.TradingBases.ContainsKey(dataNew.TradingPairs[item].Base))
                            {
                                // ADD TRADINGBASES
                                result.TradingBases.Add(dataNew.TradingPairs[item].Base, new Token()
                                {
                                    Quotes = new List<string>(),
                                    Bases = new List<string>(),
                                });
                                result.TradingBases[dataNew.TradingPairs[item].Base].Bases.Add(dataNew.TradingPairs[item].Base);
                                result.TradingBases[dataNew.TradingPairs[item].Base].Quotes.Add(dataNew.TradingPairs[item].Quote);
                            }
                            else
                            {
                                if (!result.TradingBases[result.TradingPairs[item].Base].Quotes.Contains(result.TradingPairs[item].Quote))
                                {
                                    result.TradingBases[dataNew.TradingPairs[item].Base].Quotes.Add(dataNew.TradingPairs[item].Quote);
                                }
                            }
                        }
                        // ADD INTRUDERS TO HALTEDPAIRS
                        else
                        {
                            result.HaltedPairs.Add(item, new Pair()
                            {
                                Base = dataNew.HaltedPairs[item].Base.ToString(),
                                Quote = dataNew.HaltedPairs[item].Quote.ToString(),
                                Trading = false,
                                Timestamp = timestamp
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log("Error while extracting dataNew...");
                        // Log($"{ex.Message}");
                    }
                }
            return result;
        }

        return null;

    }
}