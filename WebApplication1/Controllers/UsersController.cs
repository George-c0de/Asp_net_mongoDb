using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[action]")]
public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService) =>
        _usersService = usersService;

    [HttpGet]
    public async Task<List<Users>> Get() =>
        await _usersService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Users>> Get(string id)
    {
        var user = await _usersService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return user;
    }
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var users = await _usersService.GetAsync();
            Users user = null;
            foreach (var el in users)
            {
                if (el.Email == model.Email)
                {
                    user = el;
                }
            }
            if (user == null)
            {
                return View("Message");
            }

            var code = await _usersService.GetToken(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Users", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

            //Здесь отправка по email, так как ветка без предыдущего пула
              EmailMessageSender emailService = new EmailMessageSender();
            await emailService.SendEmailAsync(model.Email, "Reset Password", $"Для сброса пароля пройдите по ссылке: <a href='{callbackUrl}'>link</a>");
            //пока просто редирект


            //return Redirect(Url.Action("ResetPassword","Users", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme));
            return View("ForgotPasswordConfirmation");
        }
        return View(model);
    }
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string code = null)
    {
        return code == null ? View("Error") : View();
    }
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var users = await _usersService.GetAsync();
        Users user = null;
        foreach (var el in users)
        {
            if (el.Token == model.Code)
            {
                user = el;
            }
        }
        if (user == null)
        {
            return View("Error403");
        }
        user.Password = model.Password;
        user.Token = null;
        await _usersService.UpdateAsync(user.Id, user);
        return View("Successfully");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
        Request.ContentType = "application/json";
        if (ModelState.IsValid)
        {
            var user = await _usersService.GetAsync();
            foreach (var el in user.AsReadOnly())
            {
                if (el.Name == model.Name)
                {
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
                    return View(model);
                }
            }
            // добавляем пользователя в бд
            var newUser = new Users {Name = model.Name, Password = model.Password, Surname = "", Email = model.Email};
            await _usersService.CreateAsync(newUser);
            return Redirect(Url.Action("Login","Users"));
        }
        return View(model);
    }

    //[HttpPut("{id:length(24)}")]
    //public async Task<IActionResult> Update(string id, Users updatedUser)
    //{
    //    var user = await _usersService.GetAsync(id);

    //    if (user is null)
    //    {
    //        return NotFound();
    //    }

    //    updatedUser.Id = user.Id;

    //    await _usersService.UpdateAsync(id, updatedUser);

    //    return NoContent();
    //}

    //[HttpDelete("{id:length(24)}")]
    //public async Task<IActionResult> Delete(string id)
    //{
    //    var user = await _usersService.GetAsync(id);

    //    if (user is null)
    //    {
    //        return NotFound();
    //    }

    //    await _usersService.RemoveAsync(id);

    //    return NoContent();
    //}
    [HttpGet]
    //auto
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginModel model)
    {
        Request.ContentType = "application/json";
        if (ModelState.IsValid)
        {
            var user = await _usersService.GetAsync();
            foreach (var el in user)
            {
                if (el.Name == model.Name && el.Password == model.Password)
                {
                    await Authenticate(model.Name);
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Некорректные логин и(или) пароль");
        }

        return View(model);
    }
    
    private async Task Authenticate(string user)
    {
        // создаем один claim
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, user)
        };
        // создаем объект ClaimsIdentity
        ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        // установка аутентификационных куки
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

    }
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Users");
    }
}