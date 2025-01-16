using ChatApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApplication.Controllers
{
    public class AuthController : Controller
    {
        private UserContext _userContext;

        public AuthController(UserContext userContext)
        {
            _userContext = userContext;
        }

        [Route("/")]
        public IActionResult Index()
        {
            return View(_userContext.Users.ToList());
        }

        [Route("Register")]
        public IActionResult Registration()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SecurePage");
            }
            return View();
        }

        [Route("Register")]
        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isDuplicate = _userContext.Users.Any(u => u.Username == model.Username || u.Email == model.Email);
                if (isDuplicate)
                {
                    if (_userContext.Users.Any(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError("Username", "This username is already taken.");
                    }

                    if (_userContext.Users.Any(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                    }

                    return View(model); // Return the form with errors
                }
                User user = new User();
                user.Username = model.Username;
                user.Email = model.Email;
                user.PasswordHash = model.PasswordHash;
                _userContext.Users.Add(user);
                _userContext.SaveChanges();

                ModelState.Clear();
                ViewBag.Message = $"{user.Username} registered successfully. Please login";

                return View();
            }
            return View(model);
        }

        [Route("LogIn")]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SecurePage");
            }
            return View();
        }

        [Route("LogIn")]
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userContext.Users.Where(x => x.Username == model.Username && x.PasswordHash == model.PasswordHash).FirstOrDefault();
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("SecurePage");
                }
                else
                {
                    ModelState.AddModelError("", "Username or Password is not correct");
                }
                return View();
            }
            return View(model);
        }

        [Route("LogOut")]
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [Route("SecurePage")]
        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            var usernames = _userContext.Users.ToList();
            usernames.Remove(_userContext.Users.Where(x => x.Username == HttpContext.User.Identity.Name).FirstOrDefault());
            return View(usernames);
        }

        [Route("Random-Chat")]
        [Authorize]
        public IActionResult RandomChat()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }
    }
}
