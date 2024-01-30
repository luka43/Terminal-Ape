using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.Models.Exchange;
public class Order
{
    public int ID { get; set; } = GenerateOrderID();
    public string Base { set; get; }
    public string Quote { set; get; }
    public string Side { set; get; }
    public string Type { set; get; }
    public string Size { set; get; }
    public string SizeQuote { get; set; }
    public string Stoploss { set; get; }
    public string Takeprofit { set; get; }
    public string Reenter { set; get; }
    public string NumberOfExec { set; get; }
    public List<string> Exchange { set; get; }
    public long Timestamp { set; get; }

    static int GenerateOrderID()
    {
        Random random = new Random();
        int result = random.Next(100000, 1000000);

        return result;
    }
}
