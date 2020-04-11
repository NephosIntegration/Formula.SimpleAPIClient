using System;

namespace Formula.SimpleAPIClient
{
    public class SimpleAPIClientConfigModel
    {
        public SimpleAPIClientConfigModel()
        {
            this.OAuth2Config = new OAuth2ConfigModel();
        }

        public OAuth2ConfigModel OAuth2Config { get; set; }
    }
}
