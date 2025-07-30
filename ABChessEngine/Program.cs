using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessEngine
{
    class Program
    {
        /// <summary>
        /// The main entry point for the Console application.
        /// </summary>
        static void Main(string[] args)
        {
            // Start UCI engine.
            new UCIConsole().Start();
        }
    }
}
