using System;
using Xunit;
using Formula.SimpleAPIClient;

namespace Formula.SimpleAPIClient.Tests
{
    public static class Helpers
    {
        public static SimpleAPIClientConfigModel GetConfig()
        {
            var config = SimpleAPIClientConfigLoader.Get("../../../config.json");
            return config.Instance;
            // var config = new SimpleAPIClientConfigModel {
            //     OAuth2Config = new OAuth2ConfigModel {
            //         AuthServerAddress = "https://secure.?????.com",
            //         ClientId = "?????",
            //         ClientSecret = "????"
            //     }
            // };

            // return config;            
        }
    }
}
