using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Implement to allow engine reports.
    /// </summary>
    public interface EngineReport
    {
        void Percentage(int percentage);
        void Report(string line);
    }
}
