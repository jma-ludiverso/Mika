using MikaWeb.Areas.Identity.Data;
using MikaWeb.Models.API;
using System.Threading.Tasks;

namespace MikaWeb.Areas.Identity
{
    public interface ITokenClaimsService
    {
        Task<JWTToken> GetTokenAsync(MikaWebUser user);

    }
}
