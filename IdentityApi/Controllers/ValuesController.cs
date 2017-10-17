using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Intreba.Identity.Api.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Intreba.Identity.Api.Controllers
{
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpPost("api/users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, [FromServices] UserManager<ApplicationUser> userManager)
        {
            var applicationUser = new ApplicationUser
            {
                PhoneNumber = request.PhoneNumber,
                UserName = request.EmailAddress,
                Email = request.EmailAddress
            };
            await userManager.CreateAsync(applicationUser, request.Password);
            var claims = request.Claims.Select(x => new Claim(x.Key, x.Value));
            await userManager.AddClaimsAsync(applicationUser, claims);
            return Ok();
        }
    }

    public class CreateUserRequest
    {
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
}
