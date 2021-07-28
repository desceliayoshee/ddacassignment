using System;
using ddacassignment.Areas.Identity.Data;
using ddacassignment.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(ddacassignment.Areas.Identity.IdentityHostingStartup))]
namespace ddacassignment.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<ddacassignmentContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("ddacassignmentContextConnection")));

                services.AddDefaultIdentity<ddacassignmentUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ddacassignmentContext>();
            });
        }
    }
}