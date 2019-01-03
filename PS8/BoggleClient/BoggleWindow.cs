using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoggleModel;
/// <summary>
/// Boggle Game Client
/// Author by Lin Jia&& Jin HE
/// </summary>
namespace BoggleClient
{
    /// <summary>
    /// the view implements Form and IBoggle interface
    /// </summary>
    public partial class BoggleWindow : Form, IBoggle
    {     
        /// <summary>
        /// create a buttons hashset
        /// </summary>
        private HashSet<Button> buttons;
        /// <summary>
        /// constructor of BoggleWindow
        /// </summary>
        public BoggleWindow()
        {
            InitializeComponent();
            buttons = new HashSet<Button>();
            buttons.Add(button1);
            buttons.Add(button2);
            buttons.Add(button3);
            buttons.Add(button4);
            buttons.Add(button5);
            buttons.Add(button6);
            buttons.Add(button7);
            buttons.Add(button8);
            buttons.Add(button9);
            buttons.Add(button10);
            buttons.Add(button11);
            buttons.Add(button12);
            buttons.Add(button13);
            buttons.Add(button14);
            buttons.Add(button15);
            buttons.Add(button16);
            new contorller(this);
            gamePanel.Hide();
            pendingPanel.Hide();
            loginPanel.Location = new Point(0, 30);
            this.Size = new Size(397, 474);              
        }
        /// <summary>
        /// delcare leave game event
        /// </summary>
        public event Action LeaveGameEvent;
        /// <summary>
        /// declare create user event
        /// </summary>
        public event Action<string,String> creatUserEvent;
        /// <summary>
        /// declare join game event
        /// </summary>
        public event Action<string, int,String> joinGameEvent;
        /// <summary>
        /// declare usertoken event
        /// </summary>
        public event Func<string> UserTokenEvent;
        /// <summary>
        /// declare cancel game event
        /// </summary>
        public event Action<string,String> cancelGameEvent;
        /// <summary>
        /// declare check status event
        /// </summary>
        public event Action<bool, String> CheckStatusEvent;
        /// <summary>
        /// declare play event
        /// </summary>
        public event Action<string,String> playEvent;
        /// <summary>
        /// declare exit game event
        /// </summary>
        public event Action ExitGameEvent;
        /// <summary>
        /// declare help menu event
        /// </summary>
        public event Func<string> helpmenuEvent;
        /// <summary>
        /// declare help menu click event
        /// </summary>
        public event Action helpMenuClickEvent;
        /// <summary>
        /// delegate for callback
        /// </summary>
        /// <param name="timeleft">parameter</param>
        /// <param name="score">parameter</param>
        /// <param name="score2">parameter</param>
        public delegate void CallBack(int timeleft, int score, int score2);
        /// <summary>
        /// declare a task
        /// </summary>
        private Task t1;
        /// <summary>
        /// declare a domain 
        /// </summary>
        private String domain;

        private void leaveGameButton_Click(object sender, EventArgs e)
        {
            LeaveGameEvent();
        }
        /// <summary>
        /// help method used to exit the game
        /// </summary>
        public void shutDownGame()
        {
            Environment.Exit(Environment.ExitCode);
        }
        /// <summary>
        /// help method used to show the help message box
        /// </summary>
        public void clickHelpMenu()
        {
            MessageBox.Show(helpmenuEvent(), "Help");
        }
        /// <summary>
        /// help method to do the create user
        /// </summary>
        private void helperRegister()
        {
            refresh();
            try
            {
                creatUserEvent(nameBox.Text,this.domain);
            }
            catch (TaskCanceledException)
            {
               
                Thread.CurrentThread.Abort();
            }
            catch (Exception)
            {
                errorEvent("wrong domain format");
                Thread.CurrentThread.Abort();
            }
            int time;
            Int32.TryParse(TimeLimitTextBox.Text, out time);
            Task t3 = Task.Run(() => joinGameEvent(UserTokenEvent(), time,this.domain));
            BeginInvoke((Action)delegate ()
            {
                pendingLabel.Text = "    Waiting for another Player";
            });
            try
            {
                t3.Wait();
            }catch(AggregateException)
            {
                Thread.CurrentThread.Abort();
            }
            CheckStatusEvent(false,this.domain);
        }
        /// <summary>
        /// helper method to show the pending panel
        /// </summary>
        private void refresh()
        {
            BeginInvoke((Action)delegate ()
            {
                loginPanel.Hide();
                pendingPanel.Location = new Point(0, 30);
                this.Size = new Size(334, 450);
                pendingPanel.Show();          
                pendingLabel.Text = "                  connecting";
                
            });
        }
        /// <summary>
        /// login button click
        /// </summary>
        /// <param name="sender">parameter</param>
        /// <param name="e">parameter</param>
        private void loginButton_Click(object sender, EventArgs e)
        {
            if (domainBox.Text == "")
            {
               
                MessageBox.Show("the domain should not be empty.");
            }
            else
            {
                this.domain = domainBox.Text;
                if (this.domain.Last() != '/')
                {
                    this.domain = this.domain + '/';
                }
                t1 = Task.Run(() => helperRegister());
            }                                                 
                            
        }

