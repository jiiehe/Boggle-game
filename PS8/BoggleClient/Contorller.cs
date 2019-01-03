using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;
using Newtonsoft.Json;
using BoggleModel;
using System.Threading;
/// <summary>
/// Author by Lin Jia&& Jin HE
/// </summary>
namespace BoggleClient
{
    /// <summary>
    /// Starting point for Controller. DON'T MAKE view and model know each other. 
    /// </summary>
    public class contorller
    {
        /// <summary>
        /// declare the view
        /// </summary>
        private IBoggle view;
        /// <summary>
        /// declare the view
        /// </summary>
        private Model model;
        /// <summary>
        /// declare player
        /// </summary>
        private Player player1;
        /// <summary>
        /// For canceling the current operation
        /// </summary>
        private CancellationTokenSource tokenSource;
        /// <summary>
        /// declare reponse status
        /// </summary>
        private  String reponseStatus;
        /// <summary>
        /// the constructor of contorllor, all event will handled in this class 
        /// </summary>
        public contorller(IBoggle view)
        {
            tokenSource = new CancellationTokenSource();
            this.view = view;         
            this.view.LeaveGameEvent += Cancel;
            this.view.creatUserEvent += CreateUser;
            this.view.UserTokenEvent += getToken;
            this.view.joinGameEvent += JoinGame;
            this.view.cancelGameEvent += CancelJoinRequest;
            this.view.CheckStatusEvent += wannaMoreData;
            this.view.playEvent += playaWord;
            this.view.ExitGameEvent += callExit;
            this.view.helpmenuEvent += helpMenuContent;
            this.view.helpMenuClickEvent += callClickHelpMenu;
        }
        /// <summary>
        /// add help menu content
        /// </summary>
        /// <returns>help information. </returns>
        public string helpMenuContent()
        {
            return "Boggle Game:\n" +
"Our boggle game has three panels: login panel, pending panel and game panel.\n" +
"\n1.Login Panel: It is used to enter the user name and time limit. And it has two button: login and Quit.\n" +
"Login: After user enters a nickname and a valid time limit and clicks login, the main panel will change to pending panel to wait to the other user to join the game or directly enter a game someone already created.\n" +
"Quit: After user pressed the quit button, the program will stop immediately.\n" +
"\n2.Pending Panel: After the user created a new game, the main panel will change to pending panel and start to wait other user to join your game. It has a Cancel button that user can use it to stop waiting other user to enter the game.After user pressed cancel button, the main panel changes back to login panel.\n" +
"\n3.Game Panel: After user enters game panel, the game starts.It could let user to type in words to get points. If the word is correct, the user will get points according to the words length. Game panel has two button: Cancel the Game and Exit.\n" +
"Cancel the Game: user can use it to stop to the ongoing game.\n" +
"Exit: After user pressed the exit button, the program will stop immediately.\n" + "\nOur Program is used call back instead of timer.\n";
        }
        /// <summary>
        /// call the click help menu method
        /// </summary>
        private void callClickHelpMenu()
        {
            this.view.clickHelpMenu();
        }
        /// <summary>
        /// call the exit game method
        /// </summary>
        private void callExit()
        {
            this.view.shutDownGame();
        }
        /// <summary>
        /// Cancels the current operation (currently unimplemented)
        /// </summary>
        private void Cancel()
        {
            tokenSource.Cancel();
        }
        /// <summary>
        /// get the word list from server
        /// </summary>
        /// <param name="word">parameter</param>
        /// <param name="domain">parameter</param>
        public void playaWord(String word, String domain)
        {
            word = word.ToUpper();
            PlayWord(this.getToken(), word, domain);

        }
        /// <summary>
        /// method to return user token
        /// </summary>
        /// <returns>parameter</returns>
        public String getToken()
        {
           
            return this.player1.UserToken;
            
        }
        /// <summary>
        /// method used to check the game status than reset all game status
        /// </summary>
        /// <param name="brief">parameter</param>
        /// <param name="domain">parameter</param>
        public void wannaMoreData(bool brief,String domain)
        {
            try
            {
                if (brief == true)
                {


                    GameStatusAsync(true,domain);

                    if (this.model.GameState == "completed")
                    {
                        wannaMoreData(false,domain);
                    }
                    else
                    {
                        this.view.repaint(this.model.TimeLeft, this.model.Player1.Score, this.model.Player2.Score);
                    }
                }
                // when the brief is false
                else
                {
                    GameStatusAsync(false,domain);

                    if (this.model.GameState == "active")
                    {
                        this.view.paintActive(this.model.Board, this.model.TimeLeft, this.model.Player1.Nickname, this.model.Player1.Score, this.model.Player2.Nickname, this.model.Player2.Score);

                    }
                    else
                    if (this.model.GameState == "pending")
                    {
                        this.view.waitPlayerComing();
                    }
                    else
                    if (this.model.GameState == "completed")
                    {
                        this.view.getResult(this.model.Player1.Nickname, this.model.Player1.Score, this.model.Player1.WordsPlayed, this.model.Player2.Nickname, this.model.Player2.Score, this.model.Player2.WordsPlayed);
                    }
                }
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    if (e is TaskCanceledException)
                    {
                        
                        this.view.stopService();
                    }

                    else
                    {
                        Console.WriteLine("Exception: " + e.GetType().Name);
                    }
                }


            }
        }


