using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using TerminalApe.Models.Configuration;
using TerminalApe.Models.Exchange;
using TerminalApe.Services.App;
using TerminalApe.Services.Exchanges;

Logger logger = new Logger();

AppSettings appSettings = new AppSettings().Default();
PairService pairService = new PairService();

Dictionary<string, dynamic> marketCacheRaw = new Dictionary<string, dynamic>();

Dictionary<string, MarketCache> marketCacheNew = new Dictionary<string, MarketCache>();
Dictionary<string, MarketCache> newPairsCache = new Dictionary<string, MarketCache>();

string jsonString;
jsonString = System.IO.File.ReadAllText("marketCacheOld.json");

Dictionary<string, MarketCache> marketCacheOld = JsonConvert.DeserializeObject<Dictionary<string, MarketCache>>(jsonString);

foreach (string exchange in appSettings.Exchanges)
{
    // GET RAW DATA
    logger.Log($"> Exchange : {exchange}");
    marketCacheRaw[exchange] = await pairService.GetPairs(exchange);
    logger.Log("> Data: " + marketCacheRaw[exchange].result != null);
    logger.Log("> Timestamp: " + marketCacheRaw[exchange].timestamp);

    // SORT RAW DATA
    logger.Log("Sort data : ");

    marketCacheNew[exchange] = pairService.SortPairs(exchange, marketCacheRaw[exchange].result);

    logger.Log(marketCacheOld[exchange].TradingPairs.Keys.Count);  

    // COMPARE DATA
    logger.Log("Compare data : ");

    newPairsCache[exchange] = pairService.Compare(exchange, marketCacheOld[exchange], marketCacheNew[exchange]);

    foreach (string pair in newPairsCache[exchange].TradingPairs.Keys)
    {
        logger.Log(pair);
    }

    Console.ReadLine();
}

logger.Log("\n> Save data : ");
Console.ReadLine();

// SAVE ALL DATA
jsonString = JsonConvert.SerializeObject(marketCacheRaw, Formatting.Indented);
File.WriteAllText("marketCacheRaw.json", jsonString);

jsonString = JsonConvert.SerializeObject(marketCacheNew, Formatting.Indented);
File.WriteAllText("marketCacheNew.json", jsonString);
jsonString = JsonConvert.SerializeObject(newPairsCache, Formatting.Indented);
File.WriteAllText("newPairsCache.json", jsonString);


