using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Net.HttpStatusCode;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Dynamic;
using System.Net;
using System.IO;
using System.Text;
using Boggle;
using System.Threading;

namespace Boggle
{
    /// <summary>
    /// Provides a way to start and stop the IIS web server from within the test
    /// cases.  If something prevents the test cases from stopping the web server,
    /// subsequent tests may not work properly until the stray process is killed
    /// manually.
    /// </summary>
    public static class IISAgent
    {
        // Reference to the running process
        private static Process process = null;

        /// <summary>
        /// Starts IIS
        /// </summary>
        public static void Start(string arguments)
        {
            if (process == null)
            {
                ProcessStartInfo info = new ProcessStartInfo(Properties.Resources.IIS_EXECUTABLE, arguments);
                info.WindowStyle = ProcessWindowStyle.Minimized;
                info.UseShellExecute = false;
                process = Process.Start(info);
            }
        }

        /// <summary>
        ///  Stops IIS
        /// </summary>
        public static void Stop()
        {
            if (process != null)
            {
                process.Kill();
            }
        }
     

    }



    [TestClass]
    public class BoggleTests
    {
        /// <summary>
        /// This is automatically run prior to all the tests to start the server
        /// </summary>
        [ClassInitialize()]
        public static void StartIIS(TestContext testContext)
        {
            IISAgent.Start(@"/site:""BoggleService"" /apppool:""Clr4IntegratedAppPool"" /config:""..\..\..\.vs\config\applicationhost.config""");
        }

        /// <summary>
        /// This is automatically run when all tests have completed to stop the server
        /// </summary>
        [ClassCleanup()]
        public static void StopIIS()
        {
            IISAgent.Stop();
        }

        private RestTestClient client = new RestTestClient("http://localhost:60000/BoggleService.svc/");

        ///// <summary>
        ///// Note that DoGetAsync (and the other similar methods) returns a Response object, which contains
        ///// the response Stats and the deserialized JSON response (if any).  See RestTestClient.cs
        ///// for details.
        ///// </summary>
        //[TestMethod]
        //public void TestMethod1()
        //{
        //    Response r = client.DoGetAsync("word?index={0}", "-5").Result;
        //    Assert.AreEqual(Forbidden, r.Status);

        //    r = client.DoGetAsync("word?index={0}", "5").Result;
        //    Assert.AreEqual(OK, r.Status);

