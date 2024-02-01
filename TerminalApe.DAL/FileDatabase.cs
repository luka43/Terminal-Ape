using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerminalApe.Models.Configuration;
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

    public MarketCache LoadExchangeCache(string exchange, string filepath)
    {
        try
        {
            jsonString = System.IO.File.ReadAllText(filepath);
            result = JsonConvert.DeserializeObject<Dictionary<string, MarketCache>>(jsonString);
        }
        catch
        {
            return null;
        }


        return result[exchange];
    }
    public bool SaveCache(string filepath, Dictionary<string, MarketCache> cache) 
    {
        try
        {
            string jsonString = JsonConvert.SerializeObject(cache, Formatting.Indented);
            File.WriteAllText(filepath, jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
