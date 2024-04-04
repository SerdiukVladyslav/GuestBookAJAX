using Microsoft.AspNetCore.Mvc;
using StoringPassword.Models;
using StoringPassword.Repository;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StoringPassword.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository _repository;

        public AccountController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel logon)
        {
            if (ModelState.IsValid)
            {
                var user = _repository.GetUserByLoginAsync(logon.Login).Result;
                if (user == null)
                {
                    return Json(new { success = false, error = "Wrong login or password!" });
                }

                string salt = user.Salt;
 
                byte[] password = Encoding.Unicode.GetBytes(salt + logon.Password);

                byte[] byteHash = SHA256.HashData(password);

                StringBuilder hash = new StringBuilder(byteHash.Length);
                for (int i = 0; i < byteHash.Length; i++)
                    hash.Append(string.Format("{0:X2}", byteHash[i]));

                if (user.Password != hash.ToString())
                {
                    return Json(new { success = false, error = "Wrong login or password!" });
                }

                HttpContext.Session.SetString("FirstName", user.FirstName);
                HttpContext.Session.SetString("LastName", user.LastName);

                return Json(new { success = true, message = "Пользователь успешно зашел!" });
            }

            return Json(new { success = false, error = "Invalid model state" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterModel reg)
        {
            if (reg.Password != reg.PasswordConfirm)
            {
                return Json(new { success = false, error = "Passwords do not match" });
            }

            if (ModelState.IsValid)
            {
                User user = new User();
                user.FirstName = reg.FirstName;
                user.LastName = reg.LastName;
                user.Login = reg.Login;

                byte[] saltbuf = new byte[16];

                RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
                randomNumberGenerator.GetBytes(saltbuf);

                StringBuilder sb = new StringBuilder(16);
                for (int i = 0; i < 16; i++)
                    sb.Append(string.Format("{0:X2}", saltbuf[i]));
                string salt = sb.ToString();

                byte[] password = Encoding.Unicode.GetBytes(salt + reg.Password);
                byte[] byteHash = SHA256.HashData(password);

                StringBuilder hash = new StringBuilder(byteHash.Length);
                for (int i = 0; i < byteHash.Length; i++)
                    hash.Append(string.Format("{0:X2}", byteHash[i]));

                user.Password = hash.ToString();
                user.Salt = salt;

                _repository.AddUserAsync(user);

                return Json(new { success = true, message = "Пользователь успешно добавлен!" });
            }

            return Json(new { success = false, error = "Invalid model state" });
        }
    }
}
