using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Formula.SimpleAPIClient.Tests
{
    public class LocalOAuthTests
    {
        [Fact]
        public async Task ClientRequestsAndRenewsTokenForAuthenticatedRequests()
        {
            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            await using var app = builder.Build();
            var issuedTokens = 0;

            app.MapGet("/.well-known/openid-configuration", (HttpContext context) =>
            {
                var authority = $"{context.Request.Scheme}://{context.Request.Host}";
                return Results.Json(new
                {
                    issuer = authority,
                    jwks_uri = authority + "/keys",
                    token_endpoint = authority + "/token"
                });
            });
            app.MapGet("/keys", () => Results.Json(new { keys = Array.Empty<object>() }));
            app.MapPost("/token", async (HttpContext context) =>
            {
                var form = await context.Request.ReadFormAsync();
                if (form["grant_type"] != "client_credentials" || form["scope"] != "sample" ||
                    context.Request.Headers.Authorization != "Basic dGVzdC1jbGllbnQ6dGVzdC1zZWNyZXQ=")
                {
                    return Results.BadRequest();
                }

                return Results.Json(new
                {
                    access_token = "token-" + Interlocked.Increment(ref issuedTokens),
                    expires_in = 3600,
                    token_type = "Bearer"
                });
            });
            app.MapGet("/resource", (HttpContext context) =>
                context.Request.Headers.Authorization.ToString().StartsWith("Bearer token-")
                    ? Results.Text("authorized")
                    : Results.Unauthorized());

            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>().Addresses.Single();
            var config = new OAuth2ConfigModel
            {
                AuthServerAddress = address,
                ClientId = "test-client",
                ClientSecret = "test-secret",
                Scope = "sample"
            };
            var connector = new OAuth2Connector(config);

            var first = await connector.GetValidTokenAsync();
            Assert.True(first.IsSuccessful, first.Message);
            Assert.False(connector.IsExpired());
            connector.ForceExpiration();
            var second = await connector.GetValidTokenAsync();
            Assert.True(second.IsSuccessful, second.Message);
            Assert.NotEqual(first.Data.Token, second.Data.Token);

            var response = await new OAuth2Client(config).GetAsStringAsync(address + "/resource");
            Assert.True(response.IsSuccessful, response.Message);
            Assert.Equal("authorized", response.Data);
        }
    }
}
