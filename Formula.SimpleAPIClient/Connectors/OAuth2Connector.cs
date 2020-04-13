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
        public async Task<TypedStatusBuilder<DiscoveryDocumentResponse>> GetDiscoveryDocumentAsync()
        {
            var output = new TypedStatusBuilder<DiscoveryDocumentResponse>();

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

        protected override async Task<TypedStatusBuilder<TokenResponse>> EstablishTokenAsync()
        {
            var status = await this.GetDiscoveryDocumentAsync();
            TokenResponse output = null;

            if (status.IsSuccessful)
            {
                var discoveryDoc = status.Data;

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
                    status.RecordFailure(this.CurrentTokenResponse.Error, "GetClientCredentialsAsync");
                }
                else
                {
                    output = this.CurrentTokenResponse;
                    
                }
            }

            return status.ConvertWithDataAs<TokenResponse>(output);
        }

        protected override TypedStatusBuilder<OAuth2TokenModel> ParseToken(TokenResponse tokenResponse)
        {
            var output = new TypedStatusBuilder<OAuth2TokenModel>();

            var tokenModel = tokenResponse.Json.ToObject<OAuth2TokenModel>();
            tokenModel.Token = tokenModel.access_token;
            tokenModel.ExpiresAt = DateTime.Now.AddSeconds(tokenModel.expires_in);
            
            output.SetData(tokenModel);

            return output;
        }
    }
}