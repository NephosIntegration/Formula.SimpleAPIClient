using System;

namespace Formula.SimpleAPIClient
{
    public class OAuth2TokenModel : BaseToken
    {
        public String access_token { get; set; }
        public long expires_in { get; set; }
        public String token_type { get; set; }
        public String scope { get; set; }
    }
}
