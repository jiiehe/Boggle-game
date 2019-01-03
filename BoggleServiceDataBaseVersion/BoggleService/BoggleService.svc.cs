using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using static System.Net.HttpStatusCode;
/// <summary>
/// Boggle game using DB
/// Author JIN HE & LIN JIA
/// </summary>
namespace Boggle
{
    /// <summary>
    /// Boggle Service class
    /// </summary>
    public class BoggleService : IBoggleService
    {
        /// <summary>
        /// declare static boggle database
        /// </summary>
        private static string BoggleDB;
        /// <summary>
        /// constructor
        /// </summary>
        static BoggleService()
        {

            BoggleDB = ConfigurationManager.ConnectionStrings["BoggleDB"].ConnectionString;
        }

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
            //check user, user name 
            if (user == null || user.Nickname == null || user.Nickname.Trim().Length == 0)
            {
                SetStatus(Forbidden);
                return null;
            }
            // check username with "@" start
            if (user.Nickname.StartsWith("@"))
            {
                Thread.Sleep(5000);
            }
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                // Connections must be opened
                conn.Open();

                // Database commands should be executed within a transaction.  When commands 
                // are executed within a transaction, either all of the commands will succeed
                // or all will be canceled.  You don't have to worry about some of the commands
                // changing the DB and others failing.
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    // An SqlCommand executes a SQL statement on the database.  In this case it is an
                    // insert statement.  The first parameter is the statement, the second is the
                    // connection, and the third is the transaction.  

