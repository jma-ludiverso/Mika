using MikaWeb.Areas.Identity.Data;
using System.Threading.Tasks;

namespace MikaWeb.Areas.Identity
{
    public interface ITokenClaimsService
    {
        Task<string> GetTokenAsync(MikaWebUser user);

    }
}
