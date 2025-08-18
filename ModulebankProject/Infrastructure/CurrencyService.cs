namespace ModulebankProject.Infrastructure;

public class CurrencyService
{
    public static readonly string[] Currencies =
    [
        "RUB", "USD"
    ];

    public static string GetCurrency(string? value)
    {
        if (value == null) return "";
        foreach (var currency in Currencies)
        {
            if(value.ToUpper() == currency) return currency;
        }

        return "";
    }
}