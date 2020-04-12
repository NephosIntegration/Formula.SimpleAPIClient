using System;

namespace Formula.SimpleAPIClient
{
    public abstract class BaseToken : IToken
    {
        public virtual String Token { get; set; }
        public virtual DateTime? ExpiresAt { get; set; }
    }
}