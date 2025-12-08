using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platinum_Gym_System.Data;
using Platinum_Gym_System.Models;
using Platinum_Gym_System.ViewModels;

namespace Platinum_Gym_System.Controllers
{
    [Authorize(Roles = "1")]
    public class ReportsController : Controller
    {
        private readonly AppDBContext _context;

        public ReportsController(AppDBContext context)
        {
            _context = context;
        }

        // INDEX GENERAL DE REPORTES
        public IActionResult Index()
        {
            return View();
        }


        //public async Task<IActionResult> ActiveStudents()
        //{
        //    var activos = await _context.Subscriptions
        //        .Where(s => s.State == 1 && s.EndDate >= DateTime.Now)
        //        .Select(s => s.UserId)
        //        .Distinct()
        //        .CountAsync();

        //    ViewBag.TotalActivos = activos;
        //    return View();
        //}
        public async Task<IActionResult> PlansActive()
        {
            var now = DateTime.Now;

            var query = await _context.Plans
                .Where(p => p.State == 1) // opcional: si quieres incluir inactivos quita este Where
                .Select(p => new PlanActiveVM
                {
                    PlanId = p.PlanId,
                    PlanName = p.Name,
                    ActiveCount = _context.Subscriptions
                        .Where(s => s.PlanId == p.PlanId && s.State == 1 && s.EndDate >= now)
                        .Select(s => s.UserId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();

            return View(query);
        }


        // 2. Snacks vendidos (últimos 30 días)
        public async Task<IActionResult> SnacksSold()
        {
            var desde = DateTime.Now.AddDays(-30);

            var snacksVendidos = await _context.SaleDetails
                .Include(sd => sd.Product)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.SaleDate >= desde && !sd.Sale.IsCancelled)
                .GroupBy(sd => sd.Product.ProductName)
                .Select(g => new SnackReportVM
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Subtotal) // subtotal corresponde al dinero por líneas
                })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync();

            return View(snacksVendidos);
        }



        // 3. Ingresos diarios (últimos 7 días)
        public async Task<IActionResult> DailyIncome()
        {
            var desde = DateTime.Now.AddDays(-7);

            var ingresos = await _context.Sales
                .Where(s => s.SaleDate >= desde && !s.IsCancelled)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new IncomeReportVM
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            return View(ingresos);
        }

    }
}
