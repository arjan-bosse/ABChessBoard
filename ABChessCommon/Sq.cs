using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Squares tables and functions.
    /// Translation from index to square and vice versa.
    /// Calculation of distance between squares.
    /// </summary>
    public static class Sq
    {
        // board values
        static public char[] start_position =
        {
            '#','#','#','#','#','#','#','#','#','#',
            '#','#','#','#','#','#','#','#','#','#',
            '#','r','n','b','q','k','b','n','r','#',
            '#','p','p','p','p','p','p','p','p','#',
            '#',' ',' ',' ',' ',' ',' ',' ',' ','#',
            '#',' ',' ',' ',' ',' ',' ',' ',' ','#',
            '#',' ',' ',' ',' ',' ',' ',' ',' ','#',
            '#',' ',' ',' ',' ',' ',' ',' ',' ','#',
            '#','P','P','P','P','P','P','P','P','#',
            '#','R','N','B','Q','K','B','N','R','#',
            '#','#','#','#','#','#','#','#','#','#',
            '#','#','#','#','#','#','#','#','#','#'
        };

        static public string[] sq =
        {
            "##","##","##","##","##","##","##","##","##","##",
            "##","##","##","##","##","##","##","##","##","##",
            "##","a8","b8","c8","d8","e8","f8","g8","h8","##",
            "##","a7","b7","c7","d7","e7","f7","g7","h7","##",
            "##","a6","b6","c6","d6","e6","f6","g6","h6","##",
            "##","a5","b5","c5","d5","e5","f5","g5","h5","##",
            "##","a4","b4","c4","d4","e4","f4","g4","h4","##",
            "##","a3","b3","c3","d3","e3","f3","g3","h3","##",
            "##","a2","b2","c2","d2","e2","f2","g2","h2","##",
            "##","a1","b1","c1","d1","e1","f1","g1","h1","##",
            "##","##","##","##","##","##","##","##","##","##",
            "##","##","##","##","##","##","##","##","##","##"
        };

        // square values
        static public int a8 = 21;
        static public int b8 = 22;
        static public int c8 = 23;
        static public int d8 = 24;
        static public int e8 = 25;
        static public int f8 = 26;
        static public int g8 = 27;
        static public int h8 = 28;

        static public int a7 = 31;
        static public int b7 = 32;
        static public int c7 = 33;
        static public int d7 = 34;
        static public int e7 = 35;
        static public int f7 = 36;
        static public int g7 = 37;
        static public int h7 = 38;

        static public int a6 = 41;
        static public int b6 = 42;
        static public int c6 = 43;
        static public int d6 = 44;
        static public int e6 = 45;
        static public int f6 = 46;
        static public int g6 = 47;
        static public int h6 = 48;

        static public int a5 = 51;
        static public int b5 = 52;
        static public int c5 = 53;
        static public int d5 = 54;
        static public int e5 = 55;
        static public int f5 = 56;
        static public int g5 = 57;
        static public int h5 = 58;

        static public int a4 = 61;
        static public int b4 = 62;
        static public int c4 = 63;
        static public int d4 = 64;
        static public int e4 = 65;
        static public int f4 = 66;
        static public int g4 = 67;
        static public int h4 = 68;

        static public int a3 = 71;
        static public int b3 = 72;
        static public int c3 = 73;
        static public int d3 = 74;
        static public int e3 = 75;
        static public int f3 = 76;
        static public int g3 = 77;
        static public int h3 = 78;

        static public int a2 = 81;
        static public int b2 = 82;
        static public int c2 = 83;
        static public int d2 = 84;
        static public int e2 = 85;
        static public int f2 = 86;
        static public int g2 = 87;
        static public int h2 = 88;

        static public int a1 = 91;
        static public int b1 = 92;
        static public int c1 = 93;
        static public int d1 = 94;
        static public int e1 = 95;
        static public int f1 = 96;
        static public int g1 = 97;
        static public int h1 = 98;

        // row and column values
        static public int white_2nd_row = 80;
        static public int white_7th_row = 30;
        static public int black_2nd_row = 30;
        static public int black_7th_row = 80;

        // board index value of row or column 
        static private int[] rcidx_lookup;

        // Constructor.
        static Sq()
        {
            char rc = '#';
            for (int i = Sq.a8; i <= Sq.h1; i++) if (start_position[i] > rc) rc = start_position[i];
            rcidx_lookup = new int[rc + 1];
            for (rc = '1'; rc <= '8'; rc++) rcidx_lookup[rc] = 90 - 10 * (rc - '1');
            for (rc = 'a'; rc <= 'h'; rc++) rcidx_lookup[rc] = rc - 'a' + 1;
        }

        // Row or column to index calculation.
        static public int rcidx(char rc)
        {
            return rcidx_lookup[rc];
        }

        // Manhattan distance: number of rows and columns between two squares.
        static public int Mdist(int idx_a, int idx_b)
        {
            int col_diff = (idx_a % 10) - (idx_b % 10);
            int row_diff = (idx_a / 10) - (idx_b / 10);

            if (col_diff < 0) col_diff = -col_diff;
            if (row_diff < 0) row_diff = -row_diff;

            return col_diff + row_diff;
        }
    }
}
