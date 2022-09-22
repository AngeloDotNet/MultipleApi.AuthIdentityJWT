namespace WebApi.Authentication.Models.InputModels;

public class RefreshTokenInpuModel
{
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
}
