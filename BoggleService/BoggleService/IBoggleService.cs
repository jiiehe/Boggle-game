﻿using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Boggle
{
    /// <summary>
    /// interface for boggle service
    /// </summary>
    [ServiceContract]
    public interface IBoggleService
    {
        /// <summary>
        /// Sends back index.html as the response body.
        /// </summary>
        [WebGet(UriTemplate = "/api")]
        Stream API();

        ///// <summary>
        ///// Returns the nth word from dictionary.txt.  If there is
        ///// no nth word, responds with code 403. This is a demo;
        ///// you can delete it.
        ///// </summary>
        //[WebGet(UriTemplate = "/word?index={n}")]
        //string WordAtIndex(int n);
        /// <summary>
        /// Registers a new user.
        /// If either user.Name or user.Email is null or is empty after trimming, responds with status code Forbidden.
        /// Otherwise, creates a user, returns the user's token, and responds with status code Created. 
        /// </summary>
        [WebInvoke(Method = "POST", UriTemplate = "/users")]
        RegReturn Register(UserInfo user);

        /// <summary>
        ///If UserToken is invalid, TimeLimit < 5, or TimeLimit > 120, responds with status 403 (Forbidden).
        ///Otherwise, if UserToken is already a player in the pending game, responds with status 409 (Conflict).
        ///Otherwise, if there is already one player in the pending game, adds UserToken as the second player.
        ///The pending game becomes active and a new pending game with no players is created.The active game's time limit is the integer average of the time limits requested by the two players.
        ///Returns the new active game's GameID(which should be the same as the old pending game's GameID). Responds with status 201 (Created).
        ///Otherwise, adds UserToken as the first player of the pending game, and the TimeLimit as the pending game's requested time limit. Returns the pending game's GameID. Responds with status 202 (Accepted).
        /// </summary>
        [WebInvoke(Method = "POST", UriTemplate = "/games")]
        joinReturn JoinGame(JoinType user);

        /// <summary>
        ///Play a word in a game.
        ///1. If Word is null or empty when trimmed, or if GameID or UserToken is missing or invalid,
        ///or if UserToken is not a player in the game identified by GameID, responds with response code 403 (Forbidden).
        ///2. Otherwise, if the game state is anything other than "active", responds with response code 409 (Conflict).
        ///3. Otherwise, records the trimmed Word as being played by UserToken in the game identified by GameID.
        ///Returns the score for Word in the context of the game(e.g. if Word has been played before the score is zero). 
        ///Responds with status 200 (OK). Note: The word is not case sensitive.
        /// </summary>
        [WebInvoke(Method = "PUT", UriTemplate = "/games/{GameID}")]
        playWordsReturn PlayWords(playGameInfo user,string GameID);

        /// <summary>
        ///If UserToken is invalid or is not a player in the pending game, responds with status 403 (Forbidden).
        ///Otherwise, removes UserToken from the pending game and responds with status 200 (OK).
        /// </summary>
        [WebInvoke(Method = "PUT", UriTemplate = "/games")]
        void CancelJoinRequest (RegReturn user);
        /// <summary>
        /// If GameID is invalid, responds with status 403 (Forbidden).
        ///Otherwise, returns information about the game named by GameID as illustrated below. 
        ///Note that the information returned depends on whether "Brief=yes" was included as a parameter as well as on the state of the game. Responds with status code 200 (OK). Note: The Board and Words are not case sensitive.
        /// </summary>
        /// <param name="GameID">parameter</param>
        /// <param name="Brief">parameter</param>
        /// <returns></returns>
        [WebGet(UriTemplate = "/games/{GameID}?Brief={Brief}")]
        statudReturn4 gameStatus(string GameID, string Brief);
    }
}
