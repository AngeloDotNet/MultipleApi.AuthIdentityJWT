namespace WebApi.Authentication.Models.InputModels;

public class RegisterInputModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }
}
