namespace OTP.Models
{
    public class SignupRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class OtpVerificationRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }


}
