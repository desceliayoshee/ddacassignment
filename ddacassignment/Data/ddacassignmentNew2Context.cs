using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ddacassignment.Models;

namespace ddacassignment.Data
{
    public class ddacassignmentNew2Context : DbContext
    {
        public ddacassignmentNew2Context (DbContextOptions<ddacassignmentNew2Context> options)
            : base(options)
        {
        }

        public DbSet<ddacassignment.Models.Service> Service { get; set; }
    }
}
