using System;

namespace Formula.SimpleAPIClient
{
    public class OAuth2ConfigModel
    {
        public String AuthServerAddress { get; set; }
        public String ClientId { get; set; }
        public String ClientSecret { get; set; }
        public String Scope { get; set; }
        public long TokenExpirationThresholdSeconds { get; set; }
    }
}
