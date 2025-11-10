using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

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
