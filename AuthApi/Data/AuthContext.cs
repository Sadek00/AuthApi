using AuthApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace AuthApi.Data
{
    public class AuthContext : IdentityDbContext<AppUser>
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options)
        {
            
        }
    }
}
