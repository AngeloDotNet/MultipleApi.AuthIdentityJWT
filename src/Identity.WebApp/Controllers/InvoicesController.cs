using Identity.Authentication;
using Identity.Authentication.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RoleAuthorize(RoleNames.Administrator)]
    public class InvoicesController : ControllerBase
    {
        [HttpGet]
        //[RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
        public IActionResult GetInvoices()
        {
            return NoContent();
        }

        [HttpPost]
        //[Authorize(Roles = RoleNames.Administrator)]
        public IActionResult SaveInvoice()
        {
            return NoContent();
        }
    }
}
