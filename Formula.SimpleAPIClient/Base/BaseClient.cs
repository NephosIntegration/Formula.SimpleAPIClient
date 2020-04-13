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
        public virtual TypedStatusBuilder<HttpResponseMessage> HandleNonSuccessfulResponse(HttpResponseMessage responseMessage, TypedStatusBuilder<HttpResponseMessage> currentBuilder)
        {
            if (responseMessage.IsSuccessStatusCode == false)
            {
                currentBuilder.RecordFailure(responseMessage.StatusCode.ToString());
            }
            return currentBuilder;
        }

        public virtual async Task<TypedStatusBuilder<HttpResponseMessage>> GetAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            HttpResponseMessage output = null;
            var tokenStatus = await this.Connector.GetValidTokenAsync();
            var status = tokenStatus.ConvertWithDataAs<HttpResponseMessage>(output);

            if (tokenStatus.IsSuccessful)
            {
                try
                {
                    var apiClient = this.PrepareAPIClient(tokenStatus.Data);

                    output = await (cancellationToken == null ? apiClient.GetAsync(requestUri) : apiClient.GetAsync(requestUri, cancellationToken.Value));

                    if (output.IsSuccessStatusCode == false)
                    {
                        status = this.HandleNonSuccessfulResponse(output, status);
                    }
                }
                catch (Exception ex)
                {
                    status.RecordFailure(ex.Message, ex.Source);
                }
            }

            return status.ConvertWithDataAs(output);
        }

        public virtual async Task<TypedStatusBuilder<String>> GetAsStringAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            var status = await this.GetAsync(requestUri, cancellationToken);
            String output = null;

            if (status.IsSuccessful)
            {
                if (status.Data.Content != null)
                {
                    output = await status.Data.Content.ReadAsStringAsync();
                }
            }

            return status.ConvertWithDataAs(output);
        }

        public virtual async Task<TypedStatusBuilder<JObject>> GetAsJObjectAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            var status = await this.GetAsStringAsync(requestUri, cancellationToken);
            JObject output = null;

            if (status.IsSuccessful)
            {
                if (String.IsNullOrWhiteSpace(status.Data) == false)
                {
                    output = JObject.Parse(status.Data);
                }
            }

            return status.ConvertWithDataAs(output);
        }

        public virtual async Task<TypedStatusBuilder<TType>> GetAsTypeAsync<TType>(string requestUri, CancellationToken? cancellationToken = null)
        {
            var status = await this.GetAsJObjectAsync(requestUri, cancellationToken);
            TType output = default(TType);

            if (status.IsSuccessful)
            {
                output = status.Data.ToObject<TType>();
            }

            return status.ConvertWithDataAs(output);
        }
    }
}