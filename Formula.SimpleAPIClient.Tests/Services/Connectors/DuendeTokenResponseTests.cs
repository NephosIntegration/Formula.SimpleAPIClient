using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using Xunit;

namespace Formula.SimpleAPIClient.Tests
{
    public class DuendeTokenResponseTests
    {
        [Fact]
        public async Task TokenResponseIsParsedAndUsedAsBearerToken()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"sample-token\",\"expires_in\":3600,\"token_type\":\"Bearer\"}", Encoding.UTF8, "application/json")
            };
            using var tokenResponse = await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(response);
            var connector = new TestConnector();
            var before = DateTime.Now.AddSeconds(3600);

            var result = connector.Parse(tokenResponse);

            Assert.True(result.IsSuccessful);
            Assert.Equal("sample-token", result.Data.Token);
            Assert.InRange(result.Data.ExpiresAt.Value, before, DateTime.Now.AddSeconds(3600));
            using var client = new OAuth2Client(new OAuth2ConfigModel()).PrepareAPIClient(result.Data);
            Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("sample-token", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        private class TestConnector : OAuth2Connector
        {
            public TestConnector() : base(new OAuth2ConfigModel()) { }

            public Formula.SimpleCore.Status<OAuth2TokenModel> Parse(TokenResponse response)
            {
                return ParseToken(response);
            }
        }
    }
}
