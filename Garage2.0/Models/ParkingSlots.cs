using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Garage2._0.Models
{
    public class ParkingSlots
    {
        //static int capacity = (int) SlotsNo.GarageCapacity;
        //static int motorcycleSlots = (int) SlotsNo.Motorcycle;
        //public static int[,] Slots = new int[capacity, (1 / motorcycleSlots) + 1];
        public static int[,] Slots = new int[100, 4];
    }
    public enum SlotsNo
    {
        GarageCapacity = 100,
        Car = 1,
        Boat = 3,
        Bus = 2,
        Motorcycle = 1/3,
        Airplane = 3
    }
}
