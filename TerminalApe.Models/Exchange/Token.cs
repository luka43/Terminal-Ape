using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.Models.Exchange;

public class Token
{
    public List<string> Quotes { get; set; }
    public List<string> Bases { get; set; }
}
