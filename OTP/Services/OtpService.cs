namespace OTP.Services
{
    public class OtpService
    {
        public string GenerateOtp(int length = 6)
        {
            var rng = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => rng.Next(0, 10).ToString()[0])
                .ToArray());
        }
    }

}
