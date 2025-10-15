using System.Security.Principal;

namespace NewAPITests
{
    [TestClass]
    public class SessionManagerTest
    {
        const string token = "d5cadeb43f2244bb8a337e973b3d82ce";

        [TestMethod]
        public void TestAddSession()
        {
            SessionManager.AddSession(token, new UserModel { Id = 1 });

            Assert.IsTrue(SessionManager.Sessions.ContainsKey(token));
        }

        [TestMethod]
        public void TestRemoveSession()
        {
            SessionManager.AddSession(token, new UserModel { Id = 1 });

            bool removed = SessionManager.RemoveSession(token);

            Assert.IsTrue(removed);
            Assert.IsFalse(SessionManager.Sessions.ContainsKey(token));
        }

        [TestMethod]
        public void TestGetSession()
        {
            SessionManager.AddSession(token, new UserModel { Id = 1 });

            UserModel? user = SessionManager.GetSession(token);

            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
        }

        [TestMethod]
        public void TestDoesSessionExist()
        {
            SessionManager.AddSession(token, new UserModel { Id = 1 });

            bool exists = SessionManager.DoesSessionExist(token);

            Assert.IsTrue(exists);
        }
    }
}