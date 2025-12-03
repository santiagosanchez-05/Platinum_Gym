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

namespace Platinum_Gym_System.Controllers
{
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
            var appDBContext = _context.Sales
                .Include(s => s.Payment)
                .Include(s => s.SaleDetails);
            return View(await appDBContext.ToListAsync());
        }

        // GET: Sales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.Payment)
                .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.SaleId == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // GET: Sales/Create
        public IActionResult Create()
        {
            ViewData["PaymentId"] = new SelectList(_context.Payments, "PaymentId", "PaymentMethod");
            ViewBag.Products = _context.Products
                .Select(p => new { productId = p.ProductId, productName = p.ProductName, price = p.Price })
                .ToList();

            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");

            return View(new SaleCreateVM
            {
                Sale = new Sale(),
                Items = new List<SaleDetail>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleCreateVM vm)
        {
            // Validación de Items
            if (vm.Items == null || !vm.Items.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto.");
            }

            double totalVenta = 0;

            if (ModelState.IsValid)
            {
                foreach (var item in vm.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null) continue;

                    if (product.StockQuantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"No hay stock suficiente para {product.ProductName}");

                        ViewData["PaymentId"] = new SelectList(_context.Payments, "PaymentId", "PaymentMethod");
                        ViewBag.Products = _context.Products
                            .Select(p => new { productId = p.ProductId, productName = p.ProductName, price = p.Price })
                            .ToList();

                        return View(vm);
                    }

                    item.Subtotal = product.Price * item.Quantity;
                    totalVenta += item.Subtotal;
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["PaymentId"] = new SelectList(_context.Payments, "PaymentId", "PaymentMethod");
                ViewBag.Products = _context.Products
                    .Select(p => new { productId = p.ProductId, productName = p.ProductName, price = p.Price })
                    .ToList();

                return View(vm);
            }

            // Registrar venta
            vm.Sale.Total = totalVenta;
            _context.Sales.Add(vm.Sale);
            await _context.SaveChangesAsync();

            foreach (var item in vm.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                item.SaleId = vm.Sale.SaleId;
                product.StockQuantity -= item.Quantity;

                _context.SaleDetails.Add(item);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Venta registrada exitosamente.";

            return RedirectToAction(nameof(Index));
        }




        // GET: Sales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales.FindAsync(id);
            if (sale == null) return NotFound();

            ViewData["PaymentId"] = new SelectList(_context.Payments, "PaymentId", "PaymentMethod", sale.PaymentId);
            return View(sale);
        }

        // POST: Sales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SaleId,SaleDate,Total,PaymentId")] Sale sale)
        {
            if (id != sale.SaleId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SaleExists(sale.SaleId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentId"] = new SelectList(_context.Payments, "PaymentId", "PaymentMethod", sale.PaymentId);
            return View(sale);
        }

        // GET: Sales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.Payment)
                .FirstOrDefaultAsync(m => m.SaleId == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null) _context.Sales.Remove(sale);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.SaleId == id);
        }
    }
}
