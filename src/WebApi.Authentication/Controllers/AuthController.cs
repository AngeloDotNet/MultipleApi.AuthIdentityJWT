using WebApi.Authentication.Services.Application;

namespace WebApi.Authentication.Controllers;

public class AuthController : ControllerBase
{
    private readonly IIdentityService identityService;

    public AuthController(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginInputModel model)
    {
        var response = await identityService.LoginAsync(model);

        if (response != null)
        {
            return Ok(response);
        }

        return BadRequest();
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenInpuModel inputModel)
    {
        var response = await identityService.RefreshTokenAsync(inputModel);

        if (response != null)
        {
            return Ok(response);
        }

        return BadRequest();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterInputModel inputModel)
    {
        var response = await identityService.RegisterAsync(inputModel);

        if (response.Succeeded)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
}