using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ddacassignment.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the ddacassignmentUser class
    public class ddacassignmentUser : IdentityUser
    {
        [PersonalData]
        public string User_Full_Name { get; set; }

        [PersonalData]
        public string Address { get; set; }

        [PersonalData]
        public string userrole { get; set; }
    }
}
