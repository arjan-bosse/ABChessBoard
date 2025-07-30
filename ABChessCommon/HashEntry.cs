using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    public class HashEntry
    {
        public Position position;
        public Score score;
        public int tdepth;
        
        public HashEntry(Position position, Score score, int tdepth)
        {
            this.position = position;
            this.score = score;
            this.tdepth = tdepth;
        }
    }
}
