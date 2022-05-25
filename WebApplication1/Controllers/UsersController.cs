using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[action]")]
public class UsersController : Controller
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService) =>
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
            var newUser = new Users {Name = model.Name, Password = model.Password, Surname = ""};
            await _usersService.CreateAsync(newUser);
            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
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
            var user = _usersService.GetAsync();
            foreach (var el in user.Result)
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