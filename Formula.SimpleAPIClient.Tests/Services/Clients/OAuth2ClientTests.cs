using System;
using Xunit;
using Formula.SimpleAPIClient;
using System.Threading.Tasks;

namespace Formula.SimpleAPIClient.Tests
{
    public class OAuth2ClientTests
    {
        private OAuth2Client client = null;
        private SimpleAPIClientConfigModel config = null;

        public OAuth2ClientTests()
        {
            this.config = Helpers.GetConfig();
            this.client = new OAuth2Client(config.OAuth2Config);
        }

        [Fact]
        public async Task CanReceiveContentAsync()
        {
            var response = await this.client.GetAsStringAsync(this.config.OAuth2Config.AuthServerAddress + "/Manage");
            Assert.True(response.IsSuccessful);
            var pageContent = response.Data;
            Assert.NotNull(pageContent);
            Assert.IsType<String>(pageContent);
        }
    }
}
