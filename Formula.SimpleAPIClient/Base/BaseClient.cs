using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Formula.SimpleCore;
using Newtonsoft.Json.Linq;


namespace Formula.SimpleAPIClient
{
    public enum Verb 
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE,
    }

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
        /// <param name="builder">The current Status object to append the failure details to.</param>
        /// <returns>The Status after being enriched with failure details</returns>
        public virtual Status<HttpResponseMessage> HandleNonSuccessfulResponse(HttpResponseMessage responseMessage, Status<HttpResponseMessage> currentBuilder)
        {
            if (responseMessage.IsSuccessStatusCode == false)
            {
                currentBuilder.RecordFailure(responseMessage.StatusCode.ToString());
            }
            return currentBuilder;
        }

        public virtual async Task<Status<HttpResponseMessage>> SendAsync(string requestUri, Verb verb = Verb.GET, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            HttpResponseMessage output = null;
            var tokenStatus = await this.Connector.GetValidTokenAsync();
            var status = tokenStatus.ConvertWithDataAs<HttpResponseMessage>(output);

            if (tokenStatus.IsSuccessful)
            {
                try
                {
                    var apiClient = this.PrepareAPIClient(tokenStatus.Data);

                    switch (verb)
                    {
                        case Verb.POST:
                            output = await (cancellationToken == null ? apiClient.PostAsync(requestUri, content) : apiClient.PostAsync(requestUri, content, cancellationToken.Value));
                            break;
                        case Verb.PUT:
                            output = await (cancellationToken == null ? apiClient.PutAsync(requestUri, content) : apiClient.PutAsync(requestUri, content, cancellationToken.Value));
                            break;
                        case Verb.PATCH:
                            output = await (cancellationToken == null ? apiClient.PatchAsync(requestUri, content) : apiClient.PatchAsync(requestUri, content, cancellationToken.Value));
                            break;
                        case Verb.DELETE:
                            output = await (cancellationToken == null ? apiClient.DeleteAsync(requestUri) : apiClient.DeleteAsync(requestUri, cancellationToken.Value));
                            break;
                        case Verb.GET:
                        default:
                            output = await (cancellationToken == null ? apiClient.GetAsync(requestUri) : apiClient.GetAsync(requestUri, cancellationToken.Value));
                            break;
                    }


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

        public virtual async Task<Status<String>> SendAsStringAsync(string requestUri, Verb verb = Verb.GET, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            var status = await this.SendAsync(requestUri, verb, content, cancellationToken);
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

        public virtual async Task<Status<JObject>> SendAsJObjectAsync(string requestUri, Verb verb = Verb.GET, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            var status = await this.SendAsStringAsync(requestUri, verb, content, cancellationToken);
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

        public virtual async Task<Status<TType>> SendAsTypeAsync<TType>(string requestUri, Verb verb = Verb.GET, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            var status = await this.SendAsJObjectAsync(requestUri, verb, content, cancellationToken);
            TType output = default(TType);

            if (status.IsSuccessful)
            {
                output = status.Data.ToObject<TType>();
            }

            return status.ConvertWithDataAs(output);
        }


        /*******\
        GET
        \*******/
        public virtual Task<Status<HttpResponseMessage>> GetAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsync(requestUri, Verb.GET, null, cancellationToken);
        }

        public virtual Task<Status<String>> GetAsStringAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsStringAsync(requestUri, Verb.GET, null, cancellationToken);
        }

        public virtual Task<Status<JObject>> GetAsJObjectAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsJObjectAsync(requestUri, Verb.GET, null, cancellationToken);
        }

        public virtual Task<Status<TType>> GetAsTypeAsync<TType>(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsTypeAsync<TType>(requestUri, Verb.GET, null, cancellationToken);
        }
 

        /*******\
        DELETE
        \*******/
        public virtual Task<Status<HttpResponseMessage>> DeleteAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsync(requestUri, Verb.DELETE, null, cancellationToken);
        }

        public virtual Task<Status<String>> DeleteAsStringAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsStringAsync(requestUri, Verb.DELETE, null, cancellationToken);
        }

        public virtual Task<Status<JObject>> DeleteAsJObjectAsync(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsJObjectAsync(requestUri, Verb.DELETE, null, cancellationToken);
        }

        public virtual Task<Status<TType>> DeleteAsTypeAsync<TType>(string requestUri, CancellationToken? cancellationToken = null)
        {
            return this.SendAsTypeAsync<TType>(requestUri, Verb.DELETE, null, cancellationToken);
        }
 

        /*******\
        POST
        \*******/
        public virtual Task<Status<HttpResponseMessage>> PostAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsync(requestUri, Verb.POST, content, cancellationToken);
        }

        public virtual Task<Status<String>> PostAsStringAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsStringAsync(requestUri, Verb.POST, content, cancellationToken);
        }

        public virtual Task<Status<JObject>> PostAsJObjectAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsJObjectAsync(requestUri, Verb.POST, content, cancellationToken);
        }

        public virtual Task<Status<TType>> PostAsTypeAsync<TType>(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsTypeAsync<TType>(requestUri, Verb.POST, content, cancellationToken);
        }


        /*******\
        PUT
        \*******/
        public virtual Task<Status<HttpResponseMessage>> PutAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsync(requestUri, Verb.PUT, content, cancellationToken);
        }

        public virtual Task<Status<String>> PutAsStringAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsStringAsync(requestUri, Verb.PUT, content, cancellationToken);
        }

        public virtual Task<Status<JObject>> PutAsJObjectAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsJObjectAsync(requestUri, Verb.PUT, content, cancellationToken);
        }

        public virtual Task<Status<TType>> PutAsTypeAsync<TType>(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsTypeAsync<TType>(requestUri, Verb.PUT, content, cancellationToken);
        }


        /*******\
        PATCH
        \*******/
        public virtual Task<Status<HttpResponseMessage>> PatchAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsync(requestUri, Verb.PATCH, content, cancellationToken);
        }

        public virtual Task<Status<String>> PatchAsStringAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsStringAsync(requestUri, Verb.PATCH, content, cancellationToken);
        }

        public virtual Task<Status<JObject>> PatchAsJObjectAsync(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsJObjectAsync(requestUri, Verb.PATCH, content, cancellationToken);
        }

        public virtual Task<Status<TType>> PatchAsTypeAsync<TType>(string requestUri, HttpContent content = null, CancellationToken? cancellationToken = null)
        {
            return this.SendAsTypeAsync<TType>(requestUri, Verb.PATCH, content, cancellationToken);
        }
        
    }
}