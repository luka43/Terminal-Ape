using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.Models.Exchange
{
    public class MarketCache
    {
        public Dictionary<string, Pair> TradingPairs { get; set; } = new Dictionary<string, Pair>();
        public Dictionary<string, Pair> HaltedPairs { get; set; } = new Dictionary<string, Pair>();
        public List<string> AllPairs { get; set; } = new List<string>();

        public Dictionary<string, Token> TradingBases { get; set; } = new Dictionary<string, Token>();
        public Dictionary<string, Token> HaltedBases { get; set; } = new Dictionary<string, Token>();

        public Dictionary<string, Token> TradingQuotes { get; set; } = new Dictionary<string, Token>();
        public Dictionary<string, Token> HaltedQuotes { get; set; } = new Dictionary<string, Token>();

        public long Timestamp { get; set; }
    }
}
