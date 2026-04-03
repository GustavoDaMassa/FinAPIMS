using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinanceApi.Webhook.Application.Dtos;
using FinanceApi.Webhook.Application.Interfaces;

namespace FinanceApi.Webhook.Application.Services;

public class PluggyClient(HttpClient httpClient, IConfiguration configuration) : IPluggyClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<PluggyTransaction>> FetchTransactionsAsync(string transactionsLink)
    {
        var apiKey = await GetApiKeyAsync();
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

        var response = await httpClient.GetAsync(transactionsLink);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PluggyTransactionsResponse>(content, JsonOptions);
        return result?.Results ?? [];
    }

    private async Task<string> GetApiKeyAsync()
    {
        var clientId = configuration["Pluggy:ClientId"]!;
        var clientSecret = configuration["Pluggy:ClientSecret"]!;

        var body = JsonSerializer.Serialize(new { clientId, clientSecret });
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<PluggyAuthResponse>(content, JsonOptions);
        return auth?.ApiKey ?? throw new InvalidOperationException("Failed to obtain Pluggy API key.");
    }
}
