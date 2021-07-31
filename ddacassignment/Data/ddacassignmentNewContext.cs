using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ddacassignment.Models;

namespace ddacassignment.Data
{
    public class ddacassignmentNewContext : DbContext
    {
        public ddacassignmentNewContext (DbContextOptions<ddacassignmentNewContext> options)
            : base(options)
        {
        }

        public DbSet<ddacassignment.Models.Service> Service { get; set; }
    }
}
