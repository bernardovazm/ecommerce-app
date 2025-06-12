using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecommerce.Infrastructure.Payments.Pagarme.Configuration;
using Ecommerce.Infrastructure.Payments.Pagarme.Models;

namespace Ecommerce.Infrastructure.Payments.Pagarme;

public interface IPagarmeClient
{
    Task<PagarmeTransactionResponse> CreateTransactionAsync(PagarmeTransactionRequest request, CancellationToken cancellationToken = default);
    Task<PagarmeTransactionResponse> GetTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<PagarmeTransactionResponse> CaptureTransactionAsync(string transactionId, int? amount = null, CancellationToken cancellationToken = default);
    Task<PagarmeTransactionResponse> RefundTransactionAsync(string transactionId, int? amount = null, CancellationToken cancellationToken = default);
}

public class PagarmeClient : IPagarmeClient
{
    private readonly HttpClient _httpClient;
    private readonly PagarmeSettings _settings;
    private readonly ILogger<PagarmeClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PagarmeClient(
        HttpClient httpClient,
        IOptions<PagarmeSettings> settings,
        ILogger<PagarmeClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        
        // Add API key to headers
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.ApiKey}:"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
        
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Ecommerce-App/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<PagarmeTransactionResponse> CreateTransactionAsync(PagarmeTransactionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Pagar.me transaction for amount: {Amount}", request.Amount);
            
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogDebug("Pagar.me request payload: {Payload}", json);
            
            var response = await _httpClient.PostAsync("/transactions", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogDebug("Pagar.me response: {Response}", responseContent);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Pagar.me API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<PagarmeErrorResponse>(responseContent, _jsonOptions);
                    var errorMessage = string.Join("; ", errorResponse?.Errors?.Select(e => e.Message) ?? new[] { "Unknown error" });
                    throw new PagarmeException($"Pagar.me API error: {errorMessage}", response.StatusCode);
                }
                catch (JsonException)
                {
                    throw new PagarmeException($"Pagar.me API error: {response.StatusCode} - {responseContent}", response.StatusCode);
                }
            }
            
            var transaction = JsonSerializer.Deserialize<PagarmeTransactionResponse>(responseContent, _jsonOptions);
            if (transaction == null)
            {
                throw new PagarmeException("Failed to deserialize Pagar.me response");
            }
            
            _logger.LogInformation("Pagar.me transaction created successfully: {TransactionId} - Status: {Status}", 
                transaction.Id, transaction.Status);
            
            return transaction;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout calling Pagar.me API");
            throw new PagarmeException("Timeout calling Pagar.me API", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Pagar.me API");
            throw new PagarmeException("Network error calling Pagar.me API", ex);
        }
        catch (PagarmeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Pagar.me API");
            throw new PagarmeException("Unexpected error calling Pagar.me API", ex);
        }
    }

    public async Task<PagarmeTransactionResponse> GetTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting Pagar.me transaction: {TransactionId}", transactionId);
            
            var response = await _httpClient.GetAsync($"/transactions/{transactionId}", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Pagar.me API error getting transaction: {StatusCode} - {Content}", response.StatusCode, responseContent);
                throw new PagarmeException($"Failed to get transaction: {response.StatusCode}", response.StatusCode);
            }
            
            var transaction = JsonSerializer.Deserialize<PagarmeTransactionResponse>(responseContent, _jsonOptions);
            if (transaction == null)
            {
                throw new PagarmeException("Failed to deserialize Pagar.me transaction response");
            }
            
            return transaction;
        }
        catch (PagarmeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Pagar.me transaction {TransactionId}", transactionId);
            throw new PagarmeException($"Error getting transaction {transactionId}", ex);
        }
    }

    public async Task<PagarmeTransactionResponse> CaptureTransactionAsync(string transactionId, int? amount = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Capturing Pagar.me transaction: {TransactionId}, Amount: {Amount}", transactionId, amount);
            
            var request = new { amount };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/transactions/{transactionId}/capture", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Pagar.me API error capturing transaction: {StatusCode} - {Content}", response.StatusCode, responseContent);
                throw new PagarmeException($"Failed to capture transaction: {response.StatusCode}", response.StatusCode);
            }
            
            var transaction = JsonSerializer.Deserialize<PagarmeTransactionResponse>(responseContent, _jsonOptions);
            if (transaction == null)
            {
                throw new PagarmeException("Failed to deserialize Pagar.me capture response");
            }
            
            return transaction;
        }
        catch (PagarmeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing Pagar.me transaction {TransactionId}", transactionId);
            throw new PagarmeException($"Error capturing transaction {transactionId}", ex);
        }
    }

    public async Task<PagarmeTransactionResponse> RefundTransactionAsync(string transactionId, int? amount = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Refunding Pagar.me transaction: {TransactionId}, Amount: {Amount}", transactionId, amount);
            
            var request = new { amount };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/transactions/{transactionId}/refund", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Pagar.me API error refunding transaction: {StatusCode} - {Content}", response.StatusCode, responseContent);
                throw new PagarmeException($"Failed to refund transaction: {response.StatusCode}", response.StatusCode);
            }
            
            var transaction = JsonSerializer.Deserialize<PagarmeTransactionResponse>(responseContent, _jsonOptions);
            if (transaction == null)
            {
                throw new PagarmeException("Failed to deserialize Pagar.me refund response");
            }
            
            return transaction;
        }
        catch (PagarmeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding Pagar.me transaction {TransactionId}", transactionId);
            throw new PagarmeException($"Error refunding transaction {transactionId}", ex);
        }
    }
}

public class PagarmeException : Exception
{
    public System.Net.HttpStatusCode? StatusCode { get; }

    public PagarmeException(string message) : base(message)
    {
    }

    public PagarmeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public PagarmeException(string message, System.Net.HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
