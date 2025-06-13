namespace Ecommerce.Infrastructure.Payments.Pagarme.Configuration;

public class PagarmeSettings
{
    public const string SectionName = "Pagarme";

    public string ApiKey { get; set; } = string.Empty;
    public string EncryptionKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.pagar.me/1";
    public bool IsSandbox { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;

    public TestCards TestCards { get; set; } = new();
}

public class TestCards
{
    public TestCard Approved { get; set; } = new()
    {
        Number = "4111111111111111",
        HolderName = "João Silva",
        ExpirationDate = "1225",
        Cvv = "123"
    };

    public TestCard Declined { get; set; } = new()
    {
        Number = "4000000000000002",
        HolderName = "Maria Santos",
        ExpirationDate = "1225",
        Cvv = "123"
    };

    public TestCard ProcessingError { get; set; } = new()
    {
        Number = "4000000000000119",
        HolderName = "Pedro Costa",
        ExpirationDate = "1225",
        Cvv = "123"
    };
}

public class TestCard
{
    public string Number { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
}
