using System;
using Xunit;
using Formula.SimpleAPIClient;

namespace Formula.SimpleAPIClient.Tests
{
    public static class Helpers
    {
        public static SimpleAPIClientConfigModel GetConfig()
        {
            var config = SimpleAPIClientConfigLoader.Get("config.json");

            return config.Instance;
        }
    }
}
