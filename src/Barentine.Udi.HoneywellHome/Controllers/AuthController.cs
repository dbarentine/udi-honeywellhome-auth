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
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Barentine.Udi.HoneywellHome.Models;
    using IdentityModel.Client;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<AuthController> logger;

        public AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
        {
            this.httpClient = httpClientFactory.CreateClient(); 
            this.logger = logger;
        }
        
        [HttpPost]
        public async Task<IActionResult> GetUser([FromBody] CodeExchangeRequest request)
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
                TokenResponse token = await httpClient.RequestAuthorizationCodeTokenAsync(
                    new AuthorizationCodeTokenRequest
                    {
                        Address = "https://connectedhome-sandbox.apigee.net/oauth2/token",
                        ClientId = request.ClientId,
                        ClientSecret = request.ClientSecret,
                        Code = request.Code,
                        RedirectUri = $"{Request.Scheme}://{Request.Host}/auth",
                        ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader,
                    });

                JArray locations = await GetLocations(token.AccessToken, request.ClientId);
                return new OkObjectResult(locations);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calling Honeywell API");
                return new StatusCodeResult(500);
            }
        }

        private async Task<JArray> GetLocations(string accessToken, string clientId)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://connectedhome-sandbox.apigee.net/v2/locations?apikey={clientId}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            return JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());
        }
    }
}