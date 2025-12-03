using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platinum_Gym_System.Data;
using Platinum_Gym_System.Models;
using Platinum_Gym_System.Services;
using Platinum_Gym_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
namespace Platinum_Gym_System.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDBContext _context;

        public UsersController(AppDBContext context)
        {
            _context = context;
        }
        public IActionResult Login2()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> login2(User model)
        {
            // Obtener usuario solo por CI (no compares contraseña aquí)
            var userBD = await _context.Users.FirstOrDefaultAsync(u => u.CI == model.CI);

            // Usuario no encontrado
            if (userBD == null)
            {
                ViewBag.Error = "User not found";
                return View(model);
            }

            // Verificar contraseña hasheada
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(
                userBD,             // entidad encontrada
                userBD.Password,    // hash guardado en la BD
                model.Password      // contraseña ingresada
            );

            if (result != PasswordVerificationResult.Success)
            {
                ViewBag.Error = "Incorrect password";
                return View(model);
            }
            
            // Construcción de claims (manteniendo tus variables)
            byte rol = userBD.Role;
            string CI = userBD.CI;

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "User"),
        new Claim("CI", CI),
        new Claim(ClaimTypes.Role, rol.ToString())
    };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
            if (model.Password.StartsWith("156"))
            {
                return RedirectToAction("ChangePassword", new { ci = userBD.CI });
            }
            return RedirectToAction("Index", "Home");
        }
        public IActionResult ChangePassword(string ci)
        {
            if (string.IsNullOrEmpty(ci))
            {
                return RedirectToAction("Login");
            }

            ViewBag.CI = ci;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string ci, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(ci))
                return RedirectToAction("Login");

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                ViewBag.CI = ci;
                return View();
            }
            if (newPassword.StartsWith("156"))
            {
                ViewBag.Error = "Contrasena no valida";
                ViewBag.CI = ci;
                return View();  
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.CI == ci);

            if (user == null)
            {
                ViewBag.Error = "Usuario no encontrado";
                ViewBag.CI = ci;
                return View();
            }

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, newPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "La contraseña se cambió correctamente.";
            ViewBag.CI = ci;

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(User model)
        {
            var userClient = await _context.Users
                .FirstOrDefaultAsync(u => u.CI == model.CI && u.State == 1);

            // ❌ CLIENTE NO REGISTRADO
            if (userClient == null)
            {
                ViewBag.Error = "Cliente no registrado";
                model.CI = "";
                ModelState.Clear();
                return View(model);
            }

            // ❌ NO ES CLIENTE → VA A LOGIN2
            if (userClient.Role != 3)
            {
                return RedirectToAction(nameof(Login2));
            }

            // ✅ OBTENER ÚLTIMA SUSCRIPCIÓN
            var lastSub = await _context.Subscriptions
                .Where(s => s.UserId == userClient.UserId && s.State == 1)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            // ✅ ACTUALIZAR ESTADO SI YA VENCIÓ (MISMA LÓGICA QUE TU INDEX)
            if (lastSub != null && lastSub.EndDate < DateTime.Now)
            {
                lastSub.State = 0;
                _context.Subscriptions.Update(lastSub);
                await _context.SaveChangesAsync();

                // ⚠️ POPUP DE RENOVACIÓN
                TempData["WelcomeClient"] = $"⚠️ {userClient.BillingName}";
                TempData["CI"] = userClient.CI;
                TempData["ExpireDate"] = "RENUEVA TU SUSCRIPCIÓN";

                model.CI = "";
                ModelState.Clear();
                return View(model);
            }

            // ✅ CLIENTE ACTIVO → BIENVENIDO NORMAL
            TempData["WelcomeClient"] = userClient.BillingName;
            TempData["CI"] = userClient.CI;
            TempData["ExpireDate"] = lastSub?.EndDate.ToString("dd/MM/yyyy");

            model.CI = "";
            ModelState.Clear();

            return View(model);
        }


        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.Where(u=>u.State==1&&u.Role!=3).ToListAsync());
        }

        // GET: Users/Details/5
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

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,BillingName,CI,Password,Role,State,Photo,Email")] User user)
        {
            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.CI == user.CI);
            if (user1 != null)
            {
                ModelState.AddModelError(string.Empty, "No puede haber dos usuarios con el mismo CI");
            }
            if (user.Email == null)
            {
                ModelState.AddModelError(string.Empty, "El correo es obligatorio");

            }
            
            if (ModelState.IsValid)
            {
                var random = new Random();
                string Password ="156"+user.BillingName.Substring(0, 2)+user.CI+random.Next(100, 999);
                Console.Write(Password);
                var hasher = new PasswordHasher<User>();
                string hash = hasher.HashPassword(user, Password);
                user.Password= hash;
                _context.Add(user);
                await _context.SaveChangesAsync();
                await EmailService.SendAsync(user.Email, "Tu acceso al sistema",
                $"Tu contraseña generada es: {Password}");

                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
        

        // GET: Users/Edit/5
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

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,BillingName,CI,Role,State,Photo,Email")] User user)
        {
            if (id != user.UserId)
                return NotFound();

            var userBD = await _context.Users.AsNoTracking()
                             .FirstOrDefaultAsync(u => u.UserId == id);

            if (userBD == null)
                return NotFound();

            bool correoCambiado = userBD.Email != user.Email;

            if (ModelState.IsValid)
            {
                try
                {
                    // Mantener contraseña original
                    user.Password = userBD.Password;

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    // ✅ Enviar correo SOLO si fue cambiado
                    if (correoCambiado)
                    {
                        await EmailService.SendAsync(
                            user.Email,
                            "Cambio de correo exitoso",
                            $"Hola {user.BillingName}, tu correo fue actualizado correctamente en el sistema."
                        );
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }


        // GET: Users/Delete/5
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

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
