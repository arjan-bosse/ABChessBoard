using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABChessBoard
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the Windows Forms application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Start GUI.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BoardForm());
        }
    }
}
