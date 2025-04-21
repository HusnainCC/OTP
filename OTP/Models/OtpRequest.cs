namespace OTP.Models
{
    public class OtpRequest
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public DateTime Expiry { get; set; }
        public string? TempPasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
