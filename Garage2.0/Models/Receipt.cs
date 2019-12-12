using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Garage2._0.Models
{
    public class Receipt
    {
        public VehicleType Type { get; set; }

        [Display(Name = "Reg No: ")]
        public string RegNo { get; set; }

        [Display(Name = "CheckIn Time: ")]
        public DateTime CheckInTime { get; set; }

        [Display(Name = "CheckOut Time: ")]
        public DateTime CheckOutTime { get; set; }
        [Display(Name = "Total Parking Time: ")]
        public string Totalparkingtime { get; set; }
        [Display(Name = "Total Price: ")]
        public string  Totalprice { get; set; }
    }
}
