using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.Services.Internet;

internal class GetLatency
{    public async Task<long> GoogleDNS()
    {
        try
        {
            // Ping a well-known and stable server, such as Google's public DNS server (8.8.8.8)
            Ping ping = new Ping();
            PingReply reply = ping.Send("8.8.8.8", 1000);

            return reply.RoundtripTime;
        }
        catch (PingException)
        {
            return 9999;
        }
    }
}
