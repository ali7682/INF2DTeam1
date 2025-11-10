// using System.Security.Principal;

// namespace NewAPITests
// {
//     [TestClass]
//     public class SessionManagerTest
//     {
//         [TestMethod]
//         public void TestAddSession()
//         {
//             string token = Guid.NewGuid().ToString("N");

//             SessionManager.AddSession(token, new UserModel { Id = 1 });

//             Assert.IsTrue(SessionManager.Sessions.ContainsKey(token));

//             // Clean up 

//             SessionManager.Sessions.Remove(token);
//         }

//         [TestMethod]
//         public void TestRemoveSession()
//         {
//             string token = Guid.NewGuid().ToString("N");

//             SessionManager.AddSession(token, new UserModel { Id = 1 });

//             bool removed = SessionManager.RemoveSession(token);

//             Assert.IsTrue(removed);
//             Assert.IsFalse(SessionManager.Sessions.ContainsKey(token));
//         }

//         [TestMethod]
//         public void TestGetSession()
//         {
//             string token = Guid.NewGuid().ToString("N");

//             SessionManager.AddSession(token, new UserModel { Id = 1 });

//             UserModel? user = SessionManager.GetSession(token);

//             Assert.IsNotNull(user);
//             Assert.AreEqual(1, user.Id);
//         }

//         [TestMethod]
//         public void TestDoesSessionExist()
//         {
//             string token = Guid.NewGuid().ToString("N");

//             SessionManager.AddSession(token, new UserModel { Id = 1 });

//             bool exists = SessionManager.DoesSessionExist(token);

//             Assert.IsTrue(exists);
//         }
//     }
// }
