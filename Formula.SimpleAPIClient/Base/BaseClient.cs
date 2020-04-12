using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Formula.SimpleCore;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;


namespace Formula.SimpleAPIClient
{
    public abstract class BaseClient<TConnector, TToken> : IClient
    where TConnector : BaseConnector<TToken>
    where TToken : BaseToken
    {
        protected TConnector Connector = null;
        public BaseClient(TConnector connector)
        {
            this.Connector = connector;
        }

        public abstract HttpClient PrepareAPIClient(TToken token);

        /// <summary>
        /// Return as much information about a failure as possible.  
        /// Intended to be a overriden by inherited objects that know more about the expected results.
        /// </summary>
        /// <param name="responseMessage">The raw response message returned from the http client</param>
        /// <param name="builder">The current StatusBuilder object to append the failure details to.</param>
        /// <returns>The StatusBuilder after being enriched with failure details</returns>
        public virtual StatusBuilder HandleNonSuccessfulResponse(HttpResponseMessage responseMessage, StatusBuilder builder)
        {
            if (responseMessage.IsSuccessStatusCode == false)
            {
                builder.RecordFailure(responseMessage.StatusCode.ToString());
            }
            return builder;
        }

        public virtual async Task<StatusBuilder> GetAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            var output = await this.Connector.GetValidTokenAsync();

            if (output.IsSuccessful)
            {
                var token = output.GetDataAs<TToken>();
                
                var apiClient = this.PrepareAPIClient(token);

                var response = await (cancellationToken == null ? apiClient.GetAsync(requestUri) : apiClient.GetAsync(requestUri, cancellationToken.Value));

                output.SetData(response);

                if (response.IsSuccessStatusCode == false)
                {
                    output = this.HandleNonSuccessfulResponse(response, output);
                }
            }

            return output;
        }

        public virtual async Task<StatusBuilder> GetAsStringAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            var output = await this.GetAsync(requestUri, cancellationToken);

            if (output.IsSuccessful)
            {
                var response = output.GetDataAs<HttpResponseMessage>();
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    output.SetData(content);
                }
            }

            return output;
        }

        public virtual async Task<StatusBuilder> GetAsJObjectAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            var output = await this.GetAsStringAsync(requestUri, cancellationToken);

            if (output.IsSuccessful)
            {
                var response = output.GetDataAs<String>();
                if (String.IsNullOrWhiteSpace(response) == false)
                {
                    var content = JObject.Parse(response);
                    output.SetData(content);
                }
            }

            return output;
        }

        public virtual async Task<TypedStatusBuilder<TType>> GetAsTypeAsync<TType>(string requestUri, CancellationToken? cancellationToken = null)
        {
            var output = new TypedStatusBuilder<TType>();
            var results = await this.GetAsJObjectAsync(requestUri, cancellationToken);

            if (results.IsSuccessful)
            {
                var data = results.GetDataAs<JObject>();
                var content = data.ToObject<TType>();
                output.SetData(content);
            }
            else
            {
                output.Details = results.Details;
                output.Message = results.Message;
                output.Fail();
            }

            return output;
        }
    }
}