        //    string word = (string)r.Data;
        //    Assert.AreEqual("AAL", word);
        //}
        /// <summary>
        /// check the user is empty
        /// </summary>
        [TestMethod]
        public void testRegister1()
        {
            dynamic user = new ExpandoObject();
            user.Nickname = "";

            Response r = client.DoPostAsync("users", user).Result;
            Assert.AreEqual(Forbidden, r.Status);
            
        }
        /// <summary>
        /// check create with empty user name
        /// </summary>
        [TestMethod]
        public void testRegister2()
        {
            dynamic user = new ExpandoObject();
            user.Nickname = "  ";

            Response r = client.DoPostAsync("users", user).Result;
            Assert.AreEqual(Forbidden, r.Status);

        }
        /// <summary>
        /// check create user with @
        /// </summary>
        [TestMethod]
        public void testRegister3()
        {
            dynamic user = new ExpandoObject();
            user.Nickname = "@aaaa";

            Response r = client.DoPostAsync("users", user).Result;
            Assert.AreEqual(Created, r.Status);

        }
        /// <summary>
        /// check create user normally
        /// </summary>
        [TestMethod]
        public void testRegister4()
        {
            dynamic user = new ExpandoObject();
            user.Nickname = "aaaaa";

            Response r = client.DoPostAsync("users", user).Result;
            Assert.AreEqual(Created, r.Status);

        }
        /// <summary>
        /// Test a player join game
        /// </summary>
        [TestMethod]
        public void testJoinGame1()
        {
            dynamic user = new ExpandoObject();
            user.Nickname = "aa";
            Response r = client.DoPostAsync("users", user).Result;

            dynamic player = new ExpandoObject();
            player.UserToken = r.Data.UserToken;
            player.TimeLimit = 120;

            Response r1 = client.DoPostAsync("games", player).Result;
            Assert.AreEqual(Accepted, r1.Status);
        }
        /// <summary>
        /// Test second player join game
        /// </summary>
        [TestMethod]
        public void testJoinGame2()
        {
            // add first user
            dynamic user = new ExpandoObject();
            user.Nickname = "aa";
            Response r = client.DoPostAsync("users", user).Result;

            dynamic player = new ExpandoObject();
            player.UserToken = r.Data.UserToken;
            player.TimeLimit = 120;

            Response r1 = client.DoPostAsync("games", player).Result;
            Assert.AreEqual(Created, r1.Status);

            // add second user
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "bb";
            Response r3 = client.DoPostAsync("users", user).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 120;

            Response r4 = client.DoPostAsync("games", player2).Result;
            Assert.AreEqual(Accepted, r4.Status);


          

        }
        /// <summary>
        /// test with invalid time limit
        /// </summary>
        [TestMethod]
        public void testJoinGame3()
        {
            // add first user
            dynamic user = new ExpandoObject();
            user.Nickname = "aa";
            Response r = client.DoPostAsync("users", user).Result;

            dynamic player = new ExpandoObject();
            player.UserToken = r.Data.UserToken;
            player.TimeLimit = 121;

            Response r1 = client.DoPostAsync("games", player).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }
        /// <summary>
        /// test with adding same userToken player
        /// **this test is not necessary, and conflict with the requirement. 
        /// </summary>
        [TestMethod]
        public void testJoinGame4()
        {
            // add first user
            dynamic user = new ExpandoObject();
            user.Nickname = "aa";
            Response r = client.DoPostAsync("users", user).Result;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "aaa";
            Response test1 = client.DoPostAsync("users", user).Result;

            dynamic test3 = new ExpandoObject();
            test3.UserToken = test1.Data.UserToken;
            test3.TimeLimit = 120;

            Response test2 = client.DoPostAsync("games", test3).Result;


            dynamic player = new ExpandoObject();
            player.UserToken = r.Data.UserToken;
            player.TimeLimit = 120;

            Response r1 = client.DoPostAsync("games", player).Result;
            Assert.AreEqual(Accepted, r1.Status);

            // add second user
            
            Response r4 = client.DoPostAsync("games", player).Result;
            Assert.AreEqual(Conflict, r4.Status);


            user.Nickname = "aa";
            Response test5 = client.DoPostAsync("users", user).Result;

            dynamic test6 = new ExpandoObject();
            test6.UserToken = test5.Data.UserToken;
            test6.TimeLimit = 120;

            Response test7 = client.DoPostAsync("games", player).Result;

        }
        /// <summary>
        /// test cancel game normally
        /// </summary>
        [TestMethod]
        public void testCancelJoin1()
        {
            dynamic test9 = new ExpandoObject();
            test9.Nickname = "aa";
            Response test5 = client.DoPostAsync("users", test9).Result;

            dynamic test6 = new ExpandoObject();
            test6.UserToken = test5.Data.UserToken;
            test6.TimeLimit = 120;

            Response test7 = client.DoPostAsync("games", test6).Result;


            dynamic user = new ExpandoObject();
            user.Nickname = "aaaa";
            Response r = client.DoPostAsync("users", user).Result;

            dynamic player = new ExpandoObject();
            player.UserToken = r.Data.UserToken;
            player.TimeLimit = 120;
            Response r1 = client.DoPostAsync("games", player).Result;

            dynamic cancelRequest = new ExpandoObject();
            cancelRequest.UserToken = player.UserToken;

            Response r2 = client.DoPutAsync(cancelRequest, "games").Result;
            //---------------------------------
            Assert.AreEqual(OK, r2.Status);

        }
        /// <summary>
        /// test cancel game non-normally
        /// </summary>
        [TestMethod]
        public void testCancelJoin2()
        {
            dynamic user = new ExpandoObject();
            user.Nickname = "aaaa";
            Response r = client.DoPostAsync("users", user).Result;

            dynamic player = new ExpandoObject();
            player.UserToken = r.Data.UserToken;
            player.TimeLimit = 120;
            Response r1 = client.DoPostAsync("games", player).Result;

            dynamic cancelRequest = new ExpandoObject();
            cancelRequest.UserToken ="aaaaa";

            Response r2 = client.DoPutAsync(cancelRequest, "games").Result;
            Assert.AreEqual(Forbidden, r2.Status);
        }
        [TestMethod]
        public void testCalculateScore()
        {
            BoggleService boggle = new BoggleService();
            
            Assert.AreEqual(0, boggle.CalculateScore("a"));
            Assert.AreEqual(7, boggle.CalculateScore("aaaaaaaa"));
            Assert.AreEqual(1, boggle.CalculateScore("aaa"));
            Assert.AreEqual(2, boggle.CalculateScore("aaaaa"));
            Assert.AreEqual(3, boggle.CalculateScore("aaaaaa"));
            Assert.AreEqual(5, boggle.CalculateScore("aaaaaaa"));


        }
        [TestMethod]
        public void testPlayWord()
        {
            // add first player
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic player1 = new ExpandoObject();
            player1.UserToken = r1.Data.UserToken;
            player1.TimeLimit = 120;
            Response r2 = client.DoPostAsync("games", player1).Result;

            //add second player
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "bbbb";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 120;
            Response r4 = client.DoPostAsync("games", player2).Result;

            Dictionary<string, string> players = new Dictionary<string, string>();
            players.Add("usertoken", r1.Data.UserToken.ToString());
            players.Add("gameID", r2.Data.GameID.ToString());

            dynamic playWord = new ExpandoObject();
            playWord.UserToken = players["usertoken"];
            playWord.Word = "aaaaaaaaaaaaaaaaaaa";
            Response r5 = client.DoPutAsync(playWord, "games/" + players["gameID"]).Result;
            Assert.AreEqual(OK, r5.Status);

            Assert.AreEqual(-1, (Int32)r5.Data.Score);





        }
        [TestMethod]
        public void testToken()
        {
            String token = Guid.NewGuid().ToString() ;
            BoggleService A = new BoggleService();
            Assert.IsTrue(A.checkUserTokenValid(token));
            Assert.IsFalse(A.checkUserTokenValid("aaa"));

        }
        [TestMethod]
        public void testBrief()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic player1 = new ExpandoObject();
            player1.UserToken = r1.Data.UserToken;
            player1.TimeLimit = 120;
            Response r2 = client.DoPostAsync("games", player1).Result;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "aaaa";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 120;
            Response r4 = client.DoPostAsync("games", player2).Result;

