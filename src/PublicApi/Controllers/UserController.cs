using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs;
using PublicApi.Entities;
using PublicApi.Interfaces;

namespace PublicApi.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public UserController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody]LoginDto loginDto)
        {
            var user = AppUser.ValidUsers.SingleOrDefault(x => x.UserName == loginDto.UserName);

            if (user == null) return Unauthorized("Invalid username or password");

            if (user.Password != loginDto.Password) return Unauthorized("Invalid username or password");

            return Ok(new UserDto
            {
                Token = _tokenService.CreateToken(user)
            });
        }
    }
}
