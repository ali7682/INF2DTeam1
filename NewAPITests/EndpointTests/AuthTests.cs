
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewAPI.Controllers;
using Microsoft.AspNetCore.Http;

namespace NewAPITests.ControllerTests
{
    public class AuthTests
    {
        private readonly CancellationToken ct = CancellationToken.None;
        private readonly AuthController controller;
        private readonly string _examplePassword = "6b37d1ec969838d29cb611deaff50a6b";

        public AuthTests()
        {
            var config = TestConfig.CreateConfig();
            controller = new AuthController(config);

            TestAccessBootstrap.Configure(config);
        }

        private async Task<UserModel> CreateTestUser(string? username = null, string role = "USER")
        {
            var rndUser = new UserModel
            {
                Username = username ?? ("tristenen_" + new Random().Next(9999)),
                Password = _examplePassword,
                Name = "Tristenen Galaretka",
                Email = "tristenen@poort6.nl",
                Phone = "+310612345678",
                Role = role,
                BirthYear = 2020,
                Active = true
            };

            int userId = await UserAccess.CreateUserAsync(rndUser, ct);
            UserModel? created = await UserAccess.GetUserByIdAsync(userId, ct);
            return created;
        }

        private void SetAuthHeader(string token)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;
        }

        // Login

        [Fact]
        public async Task Login_ReturnsOk_WithToken()
        {
            var user = await CreateTestUser();
            var body = new LoginRequest { Username = user.Username, Password = _examplePassword };

            var result = await controller.Login(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);

            var payload = Assert.IsType<LoginResponse>(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(payload.SessionToken));
        }

        [Fact]
        public async Task Login_MissingBody_BadRequest()
        {
            LoginRequest body = null!;

            var result = await controller.Login(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        [Fact]
        public async Task Login_MissingUsername_BadRequest()
        {
            var body = new LoginRequest { Username = "", Password = "wachtwoord" };

            var result = await controller.Login(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        [Fact]
        public async Task Login_MissingPassword_BadRequest()
        {
            var body = new LoginRequest { Username = "gebruikersnaam", Password = "" };

            var result = await controller.Login(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        [Fact]
        public async Task Login_InvalidCredentials_Unauthorized()
        {
            var user = await CreateTestUser();
            var body = new LoginRequest { Username = user.Username, Password = "wachtwoord" };

            var result = await controller.Login(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        // Register

        [Fact]
        public async Task Register_HappyFlow_Ok_ThenLoginWorks()
        {
            var newUser = new UserModel
            {
                Username = "reg_" + new Random().Next(9999),
                Password = _examplePassword,
                Name = "Joris Cicenas",
                Email = "pene@deltoro.com",
                Phone = "+37061288742",
                Role = "USER",
                BirthYear = 2009,
                Active = true
            };

            var reg = await controller.Register(newUser, ct) as ObjectResult;
            Assert.NotNull(reg);
            Assert.Equal(200, reg!.StatusCode);

            var login = await controller.Login(new LoginRequest { Username = newUser.Username, Password = _examplePassword }, ct) as ObjectResult;
            Assert.NotNull(login);
            Assert.Equal(200, login!.StatusCode);
        }

        [Fact]
        public async Task Register_MissingBody_BadRequest()
        {
            UserModel body = null!;

            var result = await controller.Register(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        [Fact]
        public async Task Register_MissingUsername_BadRequest()
        {
            var body = new UserModel { Username = "", Password = "wachtwoord" };

            var result = await controller.Register(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        [Fact]
        public async Task Register_MissingPassword_BadRequest()
        {
            var body = new UserModel { Username = "gebruikersnaam", Password = "" };

            var result = await controller.Register(body, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result!.StatusCode);
        }

        // GET Profile

        [Fact]
        public async Task Profile_Get_WithoutAuth_Unauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            var actionResult = await controller.Profile();
            var result = actionResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Profile_Get_WithInvalidToken_Unauthorized()
        {
            SetAuthHeader("wdadwadwiadwaiewajenwaeaj");

            var actionResult = await controller.Profile();
            var result = actionResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Profile_Get_WithValidToken_Ok()
        {
            var user = await CreateTestUser();
            var loginAction = await controller.Login(new LoginRequest { Username = user.Username, Password = _examplePassword }, ct);
            var login = loginAction as ObjectResult;
            var payload = Assert.IsType<LoginResponse>(login!.Value);

            SetAuthHeader(payload.SessionToken);

            var actionResult = await controller.Profile();
            var result = actionResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);

            var returnedUser = Assert.IsType<UserModel>(result.Value);
            Assert.Equal(user.Username, returnedUser.Username);
        }

        // PUT Profile

        [Fact]
        public async Task Profile_Put_WithoutAuth_Unauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            var actionResult = await controller.Profile(new ChangeProfileRequest { Username = "x" });
            var result = actionResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Profile_Put_WithInvalidToken_Unauthorized()
        {
            SetAuthHeader("wdadwadwiadwaiewajenwaeaj");

            var actionResult = await controller.Profile(new ChangeProfileRequest { Username = "x" });
            var result = actionResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Profile_Put_WithValidToken_ChangesUsername_Ok()
        {
            var user = await CreateTestUser();
            var loginAction = await controller.Login(new LoginRequest { Username = user.Username, Password = _examplePassword }, ct);
            var login = loginAction as ObjectResult;
            var payload = Assert.IsType<LoginResponse>(login!.Value);

            SetAuthHeader(payload.SessionToken);

            string newUsername = "nieuwenaaaaamje";

            var putAction = await controller.Profile(new ChangeProfileRequest { Username = newUsername });
            var putResult = putAction as ObjectResult;
            Assert.NotNull(putResult);
            Assert.Equal(200, putResult!.StatusCode);

            var getAfterAction = await controller.Profile();
            var getAfter = getAfterAction as ObjectResult;
            Assert.NotNull(getAfter);
            Assert.Equal(200, getAfter!.StatusCode);

            var returnedUser = Assert.IsType<UserModel>(getAfter.Value);
            Assert.Equal(newUsername, returnedUser.Username);
        }

        // Logout

        [Fact]
        public async Task Logout_WithoutAuthHeader_Unauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            var actionResult = await controller.Logout();
            var result = actionResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Logout_WithInvalidToken_Unauthorized()
        {
            SetAuthHeader("wdadwadwiadwaiewajenwaeaj");

            var actionResult = await controller.Logout();
            var result = actionResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Logout_WithValidToken_Ok_ThenProfile401()
        {
            var user = await CreateTestUser();
            var loginAction = await controller.Login(new LoginRequest { Username = user.Username, Password = _examplePassword }, ct);
            var login = loginAction as ObjectResult;
            var payload = Assert.IsType<LoginResponse>(login!.Value);

            SetAuthHeader(payload.SessionToken);

            var byeAction = await controller.Logout();
            var bye = byeAction as ObjectResult;
            Assert.NotNull(bye);
            Assert.Equal(200, bye!.StatusCode);

            var afterAction = await controller.Profile();
            var after = afterAction as ObjectResult;
            Assert.NotNull(after);
            Assert.Equal(401, after!.StatusCode);
        }
    }
}
