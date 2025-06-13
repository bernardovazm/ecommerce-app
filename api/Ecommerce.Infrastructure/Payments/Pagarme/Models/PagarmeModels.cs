namespace Ecommerce.Infrastructure.Payments.Pagarme.Models;

public class PagarmeTransactionRequest
{
    public int Amount { get; set; }
    public string PaymentMethod { get; set; } = "credit_card";
    public PagarmeCardData? CardData { get; set; }
    public PagarmeCustomer Customer { get; set; } = new();
    public PagarmeBilling? Billing { get; set; }
    public List<PagarmeItem> Items { get; set; } = new();
    public string SoftDescriptor { get; set; } = "ECOMMERCE";
    public bool Capture { get; set; } = true;
    public bool Async { get; set; } = false;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PagarmeCardData
{
    public string Number { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
}

public class PagarmeCustomer
{
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = "individual";
    public string Country { get; set; } = "br";
    public List<PagarmeDocument> Documents { get; set; } = new();
    public List<PagarmePhone> Phones { get; set; } = new();
}

public class PagarmeDocument
{
    public string Type { get; set; } = "cpf";
    public string Number { get; set; } = string.Empty;
}

public class PagarmePhone
{
    public string Country { get; set; } = "55";
    public string Area { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Type { get; set; } = "mobile";
}

public class PagarmeBilling
{
    public string Name { get; set; } = string.Empty;
    public PagarmeAddress Address { get; set; } = new();
}

public class PagarmeAddress
{
    public string Country { get; set; } = "br";
    public string State { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class PagarmeItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public bool Tangible { get; set; } = true;
}
