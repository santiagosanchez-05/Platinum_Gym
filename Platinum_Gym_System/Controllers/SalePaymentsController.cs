using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platinum_Gym_System.Data;
using Platinum_Gym_System.Models;

namespace Platinum_Gym_System.Controllers
{
    public class SalePaymentsController : Controller
    {
        private readonly AppDBContext _context;

        public SalePaymentsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: SalePayments
        public async Task<IActionResult> Index()
        {
            var payments = _context.SalePayments
                .Include(p => p.Sale);

            return View(await payments.ToListAsync());
        }

        // GET: SalePayments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.SalePayments
                .Include(p => p.Sale)
                .FirstOrDefaultAsync(p => p.SalePaymentId == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // GET: SalePayments/Create
        public IActionResult Create(int? saleId)
        {
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", saleId);

            return View(new SalePayment
            {
                SaleId = saleId ?? 0,
                PaymentDate = System.DateTime.Now
            });
        }

        // POST: SalePayments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalePayment salePayment)
        {
            if (ModelState.IsValid)
            {
                if (salePayment.PaymentDate == default)
                    salePayment.PaymentDate = System.DateTime.Now;

                _context.SalePayments.Add(salePayment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", salePayment.SaleId);
            return View(salePayment);
        }

        // GET: SalePayments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.SalePayments.FindAsync(id);
            if (payment == null)
                return NotFound();

            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", payment.SaleId);
            return View(payment);
        }

        // POST: SalePayments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalePayment salePayment)
        {
            if (id != salePayment.SalePaymentId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salePayment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalePaymentExists(salePayment.SalePaymentId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", salePayment.SaleId);
            return View(salePayment);
        }

        // GET: SalePayments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.SalePayments
                .Include(p => p.Sale)
                .FirstOrDefaultAsync(m => m.SalePaymentId == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // POST: SalePayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.SalePayments.FindAsync(id);
            if (payment != null)
                _context.SalePayments.Remove(payment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SalePaymentExists(int id)
        {
            return _context.SalePayments.Any(e => e.SalePaymentId == id);
        }
    }
}