        /// <summary>
        /// Create a client to communicate with server
        /// </summary>
        /// <returns>HttpClient</returns>
        private HttpClient CreateClient(String domain)
        {
            // Create a client whose base address is the Github server
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(domain);

            // Tell the server that the client will accept this particular type of response of data
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // There is more client configuration to do, depending on request
            return client;
        }


        /// <summary>
        ///  Create a user
        /// </summary>
        public   void CreateUser(string nickname,String domain)
        {
            try
            {
                using (HttpClient client = CreateClient(domain))
                {
                    tokenSource = new CancellationTokenSource();
                    //Create the parameter -----------------??????
                    dynamic game = new ExpandoObject();
                    game.Nickname = nickname;

                    // Compose and send the request
                    StringContent content = new StringContent(JsonConvert.SerializeObject(game), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync("users", content,tokenSource.Token).Result;

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        dynamic user = JsonConvert.DeserializeObject(result);
                        this.player1 = new Player(nickname);
                        this.player1.UserToken = user.UserToken;


                    }
                    else
                    {
                        reponseStatus = "Creating User Error " + response.StatusCode + ", " + response.ReasonPhrase;
                        this.view.errorEvent(reponseStatus);
                        Thread.CurrentThread.Abort();
                    }
                }
            }
            catch(AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    if (e is TaskCanceledException)
                    {

                        throw new TaskCanceledException();
                    }

                    else
                    {
                        //used to deal with when the domain is not a real domain. 
                        this.view.errorEvent("wrong domain information. ");
                        throw new TaskCanceledException();
                    }
                }
            }
            
           
        }
        /// <summary>
        /// join game method
        /// </summary>
        /// <param name="userToken">parameter</param>
        /// <param name="timeLimit">parameter</param>
        /// <param name="domain">parameter</param>
        public void JoinGame(string userToken, int timeLimit,String domain)
        {
            using (HttpClient client = CreateClient(domain))
            {
                dynamic data = new ExpandoObject();
                data.UserToken = userToken;
                data.TimeLimit = timeLimit;

                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response =  client.PostAsync("games", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    dynamic game = JsonConvert.DeserializeObject(result);
                    int gameID = game.GameID;
                    this.model = new Model(this.player1);
                    this.model.GameID = gameID;
                    
                }
                else
                {
                    reponseStatus = "Joining Game Error: " + response.StatusCode + ", " + response.ReasonPhrase;
                    this.view.errorEvent(reponseStatus);
                    Thread.CurrentThread.Abort();
                }
            }
        }
        /// <summary>
        /// cancel join request method
        /// </summary>
        /// <param name="userToken">parameter</param>
        /// <param name="domain">parameter</param>
        public void CancelJoinRequest(string userToken,String domain)
        {
            
            using (HttpClient client = CreateClient(domain))
            {
                dynamic data = new ExpandoObject();
                data.UserToken = userToken;

                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response =  client.PutAsync("games", content).Result;
               
               
                
            }
        }
        /// <summary>
        /// play word method
        /// </summary>
        /// <param name="userToken">parameter</param>
        /// <param name="word">parameter</param>
        /// <param name="domain">parameter</param>
        public void PlayWord(string userToken, string word,String domain)
        {
            using (HttpClient client = CreateClient(domain))
            {
                dynamic data = new ExpandoObject();
                data.UserToken = userToken;
                data.Word = word;

                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response =  client.PutAsync("games/" + this.model.GameID, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result =  response.Content.ReadAsStringAsync().Result;
                    dynamic gameResult = JsonConvert.DeserializeObject(result);
                    int Score = gameResult.Score;
                    Words tempword = new Words(word, Score);
                    

                }else
                {
                    reponseStatus = "Play Word Error: " + response.StatusCode + ", " + response.ReasonPhrase;
                    this.view.wordEvent(reponseStatus);
                }

            }
        }
        /// <summary>
        /// get game status method
        /// </summary>
        /// <param name="brief">parameter</param>
        /// <param name="domain">parameter</param>
        public void GameStatusAsync(bool brief,String domain)
        {
            //try
            //{
                using (HttpClient client = CreateClient(domain))
                {

                    

                    dynamic data = new ExpandoObject();
                    String url = "";
                    if (brief == false)
                    {
                        url = "games/" + this.model.GameID;
                    }
                    else
                    {
                        url = "games/" + this.model.GameID + "?Brief=yes";
                    }

                    HttpResponseMessage response = client.GetAsync("games/" + this.model.GameID, tokenSource.Token).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        dynamic gameResult = JsonConvert.DeserializeObject(result);
                        this.model.GameState = gameResult.GameState;
                        if (brief == false)
                        {
                            if (this.model.GameState == "active")
                            {
                                this.model.Board = gameResult.Board;
                                this.model.TimeLimit = gameResult.TimeLimit;
                                this.model.TimeLeft = gameResult.TimeLeft;
                                this.model.Player1.Nickname = gameResult.Player1.Nickname;
                                this.model.Player1.Score = gameResult.Player1.Score;
                                String secondName = gameResult.Player2.Nickname;
                                this.model.Player2 = new Player(secondName);
                                this.model.Player2.Score = gameResult.Player2.Score;


                            }
                            if (this.model.GameState == "completed")
                            {
                                this.model.Board = gameResult.Board;
                                this.model.TimeLimit = gameResult.TimeLimit;
                                this.model.TimeLeft = gameResult.TimeLeft;
                                this.model.Player1.Nickname = gameResult.Player1.Nickname;
                                this.model.Player1.Score = gameResult.Player1.Score;
                                this.model.Player1.WordsPlayed = gameResult.Player1.WordsPlayed;
                                this.model.Player2.Nickname = gameResult.Player2.Nickname;
                                this.model.Player2.Score = gameResult.Player2.Score;
                                this.model.Player2.WordsPlayed = gameResult.Player2.WordsPlayed;
                            }
                        }
                        else
                        {
                            this.model.TimeLeft = gameResult.TimeLeft;
                            this.model.Player1.Score = gameResult.Player1.Score;
                            this.model.Player2.Score = gameResult.Player2.Score;

                        }
                    }
                    else
                    {
                        reponseStatus = "Play Word Error: " + response.StatusCode + ", " + response.ReasonPhrase;
                        this.view.errorEvent(reponseStatus);
                        Thread.CurrentThread.Abort();
                }

                }

            


         
        }



}

    
    

   
}
