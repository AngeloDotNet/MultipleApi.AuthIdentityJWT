namespace WebApi.Authentication.Services.Application;

public interface IIdentityService
{
    Task<AuthViewModel> LoginAsync(LoginInputModel inputModel);
    Task<RegisterViewModel> RegisterAsync(RegisterInputModel inputModel);
    Task<AuthViewModel> RefreshTokenAsync(RefreshTokenInpuModel inputModel);
}