// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeExchangeRequest.cs" company="One Identity Inc.">
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

namespace Barentine.Udi.HoneywellHome.Models
{
    using Newtonsoft.Json;

    public class CodeExchangeRequest
    {
        public string Code { get; set; }
        
        [JsonProperty("client_id")]
        public string ClientId { get; set; }
        
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }
}