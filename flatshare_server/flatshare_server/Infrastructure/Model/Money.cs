using System.Diagnostics.CodeAnalysis;
using flatshare_server.Infrastructure.Model.Responses;

namespace flatshare_server.Infrastructure.Model;

public class Money
{
    public enum Currency
    {
        PLN,
        EUR
    }
    public required decimal Value { get; set; }
    public required Currency Curr { get; set; }

    public override string ToString()
    {
        return $"{Value} {CurrencyStr()}";
    }
    public string CurrencyStr()
    {
        return Curr switch
        {
            Currency.PLN => "PLN",
            Currency.EUR => "EUR",
            _ => throw new NotImplementedException(),
        };
    }

    public static Currency? ParseCurrency(string value)
    {
        return value.ToUpper() switch
        {
            "PLN" => Currency.PLN,
            "EUR" => Currency.EUR,
            _ => null,
        };
    }

    public decimal ApproxPLNValue()
    {
        return Curr switch
        {
            Currency.PLN => 1.0m * Value,
            Currency.EUR => 4.2m * Value,
            _ => throw new InvalidOperationException($"Invalid currency type"),
        };
    }
}