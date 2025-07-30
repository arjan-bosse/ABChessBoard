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
    /// Large textbox to show GUI and engine reports.
    /// </summary>
    public partial class OutputForm : Form
    {
        public OutputForm()
        {
            InitializeComponent();
            this.ControlBox = false;
        }

        // Add a line to the textbox.
        public void AppendText(string text)
        {
            textBox1.AppendText(text + Environment.NewLine);
        }
    }
}
