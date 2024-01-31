using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerminalApe.Models.Exchange;

namespace TerminalApe.DAL;

public class FileDatabase
{
    private string? jsonString;
    private dynamic? result;

    public dynamic ReadDatabase(string filepath)
    {
        return null;
    }

    public MarketCache LoadExchangeCache(string filepath, string exchange)
    {
        jsonString = System.IO.File.ReadAllText(filepath);
        result = JsonConvert.DeserializeObject<Dictionary<string, MarketCache>>(jsonString);

        return result[exchange];
    }
}
