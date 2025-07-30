using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace ABChessBoard
{
    /// <summary>
    /// Forms implementation to support engine reports.
    /// </summary>
    public class BoardReport : ABChessCommon.EngineReport
    {
        private BackgroundWorker worker = null;

        // Keep a reference to the worker so the engine reports can be passed through.
        public void setWorker(BackgroundWorker worker)
        {
            this.worker = worker;
        }

        public void Percentage(int percentage)
        {
            if (worker != null) worker.ReportProgress(percentage, "");
        }

        public void Report(string line)
        {
            if (line.Contains("info currmove")) return;
            if (worker != null) worker.ReportProgress(-1, line);
        }
    }
}
