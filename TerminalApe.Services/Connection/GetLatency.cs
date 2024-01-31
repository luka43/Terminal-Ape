using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace TerminalApe.Services.Connection;

public class GetLatency
{

    Ping ping = new Ping();

    public async Task<long> GoogleDNS()
    {
        try
        {
            // Ping a well-known and stable server, such as Google's public DNS server (8.8.8.8)

            PingReply reply = ping.Send("8.8.8.8", 1000);

            return reply.RoundtripTime;
        }
        catch (PingException)
        {
            return 999;
        }
    }
    public async Task<long> ExchangeAPI(string exchange)
    {

        try
        {
            // Ping a well-known and stable server, such as Google's public DNS server (8.8.8.8)

            PingReply reply = ping.Send($"api.{ exchange }.com", 1000);

            return reply.RoundtripTime;
        }
        catch (PingException)
        {
            return 999;
        }
    }
}
