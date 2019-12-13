using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Garage2._0.Models
{
    public class ParkedVehicle
    {
        public int Id { get; set; }
        public VehicleType Type { get; set; }
        [Required]
        [MaxLength(20)]
        
        public string RegNo { get; set; }
        public string Color { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int NoOfWheels { get; set; }
        [DisplayFormat(DataFormatString = "{0:f}")]
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
    }

    public enum VehicleType
    {
        Car,
        Boat,
        Bus,
        Motorcycle,
        Airplane
    }
}
