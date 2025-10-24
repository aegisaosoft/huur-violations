namespace HuurViolations.Api.Models;

public class ViolationDto
{
    public string CitationNumber { get; set; } = string.Empty;
    public string Agency { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentStatus { get; set; } = string.Empty;
    public string FineType { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}


