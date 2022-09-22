namespace WebApi.Authentication.Models.ViewModels;

public class RegisterViewModel
{
    public bool Succeeded { get; set; }

    public IEnumerable<string> Errors { get; set; }
}
