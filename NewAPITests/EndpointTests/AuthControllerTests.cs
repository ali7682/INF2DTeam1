using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NewAPITests.EndpointTests
{
    class AuthControllerTests
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(_ => { });
        }
    }
}
