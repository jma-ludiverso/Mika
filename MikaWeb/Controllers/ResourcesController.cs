using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MikaWeb.Areas.Identity;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Extensions;
using MikaWeb.Extensions.DB;
using MikaWeb.Models.API;
using System.Threading.Tasks;

namespace MikaWeb.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ResourcesController : ControllerBase
    {

        private readonly DBConfig _dbConfig;
        private readonly ITokenClaimsService _tokenClaimsService;
        UserManager<MikaWebUser> _userManager;

        public ResourcesController(UserManager<MikaWebUser> userManager, ITokenClaimsService tokenClaimsService, IOptions<DBConfig> dbConf) 
        {
            _dbConfig = dbConf.Value;
            _tokenClaimsService = tokenClaimsService;
            _userManager= userManager;
        }

        [HttpPost]
        [Route("authenticate")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticateResponse>> Authenticate(AuthenticateRequest request)
        {
            var response = new AuthenticateResponse();
            try
            {
                var user = await _userManager.FindByNameAsync(request.UserName);
                if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    throw new System.Exception("Usuario / contraseña no válidos");
                }
                response.UserData = user;
                response.Token = await _tokenClaimsService.GetTokenAsync(user);
                response.Succeeded = true;
            }
            catch
            {
                response.Succeeded = false;
            }
            return response;
        }

        [HttpPost]
        [Route("getdata")]
        public async Task<ActionResult<ServerData>> getData(DataRequest request)
        {
            try
            {
                ApiData data = new ApiData(_dbConfig);
                return await data.GetServerData(request.Salon);
            }
            catch
            {
                return BadRequest();
            }
        }

    }
}
