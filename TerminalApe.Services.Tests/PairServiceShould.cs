using TerminalApe.Models.Configuration;
using TerminalApe.Services.Exchanges;

namespace TerminalApe.Services.Tests;

public class PairServiceShould
{
    [Theory]
    [InlineData("binance")]
    public async Task ReturnNonEmptyPairsWhenExchangeIdPassed(string exchange)
    {
        // Arrange
        var ps = new PairService(new List<IExchangeSettings>
        {
            new ExchangeSettings()
            {
                Name = "binance",
                APIUrlBase = "https://api.binance.com",
                APIUrlPairs = "/api/v3/exchangeInfo?permissions=SPOT"
            }
        });

        // Act
        var pairData = await ps.GetPairs(exchange);

        // Assert
        Assert.NotEmpty(pairData);
    }
}