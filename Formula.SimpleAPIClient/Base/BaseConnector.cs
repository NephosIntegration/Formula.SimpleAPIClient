using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Formula.SimpleCore;
using IdentityModel.Client;

namespace Formula.SimpleAPIClient
{
    public abstract class BaseConnector<TTokenModel> : IConnector
    where TTokenModel : BaseToken
    {
        protected abstract Task<Status<TokenResponse>> EstablishTokenAsync();
        protected abstract Status<TTokenModel> ParseToken(TokenResponse tokenResponse);

        protected long _tokenExpirationThresholdSeconds = 0;
        public BaseConnector(long tokenExpirationThresholdSeconds = 0)
        {
            this._tokenExpirationThresholdSeconds = tokenExpirationThresholdSeconds;
        }

        protected TokenResponse _currentTokenResponse = null;
        protected DateTime? _tokenSetLocallyAtTime = null;
        protected virtual TokenResponse CurrentTokenResponse
        { 
            get
            {
                return this._currentTokenResponse;
            }

            set
            {
                this._currentTokenParsed = false;
                this._currentToken = null;
                this._currentTokenResponse = value;
                this._tokenSetLocallyAtTime = (value == null ? (DateTime?)null : DateTime.Now);
            }
        }

        protected Boolean _currentTokenParsed = false;
        protected TTokenModel _currentToken = default(TTokenModel);
        public virtual TTokenModel CurrentToken
        {
            get
            {
                if (this._currentTokenParsed == false )
                {
                    if (this.CurrentTokenResponse != null)
                    {
                        var result = this.ParseToken(this.CurrentTokenResponse);
                        if (result.IsSuccessful)
                        {
                            this._currentTokenParsed = true;
                            this._currentToken = result.Data;
                        }
                    }
                }

                return this._currentToken;
            }
        }

        public virtual Boolean IsExpired()
        {
            var expired = true; // Assume expired

            var currentToken = this.CurrentToken;
            if (currentToken != null)
            {
                if (currentToken.ExpiresAt != null)
                {
                    var secondsRemaining = this.CurrentToken.ExpiresAt.Value.Subtract(DateTime.Now).TotalSeconds;
                    // If we have no more time remaining aftr including the threshold, we are now expired.
                    expired = (secondsRemaining - this._tokenExpirationThresholdSeconds) <= 0;
                }
            }

            return expired;
        }

        public virtual BaseConnector<TTokenModel> ForceExpiration()
        {
            this.CurrentToken.ExpiresAt = DateTime.Now.AddMinutes(-1);
            return this;
        }

        public virtual BaseConnector<TTokenModel> Reset()
        {
            this.CurrentTokenResponse = null;
            return this;
        }

        public virtual async Task<Status<TTokenModel>> GetValidTokenAsync()
        {
            var status = new Status<TTokenModel>();

            var tokenAttempted = false;  // We only want to attempt to fetch the token 1 time

            // If we don't have a token response yet
            if (this.CurrentTokenResponse == null)
            {
                // Establish one, and preserve the status (don't preserve the output data)
                tokenAttempted = true;
                var results = await this.EstablishTokenAsync();
                status = results.ConvertWithDataAs<TTokenModel>(null);
            }

            // Ensure the token response is valid
            if (status.IsSuccessful)
            {
                if (this.CurrentTokenResponse == null)
                {
                    status.RecordFailure("This implementation failed to set the token response.", "GetValidTokenResponseAsync");
                }
                else
                {
                    if (this.CurrentTokenResponse.IsError)
                    {
                        status.RecordFailure(this.CurrentTokenResponse.Error, "GetValidTokenResponseAsync");
                    }
                }
            }

            // If we are still successful, and didn't just fetch the token
            if (status.IsSuccessful && tokenAttempted == false)
            {
                // Refetch if it is expired
                if (this.IsExpired())
                {
                    tokenAttempted = true;
                    var results = await this.EstablishTokenAsync();
                    status = results.ConvertWithDataAs<TTokenModel>(null);
                }
            }

            if (status.IsSuccessful)
            {
                status.SetData(this.CurrentToken);
            }

            return status;
        }
    }
}