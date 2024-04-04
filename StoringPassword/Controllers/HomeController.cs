using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using StoringPassword.Models;
using StoringPassword.Repository;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StoringPassword.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;

        public HomeController(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> GetMessages()
        {
            try
            {
                var messages = await _repository.GetMessagesAsync();

                var formattedMessages = messages.Select(message => new
                {
                    User = message.User.FirstName + " " + message.User.LastName,
                    Text = message.Text,
                    MessageDate = message.MessageDate.ToString("yyyy-MM-dd HH:mm:ss")
                });

                return Json(formattedMessages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, error = "The message cannot be empty or contain only spaces" });
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                if (HttpContext.Session.GetString("LastName") != null &&
                    HttpContext.Session.GetString("FirstName") != null)
                {
                    string firstName = HttpContext.Session.GetString("FirstName");
                    string lastName = HttpContext.Session.GetString("LastName");

                    var user = _repository.GetUserForMessageAsync(firstName, lastName).Result;
                    if (user != null)
                    {
                        var newMessage = new Message
                        {
                            UserId = user.Id,
                            Text = message.Trim(),
                            MessageDate = DateTime.Now
                        };

                        _repository.AddMessageAsync(newMessage).Wait();

                        return Json(new { success = true, message = "Сообщение успешно добавлено!" });
                    }
                    return Json(new { success = false, error = "User not found" });
                }
                else
                {
                    return Json(new { success = false, error = "User not logged in" });
                }
            }
            return Json(new { success = false, error = "Message is empty" });
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Json(new { redirectTo = Url.Action("Index", "Home") });
        }
    }
}