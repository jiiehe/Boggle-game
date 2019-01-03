using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoggleModel;
/// <summary>
/// Author By Lin Jia && Jin HE
/// </summary>
namespace BoggleClient
{
    public interface IBoggle
    {
        /// <summary>
        /// the event to create user pass in usertoken and domain
        /// </summary>
        event Action<String,String> creatUserEvent;
        /// <summary>
        /// leave game event
        /// </summary>
        event Action LeaveGameEvent;
        /// <summary>
        /// join game event need to pass in usertoken , time limit, domain
        /// </summary>
        event Action<String, int,String> joinGameEvent;
        /// <summary>
        /// user token event return usertoken
        /// </summary>
        event Func<String> UserTokenEvent;
        /// <summary>
        /// cancel game event need to pass in usertoken and domain
        /// </summary>
        event Action<String,String> cancelGameEvent;
        /// <summary>
        /// need bool brief and domain
        /// </summary>
        event Action<bool,String> CheckStatusEvent;
        /// <summary>
        /// when staus is active to get state
        /// </summary>
        /// <param name="board"> parameter</param>
        /// <param name="timeleft"> parameter</param>
        /// <param name="name"> parameter</param>
        /// <param name="score"> parameter</param>
        /// <param name="name2"> parameter</param>
        /// <param name="score2"> parameter</param>
        void paintActive(String board, int timeleft, String name, int score,String name2, int score2);
        /// <summary>
        /// reset the state
        /// </summary>
        /// <param name="timeleft"> parameter</param>
        /// <param name="score1"> parameter</param>
        /// <param name="score2"> parameter</param>
        void repaint(int timeleft,int score1, int score2);
        /// <summary>
        /// play event to handle playword method
        /// </summary>
        event Action<String,String> playEvent;
        /// <summary>
        /// wait player coming method used to show pending penal
        /// </summary>
        void waitPlayerComing();
        /// <summary>
        /// update the game state
        /// </summary>
        /// <param name="name"> parameter</param>
        /// <param name="score"> parameter</param>
        /// <param name="words"> parameter</param>
        /// <param name="name2"> parameter</param>
        /// <param name="Score2"> parameter</param>
        /// <param name="woreds2"> parameter</param>
        void getResult(String name,int score,dynamic words,String name2, int Score2, dynamic woreds2);
        /// <summary>
        /// method used to stop game
        /// </summary>
        void stopService();
        /// <summary>
        /// event used to handle exit the window method
        /// </summary>
        event Action ExitGameEvent;
        /// <summary>
        /// method used to exit the window
        /// </summary>
        void shutDownGame();
        /// <summary>
        /// help menu event to handle help menu content method return help menu content
        /// </summary>
        event Func<string> helpmenuEvent;
        /// <summary>
        /// help menu click event to handle help menu click method
        /// </summary>
        event Action helpMenuClickEvent;
        /// <summary>
        /// method click help menu
        /// </summary>
        void clickHelpMenu();
        /// <summary>
        /// show error message method 
        /// </summary>
        /// <param name="info"></param>
        void errorEvent(String info);
        /// <summary>
        /// event to show word list
        /// </summary>
        /// <param name="info"></param>
        void wordEvent(String info);
    }
}
