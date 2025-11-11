using Xunit;

namespace NewAPITests.LogicTests
{
    public class SessionManagerTests
    {
        [Fact]
        public void TestAddSession()
        {
            // Setup
            UserModel user = new();

            SessionManager.AddSession("tester", user);

            // Assert

            UserModel? requestedSession = SessionManager.GetSession("tester");


            Assert.NotNull(requestedSession);
            
            Assert.True(requestedSession == user);

            // Cleanup

            SessionManager.RemoveSession("tester");
        }

        [Fact]
        public void TestRemoveSession()
        {
            // Setup 

            UserModel user = new();
            SessionManager.AddSession("tester", user);

            // Assert

            SessionManager.RemoveSession("tester");

            Assert.Null(SessionManager.GetSession("tester"));
        }

        [Fact]
        public void TestGetSession()
        {
            // Setup
            UserModel user = new();

            SessionManager.AddSession("tester", user);

            // Assert

            UserModel? requestedSession = SessionManager.GetSession("tester");

            Assert.IsType<UserModel>(requestedSession);
            Assert.True(requestedSession == user);

            // Cleanup
            SessionManager.RemoveSession("tester");
        }
    }
}
