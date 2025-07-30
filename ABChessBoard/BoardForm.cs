using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// ABChess (GUI top level)
//
// Version: 0.1 (original)
// Date   : 2015-08-15
// Author : Arjan Bosse
//
// Version: 0.2 (fix)
// Date   : 2016-11-06
// Author : Arjan Bosse

namespace ABChessBoard
{
    /// <summary>
    /// GUI application of ABChessBoard.
    /// 
    /// The layout is in three parts.
    /// On top: button '-', textbox 'depth', button '+',  button 'start', button 'stop', button 'undo', button 'reset'.
    /// Middle: large bitmap colored with squares and pieces.
    /// Bottom: textbox with last played move, progress bar while calculating.
    /// 
    /// This form is handling GUI events from and to the form.
    /// I.e. events when a button or square is clicked.
    /// Events when the worker thread is finished.
    /// Callback for the promotion dialog.
    /// Showing a redrawn board and updated texts.
    /// 
    /// Additional GUI forms are created, and also a a state object.
    /// The game logic (model) and the bitmap painting are passed to the state object. 
    /// </summary>
    public partial class BoardForm : Form
    {
        // Square size in pixels.
        private int squareWidth;
        private int squareHeight;

        // Worker thread is thinking or not.
        private Boolean working;

        // Special form to show promoted pieces.
        private PromotionForm promotionForm;
        // Special form to show output from the reporter.
        private OutputForm outputForm;
        // Implementation of a Engine reporter used by the Chess Engine under the hood.
        private BoardReport boardReport;
        // State object containing all Model logic.
        private BoardState boardstate;

        // Constructor of the GUI application.
        public BoardForm()
        {
            InitializeComponent();

            // Square size depends on board size.
            squareWidth = pictureBox1.Size.Width / 8;
            squareHeight = pictureBox1.Size.Height / 8;

            // Function to call when the user clicks a square.
            pictureBox1.MouseDown += pictureBox_MouseDown;

            // Worker thread.
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            backgroundWorker1.WorkerReportsProgress = true;
            working = false;

            promotionForm = new PromotionForm(squareWidth, squareHeight);
            outputForm = new OutputForm();
            boardReport = new BoardReport();      
            boardstate = new BoardState(squareWidth, squareHeight, this);
        }

        // Called from BoardState, this is needed to show a promotion dialog.
        // I.e. a modal dialog to force the player to pick a promoted piece. 
        public char Promotion(Boolean white)
        {
            Bitmap promotion = boardstate.DrawPromotion(white);
            promotionForm.AddPieces(promotion);
            promotionForm.ShowDialog();
            return promotionForm.GetChoice();
        }

        // Show some info about end of game and why no moves can be made.
        public void EndOfGame(string line)
        {
            outputForm.AppendText(line);
            outputForm.Show();
        }

        // Render the board and texts.
        private void Redraw()
        {
            // get (re)painted board bitmap from  the State
            Bitmap board = boardstate.DrawPosition();
            pictureBox1.Image = board;
            this.Controls.Add(pictureBox1);

            // Get last played move from State
            toolStripStatusLabel1.Text = boardstate.LastMove();
            toolStripProgressBar1.Value = 0;

            // Get search depth from State
            toolStripTextBox1.Text = boardstate.GetDepth().ToString();
        }

        // Show the board.
        // Create an output dialog.
        private void Board_Load(object sender, EventArgs e)
        {
            Redraw();
            outputForm.Show();
        }

        // Worker thread reports some progress.
        // Update progress bar and/or output text.
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int percentage = e.ProgressPercentage;
            string line = (string)e.UserState;

            if (percentage >= 0 && percentage <= 100)
            {
                toolStripProgressBar1.Value = percentage;
            }
            if (line != "")
            {
                outputForm.AppendText(line);
                outputForm.Show();
            }
        }

        // Worker thread finished calculating.
        // The thread itself already has stopped running.
        // Forward this fact to the State.
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            boardstate.WorkDone();
            working = false;
            Redraw();
        }

        // User clicked on a square.
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Ignore when calculating.
            if (working) return;

            Boolean left = (e.Button == MouseButtons.Left);
            Boolean right = (e.Button == MouseButtons.Right);
            Point location = e.Location;
            int x = location.X / squareWidth;
            int y = location.Y / squareHeight;
            int clicks = e.Clicks;

            // Formalize the event to the State. Pass myself so I can be called to show a promotion dialog.
            Boolean dowork = boardstate.HandleMouse(left, right, x, y, clicks);
            Redraw();

            // If the State requests a calculation, start running worker thread.
            if (dowork)
            {
                working = true;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        // Worker thread has been started, let it do the requested work.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get access to the worker
            BackgroundWorker worker = (BackgroundWorker)sender;
            // Pass worker to the reporter
            boardReport.setWorker(worker);
            // Pass reporter to the State and do the work from there
            boardstate.DoWork(boardReport);
        }

        // Handle clicked button. Decrement search depth.
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (working) return;
            boardstate.HandleButton("-");
            Redraw();
        }

        // Handle clicked button. Increment search depth.
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (working) return;
            boardstate.HandleButton("+");
            Redraw();
        }

        // Handle clicked button. Start calculating.
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (working) return;
            Boolean dowork = boardstate.HandleButton("start");
            Redraw();

            // If the State requests a calculation, start running worker thread.
            if (dowork)
            {
                working = true;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        // Handle clicked button. Stop calculating.
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (!working) return;
            boardstate.HandleButton("stop");
        }

        // Handle clicked button. Undo last move.
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (working) return;
            boardstate.HandleButton("undo");
            Redraw();
        }

        // Handle clicked button. Reset game.
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (working) return;
            boardstate.HandleButton("reset");
            Redraw();
        }
    }
}
