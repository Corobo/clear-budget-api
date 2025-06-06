﻿using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Shared.Auth.Clients.Impl
{
    public class AuthTokenClient : IAuthTokenClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AuthTokenClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var clientId = _config["Auth:ClientId"];
            var clientSecret = _config["Auth:ClientSecret"];
            var tokenUrl = _config["Auth:TokenUrl"];


            var body = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
        });

            var response = await _httpClient.PostAsync(tokenUrl, body);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("access_token").GetString();
        }
    }
}
