using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABChessBoard
{
    /// <summary>
    /// Show four pieces to choose from after a promotion.
    /// </summary>
    public partial class PromotionForm : Form
    {
        private int squareWidth;
        private int squareHeight;

        private char default_promotion = 'q';
        private char choice;

        // Constructor.
        public PromotionForm(int squareWidth, int squareHeight)
        {
            InitializeComponent();
            this.ControlBox = false;

            this.squareWidth = squareWidth;
            this.squareHeight = squareHeight;

            pictureBox1.MouseDown += pictureBox_MouseDown;
            choice = default_promotion;
        }

        public char GetChoice()
        {
            return choice;
        }

        // Fill with bitmap containing the right colored pieces.
        public void AddPieces(Bitmap promotion)
        {
            pictureBox1.Image = promotion;
            this.Controls.Add(pictureBox1);
            choice = default_promotion;
        }

        // On click return selected piece and close modal dialog.
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            Point location = e.Location;
            int x = location.X / squareWidth;
            int y = location.Y / squareHeight;

            choice = default_promotion;
            if (x == 0) choice = 'q';
            if (x == 1) choice = 'r';
            if (x == 2) choice = 'b';
            if (x == 3) choice = 'n';

            this.Close();
        }
    }
}
