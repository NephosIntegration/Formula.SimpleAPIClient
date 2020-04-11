using System;
using Xunit;
using Formula.SimpleAPIClient;

namespace Formula.SimpleAPIClient.Tests
{
    public class OAuth2ClientTests
    {
        private OAuth2Client client = null;

        public OAuth2ClientTests()
        {
            var config = Helpers.GetConfig();
            this.client = new OAuth2Client(config.OAuth2Config);
        }

        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
    }
}
