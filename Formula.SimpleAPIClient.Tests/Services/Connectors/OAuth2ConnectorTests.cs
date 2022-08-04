using System;
using Xunit;
using System.Threading.Tasks;

namespace Formula.SimpleAPIClient.Tests
{
    public class OAuth2ConnectorTests
    {
        private OAuth2Connector connector = null;

        public OAuth2ConnectorTests()
        {
            var config = Helpers.GetConfig();
            this.connector = new OAuth2Connector(config.OAuth2Config);
        }

        [Fact]
        public async Task CanGetValidToken()
        {
            var results = await this.connector.Reset().GetValidTokenAsync();
            Assert.True(results.IsSuccessful, results.Message);

            // Should be a token response
            var token = results.Data;
            Assert.NotNull(token);
            Assert.False(String.IsNullOrEmpty(token.Token), "Token was empty!");
            Assert.NotNull(token.ExpiresAt);
            Assert.False(this.connector.IsExpired(), "Token shouldn't be expired yet, as it's brand new!");
        }

        [Fact]
        public async Task CanRenewAnExpiredToken()
        {
            var results = await this.connector.Reset().GetValidTokenAsync();
            Assert.True(results.IsSuccessful, results.Message);
            
            Assert.False(this.connector.IsExpired(), "Token shouldn't be expired yet, as it's brand new!");
            Assert.True(this.connector.ForceExpiration().IsExpired(), "Token should have been expired, when forced to do so!");

            // Now a new token should be fetched automatically.
            var oldToken = results.Data.Token;

            // If we don't pause for a little bit, we will immediately be re-issued the same token.
            System.Threading.Thread.Sleep(5000);

            results = await this.connector.GetValidTokenAsync();
            Assert.True(results.IsSuccessful, results.Message);

            Assert.False(this.connector.IsExpired(), "Should have fetched a new token that has not expired yet!");
            var tokensAreSameValue = results.Data.Token.Equals(oldToken);
            Assert.False(tokensAreSameValue, "The tokens are still the same, but we are expecting a new token to have been fetched!");
        }
    }
}
