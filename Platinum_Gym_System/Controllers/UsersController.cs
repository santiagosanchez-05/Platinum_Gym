using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platinum_Gym_System.Data;
using Platinum_Gym_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //var user =await _context.Users.FirstOrDefaultAsync(w=>w.Email==model.Email && w.Password==model.Password);
            //if (user != null) {
            //    return RedirectToAction("Index", "Home");
            //}
            //ViewBag.Error = "Incorrect Credentials";
            //return View(model);
            var userLogin = from u in _context.Users
                            where u.CI == model.CI && u.Password== model.Password
                            select new
                            {
                                u,
                                RoleName = u.Role

                            };
            if (userLogin.Any())
            {
                byte rol = userLogin.First().RoleName;
                string CI = userLogin.First().u.CI;


              
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "User"),
                    new Claim("CI", CI),                       // claim personalizado
                    new Claim(ClaimTypes.Role, rol.ToString()) // debe ser string
                };

                    var claimsIdentity = new ClaimsIdentity(claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index","Home");
             }
            
            ViewBag.Error = "User not found";
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> login(User model)
        {
            //var user =await _context.Users.FirstOrDefaultAsync(w=>w.Email==model.Email && w.Password==model.Password);
            //if (user != null) {
            //    return RedirectToAction("Index", "Home");
            //}
            //ViewBag.Error = "Incorrect Credentials";
            //return View(model);
            var userLogin = from u in _context.Users
                            where u.CI == model.CI
                            select new
                            {
                                u,
                                RoleName = u.Role
                            };
            if (userLogin.Any())
            {
                byte rol = userLogin.First().RoleName;
                string CI = userLogin.First().u.CI;

               
                if (rol != 3)
                {
                    return RedirectToAction(nameof(Login2));
                }
                else
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "User"),
                    new Claim("CI", CI),                       // claim personalizado
                    new Claim(ClaimTypes.Role, rol.ToString()) // debe ser string
                };

                    var claimsIdentity = new ClaimsIdentity(claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                    ViewBag.UserName=userLogin.First().u.BillingName;
                    return View();
                }
            }
            ViewBag.Error = "User not found";
            return View();
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
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
        public async Task<IActionResult> Create([Bind("UserId,BillingName,CI,Password,Role,State,Photo")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> Edit(int id, [Bind("UserId,BillingName,CI,Password,Role,State,Photo")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
    }
}
