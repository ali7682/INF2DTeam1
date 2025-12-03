
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

        public AuthTests()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            controller = new AuthController(config);
        }

        private async Task<UserModel> CreateTestUser(string? username = null, string role = "USER")
        {
            var rndUser = new UserModel
            {
                Username = username ?? ("tristenen_" + new Random().Next(9999)),
                Password = "6b37d1ec969838d29cb611deaff50a6b",
                Name = "Tristenen Galaretka",
                Email = "tristenen@poort6.nl",
                Phone = "+310612345678",
                Role = role,
                BirthYear = 2020,
                Active = true
            };

            int userId = await UserAccess.CreateUserAsync(rndUser, ct);
            var created = await UserAccess.GetUserByIdAsync(userId, ct);
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
            var body = new LoginRequest { Username = user.Username, Password = user.Password };

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
                Password = "6b37d1ec969838d29cb611deaff50a6b",
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

            var login = await controller.Login(new LoginRequest { Username = newUser.Username, Password = newUser.Password }, ct) as ObjectResult;
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
        public void Profile_Get_WithoutAuth_Unauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            var result = controller.Profile() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public void Profile_Get_WithInvalidToken_Unauthorized()
        {
            SetAuthHeader("wdadwadwiadwaiewajenwaeaj");

            var result = controller.Profile() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Profile_Get_WithValidToken_Ok()
        {
            var user = await CreateTestUser();
            var login = await controller.Login(new LoginRequest { Username = user.Username, Password = user.Password }, ct) as ObjectResult;
            var payload = Assert.IsType<LoginResponse>(login!.Value);

            SetAuthHeader(payload.SessionToken);

            var result = controller.Profile() as ObjectResult;

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

            var result = await controller.Profile(new ChangeProfileRequest { Username = "x" }) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Profile_Put_WithInvalidToken_Unauthorized()
        {
            SetAuthHeader("wdadwadwiadwaiewajenwaeaj");

            var result = await controller.Profile(new ChangeProfileRequest { Username = "x" }) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Profile_Put_WithValidToken_ChangesUsername_Ok()
        {
            var user = await CreateTestUser();
            var login = await controller.Login(new LoginRequest { Username = user.Username, Password = user.Password }, ct) as ObjectResult;
            var payload = Assert.IsType<LoginResponse>(login!.Value);

            SetAuthHeader(payload.SessionToken);

            string newUsername = "nieuwenaaaaamje";

            var putResult = await controller.Profile(new ChangeProfileRequest { Username = newUsername }) as ObjectResult;
            Assert.NotNull(putResult);
            Assert.Equal(200, putResult!.StatusCode);

            var getAfter = controller.Profile() as ObjectResult;
            Assert.NotNull(getAfter);
            Assert.Equal(200, getAfter!.StatusCode);

            var returnedUser = Assert.IsType<UserModel>(getAfter.Value);
            Assert.Equal(newUsername, returnedUser.Username);
        }

        // Logout

        [Fact]
        public void Logout_WithoutAuthHeader_Unauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            var result = controller.Logout() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public void Logout_WithInvalidToken_Unauthorized()
        {
            SetAuthHeader("wdadwadwiadwaiewajenwaeaj");

            var result = controller.Logout() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public async Task Logout_WithValidToken_Ok_ThenProfile401()
        {
            var user = await CreateTestUser();
            var login = await controller.Login(new LoginRequest { Username = user.Username, Password = user.Password }, ct) as ObjectResult;
            var payload = Assert.IsType<LoginResponse>(login!.Value);

            SetAuthHeader(payload.SessionToken);

            var bye = controller.Logout() as ObjectResult;
            Assert.NotNull(bye);
            Assert.Equal(200, bye!.StatusCode);

            var after = controller.Profile() as ObjectResult;
            Assert.NotNull(after);
            Assert.Equal(401, after!.StatusCode);
        }
    }
}
