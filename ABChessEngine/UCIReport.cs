using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessEngine
{
    /// <summary>
    /// Console implementation to support engine reports.
    /// </summary>
    public class UCIReport : ABChessCommon.EngineReport
    {
        public void Percentage(int percentage)
        {
            // not supported in UCI
        }

        public void Report(string line)
        {
            Console.WriteLine(line);
        }
    }
}
