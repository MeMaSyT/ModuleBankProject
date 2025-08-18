namespace ModulebankProject.Features.Accounts;

public class AccountHelper
{
    public static bool TryChangeBalance(decimal balance, decimal value, out decimal result)
    {
        result = -1;
        var tempValue = balance + value;
        if (tempValue < 0) return false;

        result = tempValue;
        return true;
    }
}