            //BoggleService A = new BoggleService();
            // A.helpBrief((String)r4.Data.GameID);

            String url = "games/" + ((Int32)r4.Data.GameID) + "?Brief=yes";
            Response r5 = client.DoGetAsync(url).Result;
            Assert.AreEqual(OK, r5.Status);
        }
        [TestMethod]
        public void testActive()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic player1 = new ExpandoObject();
            player1.UserToken = r1.Data.UserToken;
            player1.TimeLimit = 120;
            Response r2 = client.DoPostAsync("games", player1).Result;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "aaaa";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 120;
            Response r4 = client.DoPostAsync("games", player2).Result;         
            String url = "games/" + ((Int32)r4.Data.GameID);
            Response r5 = client.DoGetAsync(url).Result;
            Assert.AreEqual(OK, r5.Status);
        }

        [TestMethod]
        public void testComplete()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic player1 = new ExpandoObject();
            player1.UserToken = r1.Data.UserToken;
            player1.TimeLimit = 7;
            Response r2 = client.DoPostAsync("games", player1).Result;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "aaaa";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 7;
            Response r4 = client.DoPostAsync("games", player2).Result;
            Thread.Sleep(8000);
            String url = "games/" + ((Int32)r4.Data.GameID);
            Response r5 = client.DoGetAsync(url).Result;
            Assert.AreEqual(OK, r5.Status);


            r2 = client.DoPostAsync("games", player1).Result;
           // r4 = client.DoPostAsync("games", player2).Result;
         //   r5 = client.DoGetAsync(url).Result;
           // r5 = client.DoGetAsync(url).Result;
          //  r5 = client.DoGetAsync(url+"?Brief=no").Result;
        }
        [TestMethod]
        public void testBrief2()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic player1 = new ExpandoObject();
            player1.UserToken = r1.Data.UserToken;
            player1.TimeLimit = 7;
            Response r2 = client.DoPostAsync("games", player1).Result;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "aaaa";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 7;
            Response r4 = client.DoPostAsync("games", player2).Result;
            Thread.Sleep(8000);
            String url = "games/" + ((Int32)r4.Data.GameID+"?Brief=yes");
            Response r5 = client.DoGetAsync(url).Result;
            r5 = client.DoGetAsync(url).Result;
            Assert.AreEqual(OK, r5.Status);



            
            r5 = client.DoGetAsync("games/" + ((Int32)r4.Data.GameID)).Result;
            r2 = client.DoPostAsync("games", player1).Result;
            r4 = client.DoPostAsync("games", player2).Result;
            r5 = client.DoGetAsync("games/" + ((Int32)r4.Data.GameID)).Result;

        }
        [TestMethod]
        public void testForbiden()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic player1 = new ExpandoObject();
            player1.UserToken = r1.Data.UserToken;
            player1.TimeLimit = 120;
            Response r2 = client.DoPostAsync("games", player1).Result;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "aaaa";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 120;
            Response r4 = client.DoPostAsync("games", player2).Result;
            String url = "games/" + 1173311048 + "?Brief=yes";
            Response r5 = client.DoGetAsync(url).Result;
            r5 = client.DoGetAsync(url).Result;
            Assert.AreEqual(Forbidden, r5.Status);


            url = "games/" + ((Int32)r4.Data.GameID + "?Brief=yes");
            r5 = client.DoGetAsync(url).Result;
            url = "games/" + ((Int32)r4.Data.GameID );
            r5 = client.DoGetAsync(url).Result;
        }
        [TestMethod]
        public void testCancel3()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic token = new ExpandoObject();
            token.UserToken = r1.Data.UserToken;
            Response r5 = client.DoPutAsync(token,"games").Result;
            Assert.AreEqual(Forbidden, r5.Status);

        }
        [TestMethod]
        public void testCancel4()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;


            dynamic token = new ExpandoObject();
            token.UserToken = r1.Data.UserToken;
            Response r5 = client.DoPutAsync(token, "games").Result;
            Assert.AreEqual(Forbidden, r5.Status);

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "aaaa";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 120;
            Response r4 = client.DoPostAsync("games", player2).Result;

             r5 = client.DoPutAsync(token, "games").Result;
            Assert.AreEqual(Forbidden, r5.Status);

            r5 = client.DoPutAsync(token, "games").Result;
        }

        [TestMethod]
        public void testPlayWord2()
        {
            // add first player
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "aaaa";
            Response r1 = client.DoPostAsync("users", user1).Result;

            dynamic player1 = new ExpandoObject();
            player1.UserToken = r1.Data.UserToken;
            player1.TimeLimit = 120;
            Response r2 = client.DoPostAsync("games", player1).Result;

            //add second player
            dynamic user2 = new ExpandoObject();
            user2.Nickname = "bbbb";
            Response r3 = client.DoPostAsync("users", user2).Result;

            dynamic player2 = new ExpandoObject();
            player2.UserToken = r3.Data.UserToken;
            player2.TimeLimit = 120;
            Response r4 = client.DoPostAsync("games", player2).Result;

         
            dynamic playWord = new ExpandoObject();
            playWord.UserToken = r1.Data.UserToken;
            playWord.Word = "aaaaaaaaaaaaaaaaaaa";
            Response r5 = client.DoPutAsync(playWord, "games/" + (Int32)r2.Data.GameID).Result;
            Assert.AreEqual(OK, r5.Status);

            Assert.AreEqual(-1, (Int32)r5.Data.Score);
            dynamic playWord2 = new ExpandoObject();
            playWord2.UserToken = player2.UserToken;
            playWord2.Word = "aaaaaaaaaaaaaaaaaaa";
             r5 = client.DoPutAsync(playWord, "games/" + (Int32)r2.Data.GameID).Result;

            r5 =client.DoPutAsync(playWord, "games/" + r2.Data.GameID).Result;





        }

    }
}
