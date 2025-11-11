using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NewAPITests.EndpointTests
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(_ => { });
        }

        [Fact]
        public async Task Login_HappyFlow_ReturnsToken()
        {
            var client = _factory.CreateClient();
            var u = new UserModel { Username = "e2e_login", Password = "pw", Role = "user", Active = true, CreatedAt = DateTime.UtcNow };
            await UserAccess.CreateUserAsync(u);

            var res = await client.PostAsJsonAsync("/Login", new { username = u.Username, password = "pw" });
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await res.Content.ReadFromJsonAsync<LoginResponse>();
            body.Should().NotBeNull();
            body!.SessionToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_AlternativeFlow_WrongPassword_401()
        {
            var client = _factory.CreateClient();
            var u = new UserModel { Username = "e2e_wrong", Password = "pw", Role = "user", Active = true, CreatedAt = DateTime.UtcNow };
            await UserAccess.CreateUserAsync(u);

            var res = await client.PostAsJsonAsync("/Login", new { username = u.Username, password = "nope" });
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Profile_RequiresAuth_ThenOkWithToken()
        {
            var client = _factory.CreateClient();

            var noAuth = await client.GetAsync("/Profile");
            noAuth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var u = new UserModel { Username = "e2e_profile", Password = "pw", Role = "user", Active = true, CreatedAt = DateTime.UtcNow };
            await UserAccess.CreateUserAsync(u);

            var login = await client.PostAsJsonAsync("/Login", new { username = u.Username, password = "pw" });
            var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.SessionToken;

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", token);

            var ok = await client.GetAsync("/Profile");
            ok.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = await ok.Content.ReadFromJsonAsync<UserModel>();
            user!.Username.Should().Be(u.Username);
        }

        [Fact]
        public async Task Register_HappyFlow_CreatesUser_ThenLoginWorks()
        {
            var client = _factory.CreateClient();
            var newUser = new UserModel { Username = "e2e_register", Password = "pw", Role = "user", Active = true, BirthYear = 1990, CreatedAt = DateTime.UtcNow };

            var reg = await client.PostAsJsonAsync("/Register", newUser);
            reg.StatusCode.Should().Be(HttpStatusCode.OK);

            var login = await client.PostAsJsonAsync("/Login", new { username = newUser.Username, password = "pw" });
            login.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Logout_HappyFlow_ThenProfile401()
        {
            var client = _factory.CreateClient();
            var u = new UserModel { Username = "e2e_logout", Password = "pw", Role = "user", Active = true, CreatedAt = DateTime.UtcNow };
            await UserAccess.CreateUserAsync(u);

            var login = await client.PostAsJsonAsync("/Login", new { username = u.Username, password = "pw" });
            var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.SessionToken;

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", token);

            var bye = await client.GetAsync("/Logout");
            bye.StatusCode.Should().Be(HttpStatusCode.OK);

            var after = await client.GetAsync("/Profile");
            after.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
