using Microsoft.AspNetCore.Mvc;
using Moq;
using PublicApi.Controllers;
using PublicApi.DTOs;
using PublicApi.Interfaces;

namespace UnitTests
{
    public class UserControllerTests
    {

        private readonly UserController _controller;

        public UserControllerTests()
        {
            _controller = new UserController(Mock.Of<ITokenService>());
        }

        [Fact]
        public void Login_ReturnsOk()
        {
            var result = _controller.Login(new LoginDto
            {
                UserName = "admin",
                Password = "admin"
            });

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<UserDto>(((OkObjectResult)result).Value);
        }

        [Fact]
        public void Login_ReturnsUnauthorized()
        {
            var result = _controller.Login(new LoginDto
            {
                UserName = "admin",
                Password = string.Empty
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

    }
}
