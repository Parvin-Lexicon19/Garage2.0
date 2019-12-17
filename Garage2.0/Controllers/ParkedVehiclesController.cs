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

        public ParkedVehiclesController(Garage2_0Context context)
        {
            _context = context;
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
            int nowTimeResult = nowTime.Year * 10000 + nowTime.Month * 100 + nowTime.Day + nowTime.Hour + nowTime.Minute ;
            double timePrice = 0;
            double totalParkTime =0;
            
            var model = new Statistic();

            var parkedVehicles = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime)).Select (m =>(m.NoOfWheels));
            foreach (var wheel in parkedVehicles)
            {
                totalWheels += wheel;
            }
            var totTimeChIn = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime)).Select(m => (m.CheckInTime));
            foreach (var chTime in totTimeChIn)
            {
                int chTimeResult = chTime.Year * 10000 + chTime.Month * 100 + chTime.Day + chTime.Hour + chTime.Minute ;
                totalMin = nowTimeResult - chTimeResult;
                timePrice = totalMin * 0.083;
                totalParkTime += timePrice;
            }
            

            model.TotalParkedVehiclePrice = (int)totalParkTime;
            model.TotalVehicles= parkedVehicles.Count();
            model.TotalWheels = totalWheels;
            
            await _context.SaveChangesAsync();

            return View(model);
        }
       
            // GET: ParkedVehicles
            public async Task<IActionResult> Index()
        {
            //it shows only parked vehicles and not the checked out ones
            var parkedVehicles = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime));
            return View(await parkedVehicles.ToListAsync());
           // return View(await _context.ParkedVehicle.ToListAsync());
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
    }
}
