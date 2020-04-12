# Formula.SimpleAPIClient
Easily and and securely consume API's from c#.

## Goals
* Provide an easy way of accessing API's behind various authentication schemes
    * OAuth2 - (initially using Client Credentials Grant).
    * Others ...
* Apply Transient Exception Handling
    * Configurable - Try / Retry logic
* Apply Circuit Breaker patterns
    * You should be able to gracefully handle catastrophic service failures

## Example of use
This example is against a resource based RESTful API, protected by OAuth2 as a resource server.  
For building API's like this, see the following projects.

- [Formula.SimpleAPI](https://github.com/NephosIntegration/Formula.SimpleCore)
- [Formula.SimpleResourceServer](https://github.com/NephosIntegration/Formula.SimpleResourceServer)
- [Formula.SimpleRepo](https://github.com/NephosIntegration/Formula.SimpleRepo)

```c#
using System;
using System.Threading.Tasks;
using Formula.SimpleAPIClient;
using System.Collections.Generic;

namespace MyApp
{
    /// <summary>
    /// A basic model we hope to receive from our API
    /// </summary>
    public class ProductTypeModel
    {
        public int Id { get; set; }

        public String ProductType { get; set; }
    }

    /// <summary>
    /// A wrapper around an API we wish to consume
    /// </summary>
    public class APIClient
    {
        protected OAuth2Client client = null;

        /// <summary>
        /// In our constructor we will wire up an OAuth2 Client
        /// As we will be calling an API that is secured.
        /// 
        /// All of the token negotiation will be handled for us
        /// </summary>
        public APIClient()
        {
            var config = new OAuth2ConfigModel() {
                AuthServerAddress = "http://localhost:5000",
                ClientId = "my-client-id",
                ClientSecret = "my-client-secret",
                Scope = null,
                TokenExpirationThresholdSeconds = 60 // Fetch new token if we are within 60 seconds of expiration
            };

            this.client = new OAuth2Client(config);
        }

        /// <summary>
        /// Here is where we will provide a function to get product types from 
        /// an API endpoint
        /// </summary>
        /// <returns>A list of product types</returns>
        public async Task<List<ProductTypeModel>> GetProductTypes()
        {
            // Our goal is to return a list of product types
            List<ProductTypeModel> productTypes = null;
            var endpointUri = "http://localhost:6100/test/";

            // Get the data from the the API and we are done
            // Your API endpoint might not return a payload like this, but for this example,
            // The API endpoint will return an envelope of type SimpleAPIResponse and the data within the envelop will
            // be a list of product types
            var resultsOfAPICall = await this.client.GetAsTypeAsync<SimpleAPIResponse<List<ProductTypeModel>>>(endpointUri);

            // There are many things that could have happened prevending us from making it
            // to our API.. We aren't authorized.. Services are down.. Network problems etc..
            // We have an opportunity to gracefully handle these
            if (resultsOfAPICall.IsSuccessful)
            {
                // We now know we made it to our API endpoint, 
                // let's unpack the data it returned
                // This will be whatever your API call returns as it's payload
                var payloadFromEndpoint = resultsOfAPICall.Data;

                // Your payload may vary, and may be a string or another complex type.. In this case our API returns 
                // a custom response envelope (built using Formula.SimpleAPI), so let's check it.
                if (payloadFromEndpoint.IsSuccessful)
                {
                    // If the API endpoint says it did it's work successfully
                    // So, let's unpack the products
                    productTypes = payloadFromEndpoint.Data;
                    foreach(var productType in productTypes)
                    {
                        Console.WriteLine(productType.ProductType);
                    }
                }
                else
                {
                    Console.WriteLine("Handle the specific of a failure as defined by your API");
                }
            }
            // We had some sort of failure and never made it to our API
            else
            {
                // Why did it fail?
                Console.WriteLine(resultsOfAPICall.Message);

                // Display the details (which is a key value pair)
                foreach(var detail in resultsOfAPICall.Details)
                {
                    Console.WriteLine("Key = " + detail.Key);
                    Console.WriteLine("Value = " + detail.Value);
                }
            }

            // Now that we are done playing, let's return the product types back to whatever called us as promised.
            return productTypes;
        }

    }
}
```

# Packages / Projects Used
- [Formula.SimpleCore](https://github.com/NephosIntegration/Formula.SimpleCore)
- [IdentityModel](https://github.com/IdentityModel/IdentityModel)
