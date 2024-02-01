using System;
using System.Collections.Generic;

namespace TerminalApe.Models.Exchange;

public class Order
{
    private static Random random = new Random();

    public int ID { get; set; } = GenerateOrderID();
    public string Base { get; set; }
    public string Quote { get; set; }
    public string Side { get; set; }
    public string Type { get; set; }
    public string Size { get; set; }
    public string SizeQuote { get; set; }
    public string Stoploss { get; set; }
    public string Takeprofit { get; set; }
    public string Reenter { get; set; }
    public string NumberOfExec { get; set; }
    public List<string> Exchange { get; set; }
    public long Timestamp { get; set; }

    private static int GenerateOrderID()
    {
        return random.Next(100000, 1000000);
    }
}