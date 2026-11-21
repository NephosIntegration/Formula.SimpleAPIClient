using System;
using Xunit;

namespace Formula.SimpleAPIClient.Tests
{
    public sealed class IntegrationFactAttribute : FactAttribute
    {
        public IntegrationFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("FORMULA_RUN_INTEGRATION_TESTS") != "1")
            {
                Skip = "Set FORMULA_RUN_INTEGRATION_TESTS=1 and provide config.json to run against a real identity server.";
            }
        }
    }
}
