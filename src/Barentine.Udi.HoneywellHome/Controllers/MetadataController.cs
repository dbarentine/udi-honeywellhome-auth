// --------------------------------------------------------------------------------------------------------------------
// <copyright file="auth.cs" company="One Identity Inc.">
//   ONE IDENTITY LLC. PROPRIETARY INFORMATION
// 
//   This software is confidential.  One Identity, LLC. or one of its affiliates or
//   subsidiaries, has supplied this software to you under terms of a
//   license agreement, nondisclosure agreement or both.
// 
//   You may not copy, disclose, or use this software except in accordance with
//   those terms.
// 
//   Copyright One Identity LLC.
//   ALL RIGHTS RESERVED.
// 
//   ONE IDENTITY LLC. MAKES NO REPRESENTATIONS OR
//   WARRANTIES ABOUT THE SUITABILITY OF THE SOFTWARE,
//   EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
//   TO THE IMPLIED WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE, OR
//   NON-INFRINGEMENT.  ONE IDENTITY LLC. SHALL NOT BE
//   LIABLE FOR ANY DAMAGES SUFFERED BY LICENSEE
//   AS A RESULT OF USING, MODIFYING OR DISTRIBUTING
//   THIS SOFTWARE OR ITS DERIVATIVES.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace Barentine.Udi.HoneywellHome.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Barentine.Udi.HoneywellHome.Models;
    using Flurl;
    using IdentityModel.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [Route("api/[controller]")]
    public class MetadataController : Controller
    {
        private readonly Uri apiBaseUri;
        private readonly HttpClient httpClient;
        private readonly ILogger<MetadataController> logger;

        public MetadataController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<MetadataController> logger)
        {
            this.apiBaseUri = new Uri(configuration["HoneywellApiBaseUri"]);
            this.httpClient = httpClientFactory.CreateClient(); 
            this.logger = logger;
        }
        
        [HttpPost]
        [Route("users")]
        public async Task<IActionResult> GetUsers([FromBody] CodeExchangeRequest request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Code) ||
                string.IsNullOrEmpty(request.ClientId) ||
                string.IsNullOrEmpty(request.ClientSecret))
            {
                return BadRequest();
            }

            try
            {
                Url url = new Url(apiBaseUri).AppendPathSegment("oauth2/token");
                
                TokenResponse token = await httpClient.RequestAuthorizationCodeTokenAsync(
                    new AuthorizationCodeTokenRequest
                    {
                        Address = url.ToString(),
                        ClientId = request.ClientId,
                        ClientSecret = request.ClientSecret,
                        Code = request.Code,
                        RedirectUri = $"{Request.Scheme}://{Request.Host}/auth",
                        ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader,
                    });

                if (token.IsError)
                {
                    throw new Exception($"Failed to exchange code for a token. Status Code: {token.HttpStatusCode}, Error: {token.Error}");
                }
                
                JArray locations = await GetLocations(token.AccessToken, request.ClientId);
                List<User> users = GetUsers(locations);

                return new OkObjectResult(new UsersResponse
                {
                    Users = users,
                    Locations = locations,
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calling Honeywell API");
                
                ObjectResult error = new ObjectResult(new UsersResponse {ErrorMessage = ex.Message})
                {
                    StatusCode = 500,
                };
                return error;
            }
        }

        [HttpGet]
        [Route("config")]
        public IActionResult GetConfig()
        {
            return new OkObjectResult(new
            {
                ApiBaseUri = apiBaseUri.AbsoluteUri,
            });
        }

        private List<User> GetUsers(JArray locations)
        {
            IDictionary<string, User> users = new Dictionary<string, User>();

            foreach (JToken location in locations)
            {
                JArray usrs = location["users"] as JArray;

                if (usrs == null)
                {
                    continue;
                }
                
                foreach (JToken user in usrs)
                {
                    string userId = user["userID"].Value<string>();

                    if (!users.ContainsKey(userId))
                    {
                        users.Add(userId, new User
                        {
                            UserId = userId,
                            Username = user["username"].Value<string>(),
                            FirstName = user["firstname"].Value<string>(),
                            LastName = user["lastname"].Value<string>(),
                        });
                    }
                }
            }
            
            return users.Values.ToList();
        }
        
        private async Task<JArray> GetLocations(string accessToken, string clientId)
        {
            Url url = new Url(apiBaseUri).AppendPathSegment("v2/locations").SetQueryParam("apikey", clientId);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url.ToString());
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            return JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());
        }
    }
}