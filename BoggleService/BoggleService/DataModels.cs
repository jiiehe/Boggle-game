using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Boggle
{
    /// <summary>
    /// class for create user parameter
    /// </summary>
    public class UserInfo
    {
        public String Nickname { set; get; }
        /// <summary>
        /// add for identifing the user's gameID, as a link with userToken and gameID
        /// </summary>
        //public string GameID { set; get; }
    }
    /// <summary>
    /// class for create user return type
    /// </summary>
    public class RegReturn
    {
        public String UserToken { set; get; }
    }
    /// <summary>
    /// class used to indicate game attribute
    /// </summary>
    public class Game
    {
        public BoggleBoard boggleboard { set; get; }
        public string GameState { set; get; }
        public Player player1 { set; get; }
        public Player player2 { set; get; }
        public int TimeLimit { set; get; }
        public string GameID { set; get; }
        public int TimeLeft { set; get; }
        public DateTime startTime { set; get; }

    }
    /// <summary>
    /// class used to indicate player attribute
    /// </summary>
    public class Player
    {
        public String Nickname { set; get; }
        public string UserToken { set; get; }
        public int TimeLimit { set; get; }
        public int Score { set; get; }
        public string GameID { set; get; }
        // use to record the playword list 
        public List<WordsPlayed> words { set; get; }
    }
    /// <summary>
    /// class for joingame parameter
    /// </summary>
    public class JoinType
    {
        public String UserToken { set; get; }
        public int TimeLimit { set; get; }
    }
    /// <summary>
    /// class for join game return type
    /// </summary>
    public class joinReturn
    {
        public string GameID { set; get; }
    }

    /// <summary>
    /// class for word attribute
    /// </summary>
    public class WordsPlayed
    {
        public String Word { set; get; }
        public int Score { set; get; }
    }
    /// <summary>
    /// class for game status 
    /// </summary>
    [DataContract]
    public class playerForstatus4
    {
        [DataMember(EmitDefaultValue = false)]
        public String Nickname { set; get; }
        [DataMember]
        public int Score { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<WordsPlayed> WordsPlayed { set; get; }
    }
    /// <summary>
    /// class for game status return type
    /// </summary>
    [DataContract]
    public class statudReturn4
    {
        [DataMember(EmitDefaultValue = false)]
        public String GameState { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public String Board { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int?TimeLeft { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public playerForstatus4 Player1 { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public playerForstatus4 Player2 { set; get; }
    }
    /// <summary>
    /// for playWord method's parameter
    /// </summary>
    public class playGameInfo
    {
        public string UserToken { set; get; }
        public string Word { set; get; }
        //public List<string> Words { set; get; }
    }
    /// <summary>
    /// for playWord return
    /// </summary>
    public class playWordsReturn
    {
        public int Score { set; get; }
    }
    /// <summary>
    /// for canceljoinrequest method's parameter
    /// </summary>
    public class cancelJoinInfo
    {
        public string UserToken { set; get; }
    }
}