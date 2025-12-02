using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platinum_Gym_System.Data;
using Platinum_Gym_System.Models;

namespace Platinum_Gym_System.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDBContext _context;

        public ProductsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            // Opciones predefinidas para categorías
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Suplementos", Text = "Suplementos" },
                new SelectListItem { Value = "Ropa", Text = "Ropa" },
                new SelectListItem { Value = "Accesorios", Text = "Accesorios" },
                new SelectListItem { Value = "Equipamiento", Text = "Equipamiento" },
                new SelectListItem { Value = "Otros", Text = "Otros" }
            };

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Category,Price,StockQuantity,ProductImage")] Product product)
        {
            // Validar si ya existe un producto con el mismo nombre
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower());

            if (existingProduct != null)
            {
                ModelState.AddModelError("ProductName", "Ya existe un producto con este nombre.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            // Recargar las categorías si hay error
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Suplementos", Text = "Suplementos" },
                new SelectListItem { Value = "Ropa", Text = "Ropa" },
                new SelectListItem { Value = "Accesorios", Text = "Accesorios" },
                new SelectListItem { Value = "Equipamiento", Text = "Equipamiento" },
                new SelectListItem { Value = "Otros", Text = "Otros" }
            };

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Suplementos", Text = "Suplementos", Selected = product.Category == "Suplementos" },
                new SelectListItem { Value = "Ropa", Text = "Ropa", Selected = product.Category == "Ropa" },
                new SelectListItem { Value = "Accesorios", Text = "Accesorios", Selected = product.Category == "Accesorios" },
                new SelectListItem { Value = "Equipamiento", Text = "Equipamiento", Selected = product.Category == "Equipamiento" },
                new SelectListItem { Value = "Otros", Text = "Otros", Selected = product.Category == "Otros" }
            };

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Category,Price,StockQuantity,ProductImage")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            // Validar nombre único excluyendo el producto actual
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower() && p.ProductId != id);

            if (existingProduct != null)
            {
                ModelState.AddModelError("ProductName", "Ya existe un producto con este nombre.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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

            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Suplementos", Text = "Suplementos", Selected = product.Category == "Suplementos" },
                new SelectListItem { Value = "Ropa", Text = "Ropa", Selected = product.Category == "Ropa" },
                new SelectListItem { Value = "Accesorios", Text = "Accesorios", Selected = product.Category == "Accesorios" },
                new SelectListItem { Value = "Equipamiento", Text = "Equipamiento", Selected = product.Category == "Equipamiento" },
                new SelectListItem { Value = "Otros", Text = "Otros", Selected = product.Category == "Otros" }
            };

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto eliminado exitosamente.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        // Método adicional para obtener productos por categoría
        public async Task<IActionResult> ByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _context.Products
                .Where(p => p.Category == category)
                .ToListAsync();

            ViewBag.Category = category;
            return View(products);
        }

        // Método para verificar stock bajo
        public async Task<IActionResult> LowStock(int threshold = 10)
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            ViewBag.Threshold = threshold;
            return View(lowStockProducts);
        }
    }
}