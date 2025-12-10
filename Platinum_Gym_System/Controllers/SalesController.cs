using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platinum_Gym_System.Data;
using Platinum_Gym_System.Models;
using Platinum_Gym_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace Platinum_Gym_System.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly AppDBContext _context;

        public SalesController(AppDBContext context)
        {
            _context = context;
        }

        // GET: Sales
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sales.ToListAsync());
        }

        // GET: Sales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.SaleDetails).ThenInclude(d => d.Product)
                .Include(s => s.SalePayments)
                .Include(s => s.Cancellations) // 🔥 si agregaste historial de anulaciones
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null) return NotFound();

            return View(sale);
        }


        // GET: Sales/Create
        // GET: Sales/Create
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products
                .Select(p => new { productId = p.ProductId, productName = p.ProductName, price = p.Price })
                .ToList();

            return View(new SaleCreateVM
            {
                Sale = new Sale(),
                Items = new List<SaleDetail>(),
                Payments = new List<SalePayment>()
            });
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleCreateVM vm)
        {
            // 🔹 Always reload products for the view (in case validation fails)
            ViewBag.Products = _context.Products
                .Select(p => new { productId = p.ProductId, productName = p.ProductName, price = p.Price })
                .ToList();

            // 🔹 Remove empty items
            vm.Items = (vm.Items ?? new List<SaleDetail>())
                .Where(x => x.ProductId > 0 && x.Quantity > 0)
                .ToList();

            if (!vm.Items.Any())
            {
                ModelState.AddModelError("", "You must add at least one product.");
                return View(vm);
            }

            double totalSale = 0;

            // 🔹 Validate stock and calculate total
            foreach (var item in vm.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null || product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Insufficient stock for product: {product?.ProductName}");
                    return View(vm);
                }

                item.Subtotal = product.Price * item.Quantity;
                totalSale += item.Subtotal;
            }

            // 🔹 If no sale date is provided, use current time
            if (vm.Sale.SaleDate == default)
                vm.Sale.SaleDate = DateTime.Now;

            vm.Sale.Total = totalSale;

            // 1️⃣ Register Sale
            _context.Sales.Add(vm.Sale);
            await _context.SaveChangesAsync(); // generates SaleId

            // 2️⃣ Register Details
            foreach (var item in vm.Items)
            {
                item.SaleId = vm.Sale.SaleId;

                var product = await _context.Products.FindAsync(item.ProductId);
                product.StockQuantity -= item.Quantity;

                _context.SaleDetails.Add(item);
            }

            await _context.SaveChangesAsync();

            // 3️⃣ Register Payments (if any)
            vm.Payments = (vm.Payments ?? new List<SalePayment>())
                .Where(p => p.Amount > 0).ToList();

            if (vm.Payments.Any())
            {
                foreach (var pay in vm.Payments)
                {
                    pay.SaleId = vm.Sale.SaleId;

                    if (pay.PaymentDate == default)
                        pay.PaymentDate = DateTime.Now;

                    _context.SalePayments.Add(pay);
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Sale registered successfully.";

            return RedirectToAction(nameof(Index));
        }







        // GET: Sales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }
            return View(sale);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SaleId,SaleDate,Total")] Sale sale)
        {
            if (id != sale.SaleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SaleExists(sale.SaleId))
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
            return View(sale);
        }

        // GET: Sales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales
                .FirstOrDefaultAsync(m => m.SaleId == id);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.SaleId == id);
        }


        public async Task<IActionResult> Cancel(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null) return NotFound();

            return View(new SaleCancelVM { SaleId = id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(SaleCancelVM vm)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .FirstOrDefaultAsync(s => s.SaleId == vm.SaleId);

            if (sale == null) return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            // Only cancel if not already cancelled
            if (!sale.IsCancelled)
            {
                sale.IsCancelled = true;
                _context.Update(sale);

                // Get current logged user
                var userCI = User.FindFirst("CI")?.Value;
                var cancelledBy = userCI;

                // Register reason + user
                var cancellation = new SaleCancellation
                {
                    SaleId = sale.SaleId,
                    Reason = vm.Reason,
                    CancellationDate = DateTime.Now,
                    CancelledBy = cancelledBy
                };

                _context.SaleCancellations.Add(cancellation);

                // Restore stock
                foreach (var detail in sale.SaleDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                        product.StockQuantity += detail.Quantity;
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Sale cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
