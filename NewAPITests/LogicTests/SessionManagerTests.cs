using System.Runtime.CompilerServices;
using Xunit;

namespace NewAPITests.LogicTests
{
    public class SessionManagerTests
    {
        [Fact]
        public async Task TestAddSession()
        {
            // Setup
            int exampleUserId = 1;

            int sessionCountBefore = SessionManager.SessionCount;

            await SessionManager.AddSession("tester", exampleUserId, TestContext.Current.CancellationToken);

            // Assert

            int? requestedSession = await SessionManager.GetSession("tester", TestContext.Current.CancellationToken);

            Assert.True(SessionManager.SessionCount != sessionCountBefore);

            Assert.NotNull(requestedSession);
            
            Assert.True(requestedSession == exampleUserId);

            // Cleanup

            await SessionManager.RemoveSession("tester", TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task TestRemoveSession()
        {
            // Setup 

            int exampleUserId = 1;
            await SessionManager.AddSession("tester", exampleUserId, TestContext.Current.CancellationToken);

            // Assert

            await SessionManager.RemoveSession("tester", TestContext.Current.CancellationToken);

            Assert.Null(await SessionManager.GetSession("tester", TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task TestGetSession()
        {
            // Setup
            int exampleUserId = 1;

            await SessionManager.AddSession("tester", exampleUserId, TestContext.Current.CancellationToken);

            // Assert

            int? requestedUserId = await SessionManager.GetSession("tester", TestContext.Current.CancellationToken);

            Assert.NotNull(requestedUserId);
            Assert.True(requestedUserId == exampleUserId);

            // Cleanup
            await SessionManager.RemoveSession("tester", TestContext.Current.CancellationToken);
        }
    }
}