        /// <summary>
        /// helper method to reset the players' scores
        /// </summary>
        /// <param name="timeleft">parameter</param>
        /// <param name="score">parameter</param>
        /// <param name="score2">parameter</param>
        public void repaint(int timeleft, int score, int score2)
        {

            BeginInvoke((Action)delegate ()
            {
                timeAppear.Text = timeleft + "";
                CustomerScore.Text = score2 + "";
                hostScore.Text = score + "";
                
            });
            CheckStatusEvent(true,this.domain);

        }

        /// <summary>
        /// cancel button click
        /// </summary>
        /// <param name="sender">parameter</param>
        /// <param name="e">parameter</param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
          
            
            Task.Run(() => cancelGameEvent(UserTokenEvent(),this.domain));
            

            LeaveGameEvent();
            pendingPanel.Hide();
            loginPanel.Location = new Point(0, 30);
            this.Size = new Size(397, 474);
            loginPanel.Show();
        }
        /// <summary>
        /// when the status is active, need to get the right now status
        /// </summary>
        /// <param name="board">parameter</param>
        /// <param name="timeleft">parameter</param>
        /// <param name="name">parameter</param>
        /// <param name="score">parameter</param>
        /// <param name="name2">parameter</param>
        /// <param name="score2">parameter</param>
        public void paintActive(string board, int timeleft, string name, int score, string name2, int score2)
        {
            BeginInvoke((Action)delegate ()
            {
                pendingPanel.Hide();
                gamePanel.Location = new Point(0, 30);
                this.Size = new Size(580, 529);
                gamePanel.Show();
                int i = 0;
                foreach (Button buttons in buttons)
                {
                    buttons.Text = board[i] + "";
                    i++;
                }
                timeAppear.Text = timeleft + "";
                hostBox.Text = name;
                hostScore.Text = score + "";
                customerBox.Text = name2;
                CustomerScore.Text = score2 + "";
                
            });
            
            CheckStatusEvent(true,this.domain);
        }
        /// <summary>
        /// send button click
        /// </summary>
        /// <param name="sender">parameter</param>
        /// <param name="e">parameter</param>
        private void sendButton_Click(object sender, EventArgs e)
        {
         
            String content = wordBox.Text;
            Task.Run(() => playEvent(content,this.domain));
                
            
            wordBox.Text = "";
        }
        /// <summary>
        /// helper method to wait player
        /// </summary>
        public void waitPlayerComing()
        {  
           CheckStatusEvent(false,this.domain);
        }
        /// <summary>
        /// need to add player's information to here, and actually we need to get a way to make the red part appear the host players all the time. 
        /// </summary>
        /// <param name="name">parameter</param>
        /// <param name="score">parameter</param>
        /// <param name="words">parameter</param>
        /// <param name="name2">parameter</param>
        /// <param name="Score2">parameter</param>
        /// <param name="woreds2">parameter</param>
        public void getResult(string name, int score, dynamic words, string name2, int Score2, dynamic words2)
        {
            BeginInvoke((Action)delegate ()
            {
                timeAppear.Text = 0 + "";
                String result1="";
                String result2 = "";
                foreach(dynamic word in words)
                {
                    result1 += "\nword: " + word.Word + " score is: " + word.Score;
                }
                foreach (dynamic word in words2)
                {
                    result2 += "\nword: " + word.Word + " score is: " + word.Score;
                }

                MessageBox.Show("player1's name: " + name + "\nthe score is: " + score + "\nthe words played: \n" + result1
                    +"\nPlayer2's name: "+name2+"\nthe score is: "+Score2+"\nthe words played: \n"+result2);
                stopService();
            });
        }
        /// <summary>
        /// restart a new game,and stop the backgroundworker. 
        /// </summary>
        public void stopService()
        {
            BeginInvoke((Action)delegate ()
            {
                wordBox.Text = "";
                gamePanel.Hide();
                pendingPanel.Hide();
                loginPanel.Location = new Point(0, 30);
                this.Size = new Size(397, 474);
                loginPanel.Show();
            });
        }
        /// <summary>
        /// quit button click
        /// </summary>
        /// <param name="sender">parameter</param>
        /// <param name="e">parameter</param>
        private void gamePanelQuitButton_Click(object sender, EventArgs e)
        {
            ExitGameEvent();
        }
        /// <summary>
        /// exit button click
        /// </summary>
        /// <param name="sender">parameter</param>
        /// <param name="e">parameter</param>
        private void logInPanelQuitButton_Click(object sender, EventArgs e)
        {
            ExitGameEvent();
        }
        /// <summary>
        /// help menu click
        /// </summary>
        /// <param name="sender">parameter</param>
        /// <param name="e">parameter</param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            helpMenuClickEvent();
        }
        /// <summary>
        /// help method to handle error
        /// </summary>
        /// <param name="info">parameter</param>
        public void errorEvent(string info)
        {
            MessageBox.Show(info);
            stopService();
        }
        /// <summary>
        /// help method to show word list user type in 
        /// </summary>
        /// <param name="info">parameter</param>
        public void wordEvent(string info)
        {
            MessageBox.Show(info);
        }
        /// <summary>
        /// help method to handle close in the form
        /// </summary>
        /// <param name="sender">parameter</param>
        /// <param name="e">parameter</param>
        private void BoggleWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
    }
}