                    using (SqlCommand command =
                        new SqlCommand(getPath("registerInsert"),
                                        conn,
                                        trans))
                    {
                        string userID = Guid.NewGuid().ToString();

                        // This is where the placeholders are replaced.
                        command.Parameters.AddWithValue("@UserID", userID);
                        command.Parameters.AddWithValue("@Nickname", user.Nickname);
                        // This executes the command within the transaction over the connection.  The number of rows
                        // that were modified is returned.  Perhaps I should check and make sure that 1 is returned
                        // as expected.
                        command.ExecuteNonQuery();
                        // Immediately before each return that appears within the scope of a transaction, it is
                        // important to commit the transaction.  Otherwise, the transaction will be aborted and
                        // rolled back as soon as control leaves the scope of the transaction. 
                        trans.Commit();
                        // return info
                        RegReturn info = new RegReturn();
                        info.UserToken = userID;
                        SetStatus(Created);
                        return info;

                    }
                }
            }

        }
        /// <summary>
        /// this class is for easily changing sql requirements  
        /// </summary>
        /// <param name="requirement">SQL command </param>
        /// <returns>return the command that we need. </returns>
        public String getPath(String requirement)
        {
            // check whether the userId is in Users table and get data
            if (requirement == "check")
            {
                return "select UserID from Users where UserID = @UserID";
            }
            // select all data when the game only has player1
            else if (requirement == "checkStatus")
            {
                return "select * From Games where Player2 is Null And Player1 is not NUll";
            }
            // insert player1 and timelimit into Games table
            else if (requirement == "joinAdd1")
            {
                return "insert into Games (Player1,TimeLimit) output inserted.GameID values(@Player1,@TimeLimit)";
            }
            // when the game need to add player2, insert player2, timelimit, board, startTime
            else if (requirement == "joinAdd2")
            {
                return "update Games set Player2=@Player2, TimeLimit=@TimeLimit,Board=@Board, StartTime=@StartTime output inserted.GameID where GameID=@GameID";
            }
            // check whether the userId is in Games table and get data
            else if (requirement == "gameStatus")
            {
                return "select * From Games where GameID=@GameID";
            }
            // From Word table, we selected Word and Score when GameID and Player existed
            else if (requirement == "wordsList")
            {
                return "select Word,Score From Words where GameID=@GameID And Player=@Player";
            }
            // We get all data from Games table when GameID existed
            else if (requirement == "getGameState")
            {
                return "select * From Games where GameID=@GameID";
            }
            // when GameID, Usertoken and Word are existed, we get all data form Words table
            else if (requirement == "checkWord")
            {
                return "select * from Words where GameID=@GameID And Player=@Player And Word=@Word";
            }
            // the instruction is used to add new data into Words table
            else if (requirement == "updatePlayWords")
            {
                return "insert into Words (Word,GameID,Player,Score) output inserted.Score values(@Word,@GameID, @Player,@Score)";

            }
            // used to cancel game 
            else if (requirement == "updateCancelGame")
            {
                return "delete From Games where GameID=@GameID";

            }
            // used to check player's usertoken
            else if (requirement == "cancelCheck2")
            {
                return "select * From Games where Player1=@Player";
            }
            // get all data from Games tale when userID exist
            else if(requirement == "helpBriefCheck")
            {
                return "select * From Games where GameID=@GameID";
            }
            // get all data form Games table when we find gameID and player
            else if(requirement == "getCheckMatch")
            {
                return "select * from Games where GameID=@GameID And (Player1=@Player OR Player2=@Player)";
            }
            // get nickname when userToken exist
            else if(requirement == "getNickName"){
                return "select Nickname From Users where UserID=@UserID";
            }
            // get total score when player and gameID exist
            else if(requirement == "getSumScore")
            {
                return "select Sum(Score) as point from Words where Player=@Player And GameID =@GameID";
            }
            // insert userToken and nickName into USers table 
            else if(requirement == "registerInsert")
            {
                return "insert into Users (UserID, Nickname) values(@UserID, @Nickname)";
            }
            return null;
        }



        /// <summary>
        /// method for joining game
        /// </summary>
        /// <param name="user">the user which is joining the game, contains a UserToken and a TimeLimit. </param>
        /// <returns>return a GameID. </returns>
        public joinReturn JoinGame(JoinType user)
        {
         
            if (user.TimeLimit < 5 || user.TimeLimit > 120)
            {
                SetStatus(Forbidden);
                return null;
            }
            // need UserToken, GameID, TimeLimit, Player1. 
            else
            {
                using (SqlConnection conn = new SqlConnection(BoggleDB))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {

                        // Here, the SqlCommand is a select query.  We are interested in whether item.UserID exists in
                        // the Users table.
                        using (SqlCommand command = new SqlCommand(getPath("check"), conn, trans))
                        {
                            command.Parameters.AddWithValue("@UserID", user.UserToken);

                            // This executes a query (i.e. a select statement).  The result is an
                            // SqlDataReader that you can use to iterate through the rows in the response.
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    SetStatus(Forbidden);
                                    reader.Close();
                                    return null;
                                }
                            }
                        }
                        // select all data when the game only has player1
                        using (SqlCommand command = new SqlCommand(getPath("checkStatus"), conn, trans))
                        {

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                // if the table does not have player1
                                if (reader.HasRows == false)
                                {
                                    reader.Close();
                                    // insert player1 and timelimit into Games table
                                    using (SqlCommand cmd = new SqlCommand(getPath("joinAdd1"), conn, trans))
                                    {
                                        cmd.Parameters.AddWithValue("@Player1", user.UserToken);
                                        cmd.Parameters.AddWithValue("@TimeLimit", user.TimeLimit);
                                        // set the return info
                                        joinReturn info = new joinReturn();
                                        info.GameID = cmd.ExecuteScalar().ToString();
                                        SetStatus(Accepted);
                                        trans.Commit();
                                        return info;
                                    }
                                }
                                // if the table has player1
                                else
                                {
                                    int TimeLimit = 0;
                                    int GameID = 0;
                                    // read to get player1's information
                                    reader.Read();
                                    Int32.TryParse(reader["GameID"].ToString(), out GameID);
                                    Int32.TryParse(reader["TimeLimit"].ToString(), out TimeLimit);
                                    String checkToken = reader["Player1"].ToString();
                                    // check user token valid or not
                                    if (checkToken == user.UserToken)
                                    {
                                        SetStatus(Conflict);
                                        return null;
                                    }
                                    reader.Close();
                                    // when the game need to add player2, insert player2, timelimit, board, startTime
                                    using (SqlCommand cmd = new SqlCommand(getPath("joinAdd2"), conn, trans))
                                    {

                                        int timeLimit = (TimeLimit + user.TimeLimit) / 2;
                                        cmd.Parameters.AddWithValue("@GameID", GameID);
                                        cmd.Parameters.AddWithValue("@Player2", user.UserToken);
                                        cmd.Parameters.AddWithValue("@TimeLimit", timeLimit);
                                        cmd.Parameters.AddWithValue("@Board", new BoggleBoard().ToString());
                                        cmd.Parameters.AddWithValue("@StartTime", DateTime.Now);
                                        // We execute the command with the ExecuteScalar method, which will return to
                                        // us the requested auto-generated ItemID.
                                        // return info
                                        joinReturn info = new joinReturn();
                                        info.GameID = cmd.ExecuteScalar().ToString();
                                        SetStatus(Created);
                                        trans.Commit();
                                        return info;
                                    }
                                }
                            }
                        }
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
        public playWordsReturn PlayWords(playGameInfo user, String GameID)
        {
            //get gameID
            int number;
            bool result = Int32.TryParse(GameID, out number);
            //check user info
            if (user == null || user.Word == null || user.Word.Trim().Length == 0 || GameID == null ||
                user.UserToken == null || !result)
            {
                SetStatus(Forbidden);
                return null;
            }
            //check game status
            statudReturn4 check = gameStatus(GameID, "");
            if (check.GameState != "active")
            {
                SetStatus(Conflict);
                return null;
            }
            //
            if (checkmatching(GameID, user.UserToken) == false)
            {
                SetStatus(Forbidden);
                return null;
            }
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    BoggleBoard NewGame = new BoggleBoard(check.Board);
                    int scoreResult = 0;
                    if (user.Word.Length < 3)
                    {
                        scoreResult = 0;
                    }
                    else
                    if (NewGame.CanBeFormed(user.Word) && File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory.ToString() + "dictionary.txt").Contains(user.Word))
                    {
                        scoreResult = CalculateScore(user.Word);
                    }
                    else
                    {
                        scoreResult--;

                    }
                    if (checkWordPlay(GameID, user.UserToken, user.Word) == false)
                    {
                        using (SqlCommand cmd = new SqlCommand(getPath("updatePlayWords"), conn, trans))
                        {

                            cmd.Parameters.AddWithValue("@Word", user.Word);
                            cmd.Parameters.AddWithValue("@Score", scoreResult);
                            cmd.Parameters.AddWithValue("@GameID", GameID);
                            cmd.Parameters.AddWithValue("@Player", user.UserToken);
                            cmd.ExecuteNonQuery();
                            trans.Commit();
                        }
                    }
                    else
                    {
                        scoreResult = 0;
                    }
                    playWordsReturn info = new playWordsReturn();
                    info.Score = scoreResult;
                    SetStatus(OK);
                    return info;
                }
            }
        }
        /// <summary>
        /// check whether the player have played the same word in this current game. 
        /// </summary>
        /// <param name="GameID">this game's GameID</param>
        /// <param name="Usertoken">the player's Usertoken</param>
        /// <param name="word">the word that the player played. </param>
        /// <returns>return false if the word did not be played by this player in this game. </returns>
        private bool checkWordPlay(string GameID, string Usertoken, string word)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    // Here, the SqlCommand is a select query.  We are interested in whether item.UserID exists in
                    // the Users table.
                    using (SqlCommand command = new SqlCommand(getPath("checkWord"), conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", GameID);
                        command.Parameters.AddWithValue("@Player", Usertoken);
                        command.Parameters.AddWithValue("@Word", word);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                reader.Close();
                                return false;
                            }
                            else
                            {
                                reader.Close();
                                return true;
                            }

                        }
                    }
                }
            }
        }
        /// <summary>
        /// check whether gameID and userToken existed in Games table
        /// </summary>
        /// <param name="GameID">gameID need to check whether it exist in Games or not</param>
        /// <param name="UserToken">UserToken need to check whether it exist in Games or not</param>
        /// <returns>return false if the player is not the player in the game. </returns>
        public bool checkmatching(String GameID, String UserToken)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    // Here, the SqlCommand is a select query.  We are interested in whether item.UserID exists in
                    // the Users table.
                    using (SqlCommand command = new SqlCommand(getPath("getCheckMatch"), conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", GameID);
                        command.Parameters.AddWithValue("@Player", UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows == false)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
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
            if (word.Length < 3)
            {
                scoreResult = 0;
            }
            else if (word.Length > 7)
            {
                scoreResult += 11;
            }
            else if (word.Length == 7)
            {
                scoreResult += 5;
            }
            else if (word.Length == 6)
            {
                scoreResult += 3;
            }
            else if (word.Length == 5)
            {
                scoreResult += 2;
            }
            else
            {
                scoreResult += 1;
            }
            return scoreResult;
        }
        /// <summary>
        /// this is a helper method to return a short version of GameStatus. 
        /// </summary>
        /// <param name="GameID">the GameID that the player in. </param>
        /// <returns>return information of client need.</returns>
        public statudReturn4 helpBrief(String GameID, int timeLeft, String GameState)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    // Here, the SqlCommand is a select query.  We are interested in whether item.UserID exists in
                    // the Users table.
                    using (SqlCommand command = new SqlCommand(getPath("helpBriefCheck"), conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", GameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                            statudReturn4 info = new statudReturn4();
                            info.GameState = GameState;
                            info.TimeLeft = timeLeft;
                            info.Player1 = new playerForstatus4();
                            info.Player1.Score = getPoint(reader["Player1"].ToString(),GameID);
                            info.Player2 = new playerForstatus4();
                            info.Player2.Score = getPoint(reader["Player2"].ToString(),GameID);
                            SetStatus(OK);
                            return info;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// help method used to get the nick name form Users table
        /// </summary>
        /// <param name="userToken">usertoken for parameter</param>
        /// <returns>get the player's name </returns>
        public String getName(String userToken)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    // Here, the SqlCommand is a select query.  We are interested in whether item.UserID exists in
                    // the Users table.
                    // And get the username
                    using (SqlCommand command = new SqlCommand(getPath("getNickName"), conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", userToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                            String name = reader["Nickname"].ToString();
                            return name;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// help method used to get the total score in Word table
        /// </summary>
        /// <param name="userToken">the player's usertoken. </param>
        /// <param name="GameID">the Game ID</param>
        /// <returns>return the points that the player get in this game. </returns>
        public int getPoint(String userToken,String GameID)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    // Here, the SqlCommand is a select query to get sum, when player and gameid are existed
                    using (SqlCommand command = new SqlCommand(getPath("getSumScore"), conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player", userToken);
                        command.Parameters.AddWithValue("@GameID",GameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                            int result = 0;
                            String point = reader["point"].ToString();
                            Int32.TryParse(point, out result);
                            return result;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// help method used to get word and score from Words table
        /// </summary>
        /// <param name="GameID">gameid need to check whether exist or not</param>
        /// <param name="UserToken">usertoken need to check whether exist or not</param>
        /// <returns>return the list of words that the player played in this game. </returns>
        public List<WordsPlayed> getWords(String GameID, String UserToken)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    // From Word table, we selected Word and Score when GameID and Player existed
                    using (SqlCommand command = new SqlCommand(getPath("wordsList"), conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player", UserToken);
                        command.Parameters.AddWithValue("@GameID", GameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<WordsPlayed> words = new List<WordsPlayed>();
                            while (reader.Read() != false)
                            {
                                // update the word and score
                                WordsPlayed word = new WordsPlayed();
                                word.Word = reader["Word"].ToString();
                                word.Score = (Int32)reader["Score"];
                                words.Add(word);
                            }
                            return words;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// used to check gameStatus. 
        /// </summary>
        /// <param name="GameID">the game ID that the client want to check. </param>
        /// <param name="Brief">whether the user want just get a brief information </param>
        /// <returns>return the game status information. </returns>
        public statudReturn4 gameStatus(String GameID, String Brief)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    // Here, the SqlCommand is a select query.  We are interested in whether item.UserID exists in
                    // the Users table.
                    using (SqlCommand command = new SqlCommand(getPath("gameStatus"), conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", GameID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                SetStatus(Forbidden);

                                reader.Close();
                                return null;
                            }
                            else
                            {
                                reader.Read();

                                if (reader["Player2"] is DBNull)
                                {
                                    SetStatus(OK);
                                    statudReturn4 info = new statudReturn4()
                                    {
                                        GameState = "pending"
                                    };
                                    return info;
                                }
                                else
                                {
                                    String board = reader["Board"].ToString();
                                    int timeLimit = (Int32)reader["TimeLimit"];
                                    DateTime startTime = (DateTime)reader["StartTime"];
                                    double tran = Convert.ToDouble((DateTime.Now.Subtract(startTime)).TotalSeconds);
                                    int getTime = timeLimit - (Int32)tran;
                                    //maybe finish the part for Active. 
                                    if (getTime > 0)
                                    {
                                        if (Brief == "yes")
                                        {
                                            return helpBrief(GameID, getTime, "active");
                                        }
                                        statudReturn4 info = new statudReturn4();
                                        info.GameState = "active";
                                        info.Board = board;
                                        info.TimeLimit = timeLimit;
                                        info.TimeLeft = getTime;
                                        info.Player1 = new playerForstatus4();
                                        info.Player1.Nickname = getName(reader["Player1"].ToString());
                                        info.Player1.Score = getPoint(reader["Player1"].ToString(),GameID);
                                        info.Player2 = new playerForstatus4();
                                        info.Player2.Nickname = getName(reader["Player2"].ToString());
                                        info.Player2.Score = getPoint(reader["Player2"].ToString(),GameID);
                                        SetStatus(OK);
                                        return info;
                                    }
                                    else
                                    {
                                        if (Brief == "yes")
                                        {
                                            return helpBrief(GameID, 0, "completed");
                                        }
                                        statudReturn4 info = new statudReturn4();
                                        info.GameState = "completed";
                                        info.Board = board;
                                        info.TimeLimit = timeLimit;
                                        info.TimeLeft = 0;
                                        info.Player1 = new playerForstatus4();
                                        info.Player1.Nickname = getName(reader["Player1"].ToString());
                                        info.Player1.Score = getPoint(reader["Player1"].ToString(),GameID);
                                        info.Player1.WordsPlayed = getWords(GameID, reader["Player1"].ToString());
                                        info.Player2 = new playerForstatus4();
                                        info.Player2.Nickname = getName(reader["Player2"].ToString());
                                        info.Player2.Score = getPoint(reader["Player2"].ToString(),GameID);
                                        info.Player2.WordsPlayed = getWords(GameID, reader["Player2"].ToString());
                                        SetStatus(OK);
                                        return info;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// help method to check whether the generated guid user token is valid or not 
        /// need to change register method when chack the guid token valid------------
        /// </summary>
        /// <param name="guid">parameter</param>
        /// <returns>return true if the usertoken is valid. </returns>
        public bool checkUserTokenValid(string guid)
        {
            Guid output;
            bool isValid = Guid.TryParse(guid, out output);
            if (isValid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// cancel join request method
        /// </summary>
        /// <param name="user">parameter</param>
        public void CancelJoinRequest(cancelJoinInfo user)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    // Here, the SqlCommand is a select query.  We are interested in whether item.UserID exists in
                    // the Users table.
                    using (SqlCommand command = new SqlCommand(getPath("check"), conn, trans))
                    {
                        command.Parameters.AddWithValue("UserID", user.UserToken);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // do not has corresponding userToken
                            if (!reader.HasRows)
                            {
                                SetStatus(Forbidden);
                                reader.Close();
                            }
                            //check whether the usertoken is player1's usertoken
                            //the line is from Users Table instead of Games
                            //need to check whether we can get the Game first from the Games Table, if Games doesn't contains the UserToken, set Forbidden, 
                            //I think we can just get the Game Status by make sure whether the Player2 is null, it's pending if it is, otherwise it's Forbidden. 
                            else
                            {
                                reader.Close();
                                using (SqlCommand cmd = new SqlCommand(getPath("cancelCheck2"), conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@Player", user.UserToken);
                                    using (SqlDataReader reader1 = cmd.ExecuteReader())
                                    {
                                        // do not has corresponding userToken
                                        if (!reader1.HasRows)
                                        {
                                            SetStatus(Forbidden);
                                        }
                                        else
                                        {
                                            reader1.Read();
                                            if (reader1["Player2"] is DBNull)
                                            {
                                                using (SqlCommand cmd1 = new SqlCommand(getPath("updateCancelGame"), conn, trans))
                                                {
                                                    //-----------------------need also to update
                                                    cmd1.Parameters.AddWithValue("@GameID", (Int32)reader1["GameID"]);
                                                    reader1.Close();
                                                    cmd1.ExecuteNonQuery();
                                                    trans.Commit();
                                                    SetStatus(OK);
                                                }
                                            }
                                            else
                                            {
                                                SetStatus(Forbidden);
                                            }
                                            reader1.Close();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
