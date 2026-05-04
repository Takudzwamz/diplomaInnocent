namespace Core.DTOs;

public class PaymentResponseDto
{
    public required string CartId { get; set; }
    public required string PaymentReference { get; set; }
    public required string AuthorizationUrl { get; set; }
}