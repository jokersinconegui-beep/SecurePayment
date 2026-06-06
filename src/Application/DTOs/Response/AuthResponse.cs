namespace Application.DTOs.Response;

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
