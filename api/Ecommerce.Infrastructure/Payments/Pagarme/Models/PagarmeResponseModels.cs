namespace Ecommerce.Infrastructure.Payments.Pagarme.Models;

public class PagarmeTransactionResponse
{
    public string Object { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RefuseReason { get; set; } = string.Empty;
    public string StatusReason { get; set; } = string.Empty;
    public string AcquirerResponseCode { get; set; } = string.Empty;
    public string AcquirerName { get; set; } = string.Empty;
    public string AcquirerId { get; set; } = string.Empty;
    public string AuthorizationCode { get; set; } = string.Empty;
    public string SoftDescriptor { get; set; } = string.Empty;
    public string Tid { get; set; } = string.Empty;
    public string Nsu { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public int Amount { get; set; }
    public int AuthorizedAmount { get; set; }
    public int PaidAmount { get; set; }
    public int RefundedAmount { get; set; }
    public int Installments { get; set; }
    public int Id { get; set; }
    public int Cost { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public string CardLastDigits { get; set; } = string.Empty;
    public string CardFirstDigits { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string PostbackUrl { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string CaptureMethod { get; set; } = string.Empty;
    public string AntifraudScore { get; set; } = string.Empty;
    public string BoletoUrl { get; set; } = string.Empty;
    public string BoletoBarcode { get; set; } = string.Empty;
    public string BoletoExpirationDate { get; set; } = string.Empty;
    public string Referer { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public PagarmePhone Phone { get; set; } = new();
    public PagarmeAddress Address { get; set; } = new();
    public PagarmeCustomer Customer { get; set; } = new();
    public PagarmeBilling Billing { get; set; } = new();
    public PagarmeShipping Shipping { get; set; } = new();
    public List<PagarmeItem> Items { get; set; } = new();
    public PagarmeCard Card { get; set; } = new();
    public List<PagarmeSplitRule> SplitRules { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public List<PagarmeAntifraudAnalysis> AntifraudAnalyses { get; set; } = new();
    public string Reference { get; set; } = string.Empty;
}

public class PagarmeCard
{
    public string Object { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public string FirstDigits { get; set; } = string.Empty;
    public string LastDigits { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
    public bool Valid { get; set; }
    public DateTime ExpirationDate { get; set; }
}

public class PagarmeShipping
{
    public string Name { get; set; } = string.Empty;
    public int Fee { get; set; }
    public int DeliveryDate { get; set; }
    public bool Expedited { get; set; }
    public PagarmeAddress Address { get; set; } = new();
}

public class PagarmeSplitRule
{
    public string Recipient { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public int Amount { get; set; }
    public bool Liable { get; set; }
    public bool ChargeProcessingFee { get; set; }
}

public class PagarmeAntifraudAnalysis
{
    public string Name { get; set; } = string.Empty;
    public string Score { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}

public class PagarmeErrorResponse
{
    public List<PagarmeError> Errors { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}

public class PagarmeError
{
    public string Type { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
