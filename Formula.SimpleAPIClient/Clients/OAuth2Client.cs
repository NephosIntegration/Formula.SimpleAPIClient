using System;
using System.IO;
using System.Net.Http;
using IdentityModel.Client;
using System.Text.Json;
using Formula.SimpleCore;

namespace Formula.SimpleAPIClient
{
    public class OAuth2Client : BaseClient<OAuth2Connector, OAuth2TokenModel>
    {
        private OAuth2ConfigModel config = null;

        public OAuth2Client(OAuth2ConfigModel config) : base(new OAuth2Connector(config))
        {
            this.config = config;
        }

        public override HttpClient PrepareAPIClient(OAuth2TokenModel token)
        {
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(token.Token);
            return apiClient;
        }
    }
}