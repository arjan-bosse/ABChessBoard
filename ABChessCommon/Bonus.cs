using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Give piece a bonus dependant of the square it is standing on.
    /// </summary>
    public static class Bonus
    {
        // Tables taken from:
        // http://chessprogramming.wikispaces.com/Simplified+evaluation+function

        public static readonly int[] w_pawn_middlegame =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0, 50, 50, 50, 50, 50, 50, 50, 50,  0,
            0, 10, 10, 20, 30, 30, 20, 10, 10,  0,
            0,  5,  5, 10, 25, 25, 10,  5,  5,  0,
            0,  0,  0,  0, 20, 20,  0,  0,  0,  0,
            0,  5, -5,-10,  0,  0,-10, -5,  5,  0,
            0,  5, 10, 10,-20,-20, 10, 10,  5,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        // New table to encourage pawn advancement in the endgame.
        public static readonly int[] w_pawn_endgame =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0, 50, 50, 50, 50, 50, 50, 50, 50,  0,
            0, 30, 30, 30, 30, 30, 30, 30, 30,  0,
            0, 25, 25, 25, 25, 25, 25, 25, 25,  0,
            0, 20, 20, 20, 20, 20, 20, 20, 20,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,-20,-20,-20,-20,-20,-20,-20,-20,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] w_knight =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,-50,-40,-30,-30,-30,-30,-40,-50,  0,
            0,-40,-20,  0,  0,  0,  0,-20,-40,  0,
            0,-30,  0, 10, 15, 15, 10,  0,-30,  0,
            0,-30,  5, 15, 20, 20, 15,  5,-30,  0,
            0,-30,  0, 15, 20, 20, 15,  0,-30,  0,
            0,-30,  5, 10, 15, 15, 10,  5,-30,  0,
            0,-40,-20,  0,  5,  5,  0,-20,-40,  0,
            0,-50,-40,-30,-30,-30,-30,-40,-50,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] w_bishop =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,-20,-10,-10,-10,-10,-10,-10,-20,  0,
            0,-10,  0,  0,  0,  0,  0,  0,-10,  0,
            0,-10,  0,  5, 10, 10,  5,  0,-10,  0,
            0,-10,  5,  5, 10, 10,  5,  5,-10,  0,
            0,-10,  0, 10, 10, 10, 10,  0,-10,  0,
            0,-10, 10, 10, 10, 10, 10, 10,-10,  0,
            0,-10,  5,  0,  0,  0,  0,  5,-10,  0,
            0,-20,-10,-10,-10,-10,-10,-10,-20,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] w_rook =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  5, 10, 10, 10, 10, 10, 10,  5,  0,
            0, -5,  0,  0,  0,  0,  0,  0, -5,  0,
            0, -5,  0,  0,  0,  0,  0,  0, -5,  0,
            0, -5,  0,  0,  0,  0,  0,  0, -5,  0,
            0, -5,  0,  0,  0,  0,  0,  0, -5,  0,
            0, -5,  0,  0,  0,  0,  0,  0, -5,  0,
            0,  0,  0,  0,  5,  5,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] w_queen =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,-20,-10,-10, -5, -5,-10,-10,-20,  0,
            0,-10,  0,  0,  0,  0,  0,  0,-10,  0,
            0,-10,  0,  5,  5,  5,  5,  0,-10,  0,
            0, -5,  0,  5,  5,  5,  5,  0, -5,  0,
            0,  0,  0,  5,  5,  5,  5,  0,  0,  0,
            0,-10,  5,  5,  5,  5,  5,  0,-10,  0,
            0,-10,  0,  5,  0,  0,  0,  0,-10,  0,
            0,-20,-10,-10, -5, -5,-10,-10,-20,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] w_king_middlegame =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,-30,-40,-40,-50,-50,-40,-40,-30,  0,
            0,-30,-40,-40,-50,-50,-40,-40,-30,  0,
            0,-30,-40,-40,-50,-50,-40,-40,-30,  0,
            0,-30,-40,-40,-50,-50,-40,-40,-30,  0,
            0,-20,-30,-30,-40,-40,-30,-30,-20,  0,
            0,-10,-20,-20,-20,-20,-20,-20,-10,  0,
            0, 20, 20,  0,  0,  0,  0, 20, 20,  0,
            0, 20, 30, 10,  0,  0, 10, 30, 20,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] w_king_endgame =
        {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,-50,-40,-30,-20,-20,-30,-40,-50,  0,
            0,-30,-20,-10,  0,  0,-10,-20,-30,  0,
            0,-30,-10, 20, 30, 30, 20,-10,-30,  0,
            0,-30,-10, 30, 40, 40, 30,-10,-30,  0,
            0,-30,-10, 30, 40, 40, 30,-10,-30,  0,
            0,-30,-10, 20, 30, 30, 20,-10,-30,  0,
            0,-30,-30,  0,  0,  0,  0,-30,-30,  0,
            0,-50,-30,-30,-30,-30,-30,-30,-50,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] b_pawn_middlegame = new int[120];
        public static readonly int[] b_pawn_endgame = new int[120];
        public static readonly int[] b_knight = new int[120];
        public static readonly int[] b_bishop = new int[120];
        public static readonly int[] b_rook = new int[120];
        public static readonly int[] b_queen = new int[120];
        public static readonly int[] b_king_middlegame = new int[120];
        public static readonly int[] b_king_endgame = new int[120];
        
        static Bonus()
        {
            for (int w_row = 0; w_row < 12; w_row++)
            {
                int b_row = 11 - w_row;

                for (int col = 0; col < 10; col++)
                {
                    int w_idx = 10 * w_row + col;
                    int b_idx = 10 * b_row + col;

                    b_pawn_middlegame[b_idx] = w_pawn_middlegame[w_idx];
                       b_pawn_endgame[b_idx] = w_pawn_endgame[w_idx];
                             b_knight[b_idx] = w_knight[w_idx];
                             b_bishop[b_idx] = w_bishop[w_idx];
                               b_rook[b_idx] = w_rook[w_idx];
                              b_queen[b_idx] = w_queen[w_idx];
                    b_king_middlegame[b_idx] = w_king_middlegame[w_idx];
                       b_king_endgame[b_idx] = w_king_endgame[w_idx];
                }
            }
        }
    }
}
