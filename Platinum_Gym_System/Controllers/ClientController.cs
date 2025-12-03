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
    public class ClientController : Controller
    {
        private readonly AppDBContext _context;

        public ClientController(AppDBContext context)
        {
            _context = context;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            // 1️⃣ ACTUALIZAR SUBSCRIPCIONES VENCIDAS
            var expired = await _context.Subscriptions
                .Where(s => s.State == 1 && s.EndDate < DateTime.Now)
                .ToListAsync();

            foreach (var sub in expired)
                sub.State = 0;

            await _context.SaveChangesAsync();

            // 2️⃣ TRAER CLIENTES CON ÚLTIMA SUSCRIPCIÓN
            var clients = await _context.Users
                .Where(u => u.Role == 3 && u.State == 1)
                .Select(u => new ClientIndexVM
                {
                    User = u,
                    LastSubscription = _context.Subscriptions
                        .Where(s => s.UserId == u.UserId)
                        .OrderByDescending(s => s.EndDate)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return View(clients);
        }


        // GET: Client/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            var vm = new ClientCreateVM
            {
                Plans = _context.Plans.Where(p => p.State == 1).ToList()
            };

            return View(vm);
        }


        // POST: Client/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Plans = _context.Plans.Where(p => p.State == 1).ToList();
                return View(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1️⃣ CREAR USER
                var user = new User
                {
                    BillingName = vm.BillingName,
                    CI = vm.CI,
                    Role = 3,
                    State = 1
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 2️⃣ OBTENER PLAN
                var plan = await _context.Plans.FindAsync(vm.PlanId);

                // 3️⃣ CREAR SUBSCRIPCIÓN
                var subscription = new Subscription
                {
                    UserId = user.UserId,
                    PlanId = plan.PlanId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(plan.DurationMonths),
                    State = 1
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                // 4️⃣ CREAR PAGO
                var payment = new Payment
                {
                    SubscriptionId = subscription.SubscriptionId,
                    Amount = plan.Price,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = vm.PaymentMethod,
                    State = 1
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Client/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,BillingName,CI")] User formUser)
        {
            if (id != formUser.UserId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(formUser);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // ✅ SOLO se actualiza lo permitido
            user.BillingName = formUser.BillingName;
            user.CI = formUser.CI;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.UserId))
                    return NotFound();
                throw;
            }
        }


        // GET: Client/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.State = 0;
                var activeSubs = await _context.Subscriptions
                   .Where(s => s.UserId == id && s.State == 1)
                   .ToListAsync();

                foreach (var sub in activeSubs)
                    sub.State = 0;
                _context.Users.Update(user);

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
        public async Task<IActionResult> Renew(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new SubscriptionRenewVM
            {
                UserId = user.UserId,
                Plans = await _context.Plans.Where(p => p.State == 1).ToListAsync()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(SubscriptionRenewVM vm)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Obtener última suscripción del cliente
                var lastSub = await _context.Subscriptions
                    .Where(s => s.UserId == vm.UserId)
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                // 2️⃣ Obtener el nuevo plan
                var plan = await _context.Plans.FindAsync(vm.PlanId);

                // 3️⃣ Determinar fecha de inicio REALISTA
                DateTime startDate;

                if (lastSub != null && lastSub.EndDate > DateTime.Now)
                    startDate = lastSub.EndDate;      // sigue activo → encadena
                else
                    startDate = DateTime.Now;         // estaba vencido → hoy

                var endDate = startDate.AddMonths(plan.DurationMonths);

                // 4️⃣ Crear nueva suscripción
                var newSub = new Subscription
                {
                    UserId = vm.UserId,
                    PlanId = plan.PlanId,
                    StartDate = startDate,
                    EndDate = endDate,
                    State = 1
                };

                _context.Subscriptions.Add(newSub);
                await _context.SaveChangesAsync();

                // 5️⃣ Registrar nuevo pago
                var payment = new Payment
                {
                    SubscriptionId = newSub.SubscriptionId,
                    Amount = plan.Price,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = vm.PaymentMethod,
                    State = 1
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
