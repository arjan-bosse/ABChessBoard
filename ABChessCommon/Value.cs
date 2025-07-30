using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Global values used in the application.
    /// </summary>
    public static class Value
    {
        // Piece values in centipawns
        public static int queen = 900;
        public static int rook = 500;
        public static int bishop = 330;
        public static int knight = 320;
        public static int pawn = 100;

        // Additional values
        public static int max = 100000;
        public static int mate = 50000;
        public static int winning = 20000;

        // Engine search depth
        public static int search_depth = 4;
        public static int quiescence_depth = 8;

        // Nodes to calculate before report
        public static long info_per_nodes = 100000;
    }
}
