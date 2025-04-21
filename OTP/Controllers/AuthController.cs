using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using OTP.Models;
using OTP.Services;
using System;

namespace OTP.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, OtpService otpService, EmailService emailService)
        {
            _context = context;
            _otpService = otpService;
            _emailService = emailService;
        }

        // Step 1: Initiate Signup (Generate OTP)
        [HttpPost("signup-initiate")]
        public async Task<IActionResult> SignupInitiate([FromBody] SignupRequest request)
        {
            // Check if the email already exists
            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest("Email already registered.");

            // Generate OTP
            var otp = _otpService.GenerateOtp();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var otpEntry = new OtpRequest
            {
                Email = request.Email,
                Otp = otp, // ✅ use variable 'otp', not 'Otp'
                TempPasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            await _context.OtpRequests.AddAsync(otpEntry);
            await _context.SaveChangesAsync();

            // Send OTP to the user's email
            await _emailService.SendOtpAsync(request.Email, otp);

            return Ok("OTP sent to your email.");
        }

        // Step 2: Verify OTP and Complete Signup
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            // Retrieve the OTP entry for the given email
            var otpRecord = await _context.OtpRequests
                .FirstOrDefaultAsync(o => o.Email == request.Email && o.Otp == request.Otp);

            // Validate OTP and its expiry
            if (otpRecord == null || otpRecord.Expiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            // Create new user based on the OTP's password hash
            var user = new User
            {
                Email = otpRecord.Email,
                PasswordHash = otpRecord.TempPasswordHash
            };

            // Register user and delete OTP record
            _context.Users.Add(user);
            _context.OtpRequests.Remove(otpRecord);
            await _context.SaveChangesAsync();

            return Ok("Signup successful.");
        }
    }

}
