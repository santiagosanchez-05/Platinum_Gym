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
        public async Task<IActionResult> Login2(User model)
        {
            var userBD = await _context.Users.FirstOrDefaultAsync(u => u.CI == model.CI);

            if (userBD == null)
            {
                ViewBag.Error = "User not found";
                return View(model);
            }

            if (userBD.Role == 3)
            {
                ViewBag.Error = "This login is only for gym staff.";
                return View();
            }

            if (model.Password == null)
            {
                ViewBag.Error = "You must enter a password.";
                return View(model);
            }

            // Hashed password
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(
                userBD,             
                userBD.Password,    
                model.Password     
            );

            if (result != PasswordVerificationResult.Success)
            {
                ViewBag.Error = "Incorrect password.";
                return View(model);
            }

            // Build claims
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

            // Force password change rule
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
                ViewBag.Error = "Passwords do not match.";
                ViewBag.CI = ci;
                return View();
            }

            if (newPassword.StartsWith("156"))
            {
                ViewBag.Error = "Invalid password.";
                ViewBag.CI = ci;
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.CI == ci);

            if (user == null)
            {
                ViewBag.Error = "User not found.";
                ViewBag.CI = ci;
                return View();
            }

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, newPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Password changed successfully.";
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

            // ❌ CLIENT NOT FOUND
            if (userClient == null)
            {
                ViewBag.Error = "Client not registered.";
                model.CI = "";
                ModelState.Clear();
                return View(model);
            }

            if (userClient.Role != 3)
            {
                return RedirectToAction(nameof(Login2));
            }


            // Obtener última suscripción 
            var lastSub = await _context.Subscriptions
                .Where(s => s.UserId == userClient.UserId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (lastSub == null || lastSub.State == 0)
            {
                TempData["ExpiredClient"] = userClient.BillingName;
                TempData["CI"] = userClient.CI;
                TempData["ExpireDate"] = lastSub?.EndDate.ToString("dd/MM/yyyy") ?? "No record";

                model.CI = "";
                ModelState.Clear();
                return View(model);
            }

            // Si existe pero está expirada
            if (lastSub.EndDate < DateTime.Now)
            {
                lastSub.State = 0;
                _context.Subscriptions.Update(lastSub);
                await _context.SaveChangesAsync();

                TempData["ExpiredClient"] = userClient.BillingName;
                TempData["CI"] = userClient.CI;
                TempData["ExpireDate"] = lastSub.EndDate.ToString("dd/MM/yyyy");

                model.CI = "";
                ModelState.Clear();
                return View(model);
            }

            TempData["WelcomeClient"] = userClient.BillingName;
            TempData["CI"] = userClient.CI;
            TempData["ExpireDate"] = lastSub.EndDate.ToString("dd/MM/yyyy");

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
            var user2 = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (user1 != null)
            {
                ModelState.AddModelError(string.Empty, "There cannot be two users with the same ID number (CI).");
            }
            if (user2 != null)
            {
                ModelState.AddModelError(string.Empty, "There cannot be two users with the same email address.");
            }
            if (user.Email == null)
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
            }

            if (ModelState.IsValid)
            {
                var random = new Random();
                string Password = "156" + user.BillingName.Substring(0, 2) + user.CI + random.Next(100, 999);
                Console.Write(Password);

                var hasher = new PasswordHasher<User>();
                string hash = hasher.HashPassword(user, Password);
                user.Password = hash;

                _context.Add(user);
                await _context.SaveChangesAsync();

                await EmailService.SendAsync(
                    user.Email,
                    "Your system access",
                    $"Your generated password is: {Password}"
                );

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

            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.CI == user.CI);
            var user2 = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (user1 != null)
            {
                ModelState.AddModelError(string.Empty, "There cannot be two users with the same ID number (CI).");
            }
            if (user2 != null)
            {
                ModelState.AddModelError(string.Empty, "There cannot be two users with the same email address.");
            }
            if (user.Email == null)
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.Password = userBD.Password;

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    if (correoCambiado)
                    {
                        await EmailService.SendAsync(
                            user.Email,
                            "Email change successful",
                            $"Hi {user.BillingName}, your email address was successfully updated in the system."
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
