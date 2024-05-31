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
using NPOI.OpenXmlFormats.Dml;
using System.Security.Claims;
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
                response = await this.AuthenticateData(user);
            }
            catch
            {
                response.Succeeded = false;
            }
            return response;
        }

        private async Task<AuthenticateResponse> AuthenticateData(MikaWebUser user)
        {
            var response = new AuthenticateResponse();
            try
            {
                response.UserData = user;
                JWTToken token = await _tokenClaimsService.GetTokenAsync(response.UserData);
                response.Token = token.Token;
                response.TokenExpires = token.Expires;
                response.Succeeded = true;
            }
            catch
            {
                response.Succeeded= false;
            }
            return response;
        }

        [HttpPost]
        [Route("authenticaterenewal")]
        public async Task<ActionResult<AuthenticateResponse>> AuthenticateRenewal()
        {
            var response = new AuthenticateResponse();
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                response = await this.AuthenticateData(await _userManager.FindByIdAsync(identity.FindFirst(ClaimTypes.Sid).Value));
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
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                ApiData data = new ApiData(_dbConfig);
                return await data.GetServerData(request.Salon, identity.FindFirst(ClaimTypes.Sid).Value);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("setData")]
        public async Task<ActionResult> setData(ClientData data)
        {
            try
            {
                ApiData appdata = new ApiData(_dbConfig);
                bool result = await appdata.setClientData(data);
                if(result)
                {
                    return Ok("DataSaved");
                }
                else
                {
                    throw new System.Exception();
                }
            }catch
            {
                return BadRequest();
            }
        }

    }
}
