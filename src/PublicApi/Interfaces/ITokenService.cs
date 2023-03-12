using PublicApi.Entities;

namespace PublicApi.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
