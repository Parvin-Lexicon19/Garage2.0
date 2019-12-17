using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage2._0.Models;

namespace Garage2._0.Controllers
{
    public class ParkedVehiclesController : Controller
    {
        private readonly Garage2_0Context _context;
        static private ParkingSlots parkingSlot;
        public ParkedVehiclesController(Garage2_0Context context)
        {
            _context = context;
            parkingSlot = new ParkingSlots();
        }

        public async Task<IActionResult> Receipt(int? id)
        {           
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle.FirstOrDefaultAsync(m => m.Id == id);

            if (parkedVehicle == null)
            {
                return NotFound();
            }

            var model = new Receipt();
            model.RegNo = parkedVehicle.RegNo;
            model.Type = parkedVehicle.Type;
            model.CheckInTime = parkedVehicle.CheckInTime;
            model.CheckOutTime = DateTime.Now;
            var totaltime = model.CheckOutTime - model.CheckInTime;
            var min = (totaltime.Minutes > 0) ? 1 : 0;


            if (totaltime.Days == 0)
            {
                model.Totalparkingtime = totaltime.Hours + " Hrs " + totaltime.Minutes + " Mins";
                model.Totalprice = ((totaltime.Hours + min) * 5) + "Kr";
            }
            else
            {
                model.Totalparkingtime = totaltime.Days + "Days" + " " +totaltime.Hours + "hrs" + " " + totaltime.Minutes + "mins";
                model.Totalprice = (totaltime.Days * 100) + ((totaltime.Hours + min) * 5) + "Kr";
            }

            parkedVehicle.CheckOutTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return View(model);
        }
        public async Task<IActionResult> GetStatistic()
        {
         
            int totalWheels = 0;
            double totalMin = 0;
            DateTime nowTime = DateTime.Now;
            int nowTimeResult =  (nowTime.Day*100) + nowTime.Hour + nowTime.Minute ;
            double timePrice = 0;
            double totalParkTimePrice =0;
            
            var model = new Statistic();

            // Get car count eheels
            var parkedVehicles = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime)).Select (m =>(m.NoOfWheels));
            foreach (var wheel in parkedVehicles)
            {
                totalWheels += wheel;
            }

            // Get car count
            var carCount =  _context.ParkedVehicle.Where(p => p.Type == VehicleType.Car).Select(u => u.Type);
            model.TotalCar = carCount.Count();

            // Get Boat count
            var boatCount = _context.ParkedVehicle.Where(p => p.Type == VehicleType.Boat).Select(u => u.Type);
            model.TotalBoat = boatCount.Count();

            // Get Bus count
            var busCount = _context.ParkedVehicle.Where(p => p.Type == VehicleType.Bus).Select(u => u.Type);
            model.TotalBus = busCount.Count();

