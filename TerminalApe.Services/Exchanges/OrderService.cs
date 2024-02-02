using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TerminalApe.Models.Exchange;
using TerminalApe.Models.Configuration;

namespace TerminalApe.Services.Exchanges;

public class OrderService
{
    private ExchangeSettings exchangeSettings { get; } = new ExchangeSettings();
    

    public async Task<dynamic> Execute(string exchange, string secretKey, Order order)
    {

        // Sign order

        if (exchange == "Binance")
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string queryString =
                $"symbol={order.Base + order.Quote}" +
                $"&side={order.Side}" +
                $"&type={order.Type}" +
                $"&quoteOrderQty={order.Size}" +
                $"&timestamp={timestamp}";

            string header = "X-MBX-APIKEY";

            string signature = Sign(secretKey, queryString);

            using (HttpClient client = new HttpClient())
            {
                // Set the base URL for the Binance API
                try
                {
                    client.BaseAddress = new Uri(exchangeSettings.Default()[exchange].APIUrlBase);

                    // Add the API key and signature to the request headers
                    client.DefaultRequestHeaders.Add(header, exchangeSettings.Default()[exchange].APIKey);
                    queryString += $"&signature={signature}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"({order}) Error: {ex.Message}");
                }
                // Send the POST request to create a market order
                HttpResponseMessage response = await client.PostAsync($"{exchangeSettings.Default()[exchange].APIUrlOrder}?{queryString}", null);

                // Read the response content
                string jsonContent = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(jsonContent);
                try
                {
                    if (data.status.ToString() == "FILLED")
                    {
                        Console.WriteLine($"Order {data["status"]}");
                        Console.WriteLine($"Symbol: {data.symbol}");
                        Console.WriteLine($"Side: {data.side}");
                        Console.WriteLine($"Type: {data.type}");
                        Console.WriteLine($"Base Quantity: {data.executedQty}");
                        Console.WriteLine($"Price: {data.fills[0].price}");

                        return data;
                    }
                    else
                    {
                        Console.WriteLine($"({order}) Error code for : {data.code}");
                        Console.WriteLine($"({order}) Error code: {data.msg}");
                        return data;
                    }

                }
                catch
                {
                    Console.WriteLine($"({order}) Error code for : {data.code}");
                    Console.WriteLine($"({order}) Error code: {data.msg}");

                    return data;
                }
            }
        }
        else
        {
            return null;
        }        
    }

    private string Sign(string apiSecret, string data)
    {
        using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

    }
}
