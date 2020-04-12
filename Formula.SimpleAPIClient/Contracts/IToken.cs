using System;

namespace Formula.SimpleAPIClient
{
    public interface IToken
    {
        string Token { get; set; }
        DateTime? ExpiresAt { get; set; }
    }
}