            // Get Airplane count
            var airplaneCount = _context.ParkedVehicle.Where(p => p.Type == VehicleType.Airplane).Select(u => u.Type);
            model.TotalAirplane = airplaneCount.Count();
            var totTimeChIn = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime)).Select(m => (m.CheckInTime));
            foreach (var chTime in totTimeChIn)
            {
                int chTimeResult = (chTime.Day*100) + chTime.Hour + chTime.Minute ;
                totalMin = nowTimeResult - chTimeResult;
                timePrice = totalMin * 0.083;
                totalParkTimePrice += timePrice;
            }
            

            model.TotalParkedVehiclePrice = (int)totalParkTimePrice;
            model.TotalVehicles= parkedVehicles.Count();
            // Get Motorcycle count
            var motorBikeCount = _context.ParkedVehicle.Where(p => p.Type == VehicleType.Motorcycle).Select(u => u.Type);
            model.TotalMotorbike = motorBikeCount.Count();

            model.TotalWheels = totalWheels;
            
            await _context.SaveChangesAsync();

            return View(model);
        }

        // GET: ParkedVehicles
        public async Task<IActionResult> Index()
        {

            ViewBag.NoOfFreePlaces = GetFreeSlotsNo();
            ViewBag.NoOfFreePlacesForMotorcycle = GetFreeSlotsNoForMotorcycle();
            
            //it shows only parked vehicles and not the checked out ones
            var parkedVehicles = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime));
            return View(await parkedVehicles.ToListAsync());
        }

        // GET: ParkedVehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var parkedVehicle = await _context.ParkedVehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }

            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ParkedVehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,RegNo,Color,Brand,Model,NoOfWheels,CheckInTime,CheckOutTime")] ParkedVehicle parkedVehicle)
        {
            if (ModelState.IsValid)
            {
                

                // Populate the current date and Time for checkIN field. Check whether the RegNo of the Vehicle already been parked in garage.

                parkedVehicle.CheckInTime = DateTime.Now;
                parkedVehicle.CheckOutTime = default(DateTime);
                var findRegNo =  _context.ParkedVehicle.Where(p => p.RegNo == parkedVehicle.RegNo && p.CheckOutTime == default(DateTime)).ToList();
                if (findRegNo.Count == 0)
                {
                    _context.Add(parkedVehicle);
                }
                else
                {
                    ModelState.AddModelError("RegNo", "Vehicle with same Regno is parked");

                    return View();

                }
                await _context.SaveChangesAsync();
                Park(parkedVehicle.Id, (int)parkedVehicle.Type);
                return RedirectToAction(nameof(Index));
            }
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }
            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,RegNo,Color,Brand,Model,NoOfWheels")] ParkedVehicle parkedVehicle)
        // Do not update the CheckInTime field for any changes in other field values.
        {
            if (id != parkedVehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newPostData = await _context.ParkedVehicle.FindAsync(id);

                    if (newPostData == null)
                    {
                        return NotFound();
                    }

                    newPostData.Type = parkedVehicle.Type;
                    newPostData.RegNo = parkedVehicle.RegNo;
                    newPostData.Color = parkedVehicle.Color;
                    newPostData.Brand = parkedVehicle.Brand;
                    newPostData.Model = parkedVehicle.Model;
                    newPostData.NoOfWheels = parkedVehicle.NoOfWheels;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkedVehicleExists(parkedVehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }

            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check out will just populate Checkout Time but will not delete any record from DB.

            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            parkedVehicle.CheckOutTime = DateTime.Now;
           // _context.ParkedVehicle.Remove(parkedVehicle);
            
            return RedirectToAction(nameof(Index));
        }

        private bool ParkedVehicleExists(int id)
        {
            return _context.ParkedVehicle.Any(e => e.Id == id);
        }
        /*Searches based on RegNo. and Type of vehicle*/
        public async Task<IActionResult> Filter(string regNo, int? type)
        {
            var model = await _context.ParkedVehicle.Where(m => m.CheckOutTime.Equals(default(DateTime))).ToListAsync();
            model = string.IsNullOrWhiteSpace(regNo) ?
                model :
                model.Where(m => m.RegNo.Contains(regNo)).ToList();

            model = type == null ?
                model :
                model.Where(m => m.Type == (VehicleType)type).ToList();

            return View(nameof(Index), model);
        }
        /*Sortes based on Type, Regno, Color or CheckInTime of vehicle*/
        [HttpGet]
        public async Task<IActionResult> Sort(string columnName)
        {         
            var model = await _context.ParkedVehicle.Where(m => m.CheckOutTime.Equals(default(DateTime))).ToListAsync();
            switch (columnName)
            {
                case "Type":
                    model = model.OrderByDescending(m => m.Type).ToList();
                    break;
                case "RegNo":
                    model = model.OrderByDescending(m => m.RegNo).ToList();
                    break;
                case "Color":
                    model = model.OrderByDescending(m => m.Color).ToList();
                    break;
                case "CheckInTime":
                    model = model.OrderByDescending(m => m.CheckInTime).ToList();
                    break;
                default:
                    break;
            }
            return View(nameof(Index), model);
        }
        //Assigns a slot to the vehicle when it checks in
        //    1/3 slot for Motorcycle
        //    1 slot for Car
        //    2 for Bus
        //    3 for Boat and Airplane
        // The Slots Array has 100 indexes, every index refers to another array of 4 indexes which has its first index as VehicleType 
        // and 2nd, 3rd and 4th as possible Motorcycle IDs or all the 3 indexes has the same ID if Vehicle Type is Car, Boat, ...
        private void Park(int id, int Type)
        {
            //For Motorcycle, checks to see if there is free slot next to already parked Motrorcycle in order to save parking space 
            //for other bigger in size vehicles
            if (Type.Equals((int)VehicleType.Motorcycle))
            {
                for (int i = 0; i < ParkingSlots.Slots.GetLength(0); i++) // Number of slots which is 100 here
                {
                    if (ParkingSlots.Slots[i, 0].Equals(0)) 
                    {
                        ParkingSlots.Slots[i, 0] = Type;
                        ParkingSlots.Slots[i, 1] = id;
                        return;
                    }
                    //if the first index contains Motocycle Type (Enum), it may have palce for another motorcycle
                    else if (ParkingSlots.Slots[i, 0].Equals((int)VehicleType.Motorcycle)) 
                    {
                        //parks the new Motorcycle in 2nd or 3rd place, Indexes 3 or 4 of the slot (which has already a Motorcycle in)
                        for (int j = 1; j < ParkingSlots.Slots.GetLength(1); j++)
                        {
                            if (ParkingSlots.Slots[i, j].Equals(0))
                            {
                                ParkingSlots.Slots[i, j] = id;
                                return;
                            }
                        }
                    }
                }                    
            }                
            else //For other Vehicle Types
            for (int i = 0; i < ParkingSlots.Slots.GetLength(0); i++)
                if (ParkingSlots.Slots[i, 0].Equals(0))
                {
                        switch (Type)
                        {
                            case (int)VehicleType.Car:
                                ParkingSlots.Slots[i, 0] = Type;
                                for (int j = 1; j < ParkingSlots.Slots.GetLength(1); j++)
                                    ParkingSlots.Slots[i, j] = id;
                                return;
                            case (int)VehicleType.Bus:
                                if(ParkingSlots.Slots[i + 1, 0].Equals(0))
                                {
                                    ParkingSlots.Slots[i, 0] = Type;
                                    ParkingSlots.Slots[i + 1, 0] = Type;
                                    for (int j = 1; j < ParkingSlots.Slots.GetLength(1); j++)
                                    {
                                        ParkingSlots.Slots[i, j] = id;
                                        ParkingSlots.Slots[i + 1, j] = id;
                                    }
                                    return;
                                }
                                break; 
                            case (int)VehicleType.Boat:
                            case (int)VehicleType.Airplane:
                                if (ParkingSlots.Slots[i + 1, 0].Equals(0) && ParkingSlots.Slots[i + 2, 0].Equals(0))
                                {
                                    ParkingSlots.Slots[i, 0] = Type;
                                    ParkingSlots.Slots[i + 1, 0] = Type;
                                    ParkingSlots.Slots[i + 2, 0] = Type;
                                    for (int j = 1; j < ParkingSlots.Slots.GetLength(1); j++)
                                    {
                                        ParkingSlots.Slots[i, j] = id;
                                        ParkingSlots.Slots[i + 1, j] = id;
                                        ParkingSlots.Slots[i + 2, j] = id;
                                    }
                                    return;
                                }
                                break;
                            default:
                                break;
                        }                    
                }
        }
        private int GetFreeSlotsNo()
        {
            int freeSlotsNo = 0;
            for (int i = 0; i < ParkingSlots.Slots.GetLength(0); i++)
            {
                if (ParkingSlots.Slots[i, 0].Equals(0))
                    freeSlotsNo++;
            }
            return freeSlotsNo;
        }
        private int GetFreeSlotsNoForMotorcycle()
        {
            int freeSlotsNoM = 0;
            for (int i = 0; i < ParkingSlots.Slots.GetLength(0); i++)
            {
                if (ParkingSlots.Slots[i, 0].Equals(0))
                    freeSlotsNoM += 3;
                else
                    if (ParkingSlots.Slots[i, 0].Equals((int)VehicleType.Motorcycle))
                {
                    for (int j = 1; j < ParkingSlots.Slots.GetLength(1); j++)
                    {
                        if (ParkingSlots.Slots[i, j].Equals(0))
                            freeSlotsNoM++;
                    }
                }
            }
            return freeSlotsNoM;
        } 
    }
}
