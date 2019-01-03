using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Author By Lin Jia&& Jin HE
/// </summary>
namespace BoggleModel
{
    /// <summary>
    /// this is used to represent two players. 
    /// </summary>
    public class Player
    {
        public  String Nickname;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Nickname">parameter</param>
        public Player(String Nickname)
        {
            this.Nickname = Nickname;
        }
        public  String UserToken
        {
            get;set;
        }
        public  int Score
        {
            get;set;
        }
        public  dynamic WordsPlayed
        {
            set;get;
        }

    }
    /// <summary>
    /// words class used to represent word string and score
    /// </summary>
    public class Words
    {
        private String Word;
        private int Score;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Word">parameter</param>
        /// <param name="Score">parameter</param>
        public Words(String Word, int Score)
        {
            this.Word = Word;
            this.Score = Score;
        }
    }
    /// <summary>
    /// class model inclues features need
    /// </summary>
    public class Model
    {
        public Player Player1;
        public Player Player2;
        /// <summary>
        /// model constructor
        /// </summary>
        /// <param name="Player1">parameter</param>
        public Model(Player Player1)
        {
            this.Player1 = Player1;           
        }
        public  int GameID
        {
            set;get;
        }
        public  String Word
        {
            get;set;
        }
        public String Board
        {
            get;set;
        }
        public  int TimeLimit
        {
            set;get;
        }
        public  int TimeLeft
        {
            set; get;
        }
        public  String GameState
        {
            get;set;
        }

    }
}
