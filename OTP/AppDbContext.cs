using Microsoft.EntityFrameworkCore;
using OTP.Models;
using System.Collections.Generic;

namespace OTP
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<OtpRequest> OtpRequests { get; set; }
    }


}
