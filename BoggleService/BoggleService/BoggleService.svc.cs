using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class BoggleService : IBoggleService
    {
     
        
        /// <summary>
        /// user token is key and nick name is value
        /// </summary>
        private   readonly static  Dictionary<String, Player> users = new Dictionary<String, Player>();
        /// <summary>
        /// GameID is key, Game class is value
        /// </summary>
        private   readonly static Dictionary<String, Game> games = new Dictionary<String, Game>();
        private static readonly object sync = new object();
        
        /// <summary>
        /// The most recent call to SetStatus determines the response code used when
        /// an http response is sent.
        /// </summary>
        /// <param name="status">parameter</param>
        private static void SetStatus(HttpStatusCode status)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        /// <summary>
        /// Returns a Stream version of index.html.
        /// </summary>
        /// <returns></returns>
        public Stream API()
        {
            SetStatus(OK);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }
        /// <summary>
        /// method for creating user
        /// </summary>
        /// <param name="user">the user provided by client. </param>
        /// <returns>return a userToken for a user. </returns>
        public RegReturn Register(UserInfo user)
        {
            lock (sync)
            {
               
              
                if (user==null||user.Nickname == null || user.Nickname.Trim().Length == 0)
                {
                    SetStatus(Forbidden);
                    return null;
                }
                if (user.Nickname.StartsWith("@"))
                {
                    Thread.Sleep(10000);
                }
               
               
                string userID = Guid.NewGuid().ToString();                  
                while (users.ContainsKey(userID))
                {
                    userID = Guid.NewGuid().ToString();
                }
                Player player = new Player();
                player.UserToken = userID;
                player.Nickname = user.Nickname;
                users.Add(userID, player);
                RegReturn info = new RegReturn();
                info.UserToken = userID;
                SetStatus(Created);
                return info;
                
            }
        }
        /// <summary>
        /// method for joining game
        /// </summary>
        /// <param name="user">the user which is joining the game, contains a UserToken and a TimeLimit. </param>
        /// <returns>return a GameID. </returns>
        public joinReturn JoinGame(JoinType user)
        {
            lock (sync)
            {
                if (!users.ContainsKey(user.UserToken) || user.TimeLimit < 5 || user.TimeLimit > 120)
                {
                    SetStatus(Forbidden);
                    return null;
                } else
                {
                          
                    if (games.Count == 0)
                    {
                        Game firstGame = new Game();
                        firstGame.GameID = (games.Count + 1) + "";
                        firstGame.GameState = "pending";
                        games[(games.Count + 1) + ""] = firstGame;

                    }
                    Game game = games[(games.Count) + ""];
                    if (game.player1 != null)
                    {
                        if (game.player1.UserToken == user.UserToken)
                        {
                            SetStatus(Conflict);
                            return null;
                        }
                        game.GameState = "active";
                        Player player2 = new Player();
                        player2.UserToken = user.UserToken;
                        player2.TimeLimit = user.TimeLimit;
                        player2.GameID = game.GameID;
                        player2.Nickname = users[user.UserToken].Nickname;
                        game.player2 = player2;
                        game.player1.words = new List<WordsPlayed>();
                        game.player2.words = new List<WordsPlayed>();
                        int avgTimeLimit = (game.player1.TimeLimit + game.player2.TimeLimit) / 2;
                        game.TimeLimit = avgTimeLimit;
                        string gameID = games[games.Count + ""].GameID;
                        Game newPendingGame = new Game();
                        newPendingGame.GameState = "pending";
                        newPendingGame.GameID = (games.Count + 1)+"";
                        games[(games.Count + 1) + ""] = newPendingGame;
                        game.boggleboard = new BoggleBoard();
                        game.startTime = DateTime.Now;
                        users[game.player2.UserToken] = player2;
                        joinReturn info = new joinReturn();
                        info.GameID = gameID;
                        SetStatus(Created);
                        return info;
                    }
                    else
                    {
                        Player player1 = new Player();
                        player1.UserToken = user.UserToken;
                        player1.TimeLimit = user.TimeLimit;
                        player1.Nickname = users[user.UserToken].Nickname;
                        game.TimeLimit = player1.TimeLimit;
                        player1.GameID = game.GameID;
                        game.player1 = player1;
                        users[game.player1.UserToken] = player1;
                        joinReturn info = new joinReturn();
                        info.GameID = game.GameID;                       
                        SetStatus(Accepted);
                        return info;
                    }                                          
                }
            }
        }
        /// <summary>
        /// method for play word
        /// </summary>
        /// <param name="user">the UserToek and the word that this player typied. </param>
        /// <param name="GameID">the GameID of this player in . </param>
        /// <returnsplayWordsReturn>return the score of the word that the user type. </returns>
        public playWordsReturn PlayWords(playGameInfo user,String GameID)
        {
            lock (sync)
            {
              
                int number;
                bool result = Int32.TryParse(GameID, out number);

                if (user==null||user.Word == null || user.Word.Trim().Length == 0 || GameID == null ||
                    user.UserToken == null  || !result||games.ContainsKey(GameID)==false)
                {
                    SetStatus(Forbidden);
                    return null;
                }
              
                Game game = games[GameID];
                gameStatus(GameID, "");
                //when the GameState is not active
                if (game.GameState != "active")
                {
                    SetStatus(Conflict);
                    return null;
                }
                if (game.player1 == null || game.player2 == null || (game.player1.UserToken != user.UserToken && game.player2.UserToken != user.UserToken))
                {
                    SetStatus(Forbidden);
                    return null;
                }
                //UserToken in the game identified by GameID.
                else
                {
                    int scoreResult = 0;
                    if (game.boggleboard.CanBeFormed(user.Word) && File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory.ToString() + "dictionary.txt").Contains(user.Word))
                    {
                        scoreResult = CalculateScore(user.Word);
                    }
                    else
                    {
                        scoreResult--;

                    }
                    if (game.player1.UserToken == user.UserToken)
                    {                       
                        WordsPlayed word = new WordsPlayed();
                        word.Word = user.Word;
                        word.Score = scoreResult;
                        helpAdd(game.player1, word);

                    }
                    else
                    {
                        WordsPlayed word = new WordsPlayed();
                        word.Word = user.Word;
                        word.Score = scoreResult;
                        helpAdd(game.player2, word);
                    }


                    //user.Words.Add(user.Word);
                    playWordsReturn info = new playWordsReturn();
                    info.Score = scoreResult;
                    SetStatus(OK);
                    return info;
                }
            }       
        }
        /// <summary>
        /// help method to add valid word to the recorded word list
        /// </summary>
        /// <param name="player">the player of the current game. </param>
        /// <param name="word">the word that typied by the user. </param>
        public void helpAdd(Player player, WordsPlayed word)
        {
            bool realAdd= true;
            foreach(WordsPlayed temp in player.words)
            {
                if (temp.Word == word.Word)
                {
                    realAdd = false;
                }
            }
            if (realAdd == true)
            {
                player.words.Add(word);
                player.Score += word.Score;
            }
        }


        /// <summary>
        /// help method to calculate the score player get
        /// </summary>
        /// <param name="word">the word that typied by the user. </param>
        /// <returns>return the score of this word. </returns>
        public int CalculateScore(string word)
        {
            int scoreResult = 0;
            if(word.Length < 3)
            {
                scoreResult = 0;
            }else if(word.Length > 7)
            {
                scoreResult += 7;
            }else if(word.Length == 7)
            {
                scoreResult += 5;
            }else if(word.Length == 6)
            {
                scoreResult += 3;
            }else if(word.Length == 5)
            {
                scoreResult += 2;
            }else
            {
                scoreResult += 1;
            }
            return scoreResult;
        }


        ///// <summary>
        ///// Demo.  You can delete this.
        ///// </summary>
        //public string WordAtIndex(int n)
        //{
        //    if (n < 0)
        //    {
        //        SetStatus(Forbidden);
        //        return null;
        //    }

        //    string line;
        //    using (StreamReader file = new System.IO.StreamReader(AppDomain.CurrentDomain.BaseDirectory + "dictionary.txt"))
        //    {
        //        while ((line = file.ReadLine()) != null)
        //        {
        //            if (n == 0) break;
        //            n--;
        //        }
        //    }

        //    if (n == 0)
        //    {
        //        SetStatus(OK);
        //        return line;
        //    }
        //    else
        //    {
        //        SetStatus(Forbidden);
        //        return null;
        //    }
        //}

        /// <summary>
        /// this is a helper method to return a short version of GameStatus. 
        /// </summary>
        /// <param name="GameID">the GameID that the player in. </param>
        /// <returns>return information of client need.</returns>
        public statudReturn4 helpBrief(String GameID)
        {
           
                Game game = games[GameID];
                statudReturn4 info = new statudReturn4();
                info.GameState = game.GameState;
                info.TimeLeft = game.TimeLeft;
                info.Player1 = new playerForstatus4();
                info.Player1.Score = game.player1.Score;
                info.Player2 = new playerForstatus4();
                info.Player2.Score = game.player2.Score;
                return info;
            


        }
        /// <summary>
        /// used to check gameStatus. 
        /// </summary>
        /// <param name="GameID">the game ID that the client want to check. </param>
        /// <param name="Brief">whether the user want just get a brief information </param>
        /// <returns></returns>
        public statudReturn4 gameStatus(String GameID, String Brief)
        {
            lock (sync)
            {
                if (games.ContainsKey(GameID) == false)
                {
                    SetStatus(Forbidden);
                    return null;
                }
                else
                {
                    Game game = games[GameID];

                    if (game.GameState == "pending")
                    {
                        SetStatus(OK);
                        statudReturn4 info = new statudReturn4()
                        {
                            GameState = "pending"
                            
                         };
                        return info;

                    }
                    else if (game.GameState == "active")
                    {
                        double trans = Convert.ToDouble((DateTime.Now.Subtract(game.startTime)).TotalSeconds);

                        int getTime = game.TimeLimit - (Int32)trans;
                        if (getTime > 0)
                        {
                            game.TimeLeft = getTime;
                        }
                        else
                        {
                            game.TimeLeft = 0;
                            game.GameState = "completed";
                            return gameStatus(GameID, Brief);

                        }
                        if (Brief == "yes")
                        {
                            statudReturn4 info = helpBrief(GameID);
                            SetStatus(OK);
                            return info;
                        }
                        else
                        {
                            statudReturn4 info = new statudReturn4();
                            info.GameState = game.GameState;
                            info.Board = game.boggleboard.ToString();
                            info.TimeLimit = game.TimeLimit;
                            info.TimeLeft = game.TimeLeft;
                            info.Player1 = new playerForstatus4();
                            info.Player1.Score = game.player1.Score;
                            info.Player1.Nickname = game.player1.Nickname;
                            info.Player2 = new playerForstatus4();
                            info.Player2.Nickname = game.player2.Nickname;
                            info.Player2.Score = game.player2.Score;
                            SetStatus(OK);
                            return info;
                        }
                    }
                    else
                    {
                        if (Brief == "yes")
                        {
                            statudReturn4 info2 = helpBrief(GameID);
                            SetStatus(OK);
                            return info2;
                        }
                        else
                        {

                            statudReturn4 info = new statudReturn4();
                            info.GameState = game.GameState;
                            info.Board = game.boggleboard.ToString();
                            info.TimeLimit = game.TimeLimit;
                            info.TimeLeft = 0;
                            info.Player1 = new playerForstatus4();
                            info.Player1.Score = game.player1.Score;
                            info.Player1.Nickname = game.player1.Nickname;
                            info.Player1.WordsPlayed = new List<WordsPlayed>(game.player1.words);
                            info.Player2 = new playerForstatus4();
                            info.Player2.Nickname = game.player2.Nickname;
                            info.Player2.Score = game.player2.Score;
                            info.Player2.WordsPlayed = new List<WordsPlayed>(game.player2.words);
                            SetStatus(OK);
                            return info;
                        }
                    }
                    
                }
            }  
        }
        /// <summary>
        /// help method to check whether the generated guid user token is valid or not 
        /// need to change register method when chack the guid token valid---------------??????
        /// </summary>
        /// <param name="guid">parameter</param>
        /// <returns></returns>
        public bool checkUserTokenValid(string guid)
        {
            Guid output;
            bool isValid = Guid.TryParse(guid, out output);
            if (isValid)
            {
                return true;
            }else
            {
                return false;
            }
        }
        /// <summary>
        /// cancel join request method
        /// </summary>
        /// <param name="user">parameter</param>
        public void CancelJoinRequest(RegReturn user)
        {
            lock (sync)
            {
                if (users.ContainsKey(user.UserToken) == false)
                {
                    SetStatus(Forbidden);

                }
                else
                {
                    Player player = users[user.UserToken];
                    String GameID = player.GameID;
                    if (GameID == null || games.ContainsKey(GameID) == false)
                    {
                        SetStatus(Forbidden);
                    }
                    else
                    {
                        Game game = games[GameID];
                        if (game.GameState != "pending")
                        {
                            SetStatus(Forbidden);
                        }
                        else
                        {                          
                            game.player1 = null;
                            UserInfo info = new UserInfo();
                            info.Nickname = "";
                        }
                    }
                }
            }    
                
                
            
        }
    }
}
