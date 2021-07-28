using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ddacassignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ddacassignment.Data
{
    public class ddacassignmentContext : IdentityDbContext<ddacassignmentUser>
    {
        public ddacassignmentContext(DbContextOptions<ddacassignmentContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
