using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Formula.SimpleCore;
using IdentityModel.Client;


namespace Formula.SimpleAPIClient
{
    public class OAuth2Connector : BaseConnector<OAuth2TokenModel>
    {
        private OAuth2ConfigModel config = null;

        public OAuth2Connector(OAuth2ConfigModel config) : base(config.TokenExpirationThresholdSeconds)
        {
            this.config = config;
        }

        private DiscoveryDocumentResponse discoveryDoc = null;
        public async Task<StatusBuilder> GetDiscoveryDocumentAsync()
        {
            var output = new StatusBuilder();

            if (this.discoveryDoc == null)
            {
                var httpClient = new HttpClient();
                
                var d = await httpClient.GetDiscoveryDocumentAsync(this.config.AuthServerAddress);
                if (d.IsError)
                {
                    output.RecordFailure(d.Error, "GetDiscoveryDocumentAsync");
                }
                else
                {
                    this.discoveryDoc = d;
                }
            }

            if (output.IsSuccessful)
            {
                output.SetData(this.discoveryDoc);
            }
            
            return output;
        }

        protected override async Task<StatusBuilder> EstablishTokenAsync()
        {
            var output = await this.GetDiscoveryDocumentAsync();

            if (output.IsSuccessful)
            {
                var discoveryDoc = output.GetDataAs<DiscoveryDocumentResponse>();

                var httpClient = new HttpClient();

                this.CurrentTokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = discoveryDoc.TokenEndpoint,
                    ClientId = config.ClientId,
                    ClientSecret = config.ClientSecret,

                    Scope = config.Scope,
                });
                
                if (this.CurrentTokenResponse.IsError)
                {
                    output.RecordFailure(this.CurrentTokenResponse.Error, "GetClientCredentialsAsync");
                }
                else
                {
                    output.SetData(this.CurrentTokenResponse);
                }
            }

            return output;
        }

        protected override StatusBuilder ParseToken(TokenResponse tokenResponse)
        {
            var output = new StatusBuilder();

            var tokenModel = tokenResponse.Json.ToObject<OAuth2TokenModel>();
            tokenModel.Token = tokenModel.access_token;
            tokenModel.ExpiresAt = DateTime.Now.AddSeconds(tokenModel.expires_in);
            
            output.SetData(tokenModel);

            return output;
        }
    }
}