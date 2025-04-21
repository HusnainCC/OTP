using Microsoft.EntityFrameworkCore;
using OTP.Services;
using OTP;
using Microsoft.AspNetCore.Mvc;
using OTP.Models;
using OTP.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("OtpDb")
           .EnableSensitiveDataLogging() // ?? shows full entity info in errors
);
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/signup-initiate", async (AppDbContext db, IEmailService emailService, [FromBody] SignupRequest request) =>
{
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (existingUser != null) return Results.BadRequest("Email already exists.");

    var otp = new Random().Next(100000, 999999).ToString();
    var otpEntry = new OtpRequest
    {
        Email = request.Email,
        Otp = otp,
        Expiry = DateTime.UtcNow.AddMinutes(5)
    };

    db.OtpRequests.Add(otpEntry);
    await db.SaveChangesAsync();

    await emailService.SendOtpAsync(request.Email, otp);
    return Results.Ok("OTP sent to email.");
});

app.MapPost("/signup-verify", async (AppDbContext db, [FromBody] OtpVerification request) =>
{
    var otpEntry = await db.OtpRequests.FirstOrDefaultAsync(o => o.Email == request.Email && o.Otp == request.Otp);
    if (otpEntry == null || otpEntry.Expiry < DateTime.UtcNow)
        return Results.BadRequest("Invalid or expired OTP.");

    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

    db.Users.Add(new User
    {
        Email = request.Email,
        PasswordHash = hashedPassword
    });

    db.OtpRequests.Remove(otpEntry);
    await db.SaveChangesAsync();

    return Results.Ok("Signup successful.");
});

app.Run();

// Request Models
public record SignupRequest(string Email, string Password);
public record OtpVerification(string Email, string Otp, string Password);

