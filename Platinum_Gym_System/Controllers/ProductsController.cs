using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platinum_Gym_System.Data;
using Platinum_Gym_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Platinum_Gym_System.Controllers
{
    [Authorize]
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
            // Predefined category options
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Supplements", Text = "Supplements" },
                new SelectListItem { Value = "Clothing", Text = "Clothing" },
                new SelectListItem { Value = "Accessories", Text = "Accessories" },
                new SelectListItem { Value = "Equipment", Text = "Equipment" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            return View();
        }


        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Category,Price,StockQuantity,ProductImage")] Product product)
        {
            // Validate if a product with the same name already exists
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower());

            if (existingProduct != null)
            {
                ModelState.AddModelError("ProductName", "A product with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Reload categories if validation fails
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Supplements", Text = "Supplements" },
                new SelectListItem { Value = "Clothing", Text = "Clothing" },
                new SelectListItem { Value = "Accessories", Text = "Accessories" },
                new SelectListItem { Value = "Equipment", Text = "Equipment" },
                new SelectListItem { Value = "Other", Text = "Other" }
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
                new SelectListItem { Value = "Supplements", Text = "Supplements", Selected = product.Category == "Supplements" },
                new SelectListItem { Value = "Clothing", Text = "Clothing", Selected = product.Category == "Clothing" },
                new SelectListItem { Value = "Accessories", Text = "Accessories", Selected = product.Category == "Accessories" },
                new SelectListItem { Value = "Equipment", Text = "Equipment", Selected = product.Category == "Equipment" },
                new SelectListItem { Value = "Other", Text = "Other", Selected = product.Category == "Other" }
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

            // Validate unique name excluding the current product
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName.ToLower() == product.ProductName.ToLower() && p.ProductId != id);

            if (existingProduct != null)
            {
                ModelState.AddModelError("ProductName", "A product with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product updated successfully.";
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
                new SelectListItem { Value = "Supplements", Text = "Supplements", Selected = product.Category == "Supplements" },
                new SelectListItem { Value = "Clothing", Text = "Clothing", Selected = product.Category == "Clothing" },
                new SelectListItem { Value = "Accessories", Text = "Accessories", Selected = product.Category == "Accessories" },
                new SelectListItem { Value = "Equipment", Text = "Equipment", Selected = product.Category == "Equipment" },
                new SelectListItem { Value = "Other", Text = "Other", Selected = product.Category == "Other" }
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
                TempData["SuccessMessage"] = "Product deleted successfully.";
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