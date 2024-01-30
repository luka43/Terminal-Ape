using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.Models.Exchange;

public class Pair
{
    public string Base { get; set; }
    public string Quote { get; set; }
    public bool Trading { get; set; }
    public long Timestamp { get; set; }
}
