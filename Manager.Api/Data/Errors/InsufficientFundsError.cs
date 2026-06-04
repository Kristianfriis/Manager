using FluentResults;

namespace Manager.Api.Data.Errors;

public class InsufficientFundsError : Error
{
    public InsufficientFundsError()
        : base("Insufficient funds")
    {
    }
}