using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ddacassignment.Models
{
    public class Service
    {
        
        public int ID { set; get; }

        [Display(Name = "Service Name")]
        [Required(ErrorMessage = "Must fill the Service Name !")]
        public String ServiceName { set; get; }

        [Display(Name = "Schedule")]
        [Required(ErrorMessage = "Must fill the Schedule!")]
        public DateTime ServiceSchedule { set; get; }

        [Display(Name = "Service Price")]
        [Range(1,500, ErrorMessage = "Must between value 1 to 500")]
        [DataType(DataType.Currency)]
        public double ServicePrice { set; get; }
    }
}
