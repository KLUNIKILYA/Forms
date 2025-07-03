using System.Diagnostics;
using System.Security.Claims;
using DataBase.Repositories;
using Forms.Models;
using Forms.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Forms.Controllers
{
    public class AuthController : Controller
    {
        IUserRepository _userRepository;

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = _userRepository.FindByCredentials(viewModel.Email, viewModel.Password);

            if (user is null)
            {
                ModelState.AddModelError("Password", "Неправильный логин или пароль");
                return View(viewModel);
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("Password", "Пользователь заблокирован");
                return View(viewModel);
            }

            var claims = new List<Claim>()
            {
                new Claim(AuthServices.CLAIM_TYPE_ID, user.Id.ToString()),
                new Claim(AuthServices.CLAIM_TYPE_NAME, user.Name),
                new Claim(AuthServices.CLAIM_TYPE_ROLE, ((int)user.Role).ToString()),
                new Claim(ClaimTypes.AuthenticationMethod, AuthServices.AUTH_TYPE_KEY),
            };

            var identity = new ClaimsIdentity(claims, AuthServices.AUTH_TYPE_KEY);

            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync(principal).Wait();

            return RedirectToAction("Index", "Templates");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterUserViewModel viewModel)
        {
            _userRepository.Register(viewModel.Name, viewModel.Password, viewModel.Email);
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();

            return RedirectToAction("Index", "Templates");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
