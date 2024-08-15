
namespace MicrosoftEmbedPowerBI.Services
{
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using MicrosoftEmbedPowerBI.Models;
    using System;
    using System.Linq;
    using System.Security;

    public class AadService
    {
        private readonly IOptions<AzureAd> azureAd;

        public AadService(IOptions<AzureAd> azureAd)
        {
            this.azureAd = azureAd;
        }

        /// <summary>
        /// Generates and returns Access token
        /// </summary>
        /// <returns>AAD token</returns>
        public string GetAccessToken(string tenant)
        {
            AuthenticationResult? authenticationResult = null;
            // For app only authentication, we need the specific tenant id in the authority url
            var tenantSpecificUrl = azureAd.Value.Instance + tenant + "/";

            // Create a confidential client to authorize the app with the AAD app
            IConfidentialClientApplication clientApp = ConfidentialClientApplicationBuilder
                                                                            .Create(azureAd.Value.ClientId)
                                                                            .WithClientSecret(azureAd.Value.ClientSecret)
                                                                            .WithAuthority(tenantSpecificUrl)
                                                                            .Build();
            // Make a client call if Access token is not available in cache
            authenticationResult = clientApp.AcquireTokenForClient(azureAd.Value.ScopeBase).ExecuteAsync().Result;

            return authenticationResult.AccessToken;

        }
    }
}