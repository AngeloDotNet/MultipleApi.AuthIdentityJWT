using Identity.BusinessLayer.Services;

namespace Identity.WebApp.Services
{
    public class HttpUserService : IUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetUserName()
        {
            return httpContextAccessor.HttpContext.User.Identity.Name;
        }
    }
}
