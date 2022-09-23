using Identity.Authentication.Extensions;
using Identity.Models;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeController : ControllerBase
{
    [HttpGet]
    public IActionResult GetMe()
    {
        var user = new UserResponse
        {
            FirstName = User.GetFirstName(),
            LastName = User.GetLastName(),
            Email = User.GetEmail()
        };

        return Ok(user);
    }
}
