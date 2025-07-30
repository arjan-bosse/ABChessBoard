using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Helper class for evaluation.
    /// For each side create and analyse the position state:
    /// Count pieces on board and their values.
    /// Bishop pair or ot.
    /// Isolated and double pawns.
    /// </summary>
    public class State
    {
        // my color
        public Boolean im_white;

        // set when adding pieces
        public int k_idx = -1;
        public int[] q = new int[9];
        public int q_len = 0;
        public int[] r = new int[10];
        public int r_len = 0;
        public int[] b = new int[10];
        public int b_len = 0;
        public int[] n = new int[10];
        public int n_len = 0;
        public int[] p = new int[8];
        public int p_len = 0;
        public int pieces = 0;
        public int value = 0;

        // set when analyse
        public int double_pawns = 0;
        public int isolated_pawns = 0;
        public Boolean bishop_pair = false;


        // CREATE

        // constructor
        public State(Boolean im_white)
        {
            this.im_white = im_white;
        }

        // add piece to state
        public void add(char piece, int idx)
        {
            if (piece == 'p' || piece == 'P') { value += Value.pawn; p[p_len++] = idx; }
            if (piece == 'n' || piece == 'N') { value += Value.knight; n[n_len++] = idx; }
            if (piece == 'b' || piece == 'B') { value += Value.bishop; b[b_len++] = idx; }
            if (piece == 'r' || piece == 'R') { value += Value.rook; r[r_len++] = idx; }
            if (piece == 'q' || piece == 'Q') { value += Value.queen; q[q_len++] = idx; }
            if (piece == 'k' || piece == 'K') k_idx = idx;
            pieces++;
        }


        // ANALYSE 

        // Count double and/or isolated pawns.
        private void pawns()
        {
            // pawns per column
            int[] pawn_col_count = new int[10];

            // count
            for (int i = 0; i < p_len; i++)
            {
                pawn_col_count[p[i] % 10]++;
            }

            // score
            for (int i = 1; i <= 8; i++)
            {
                // double
                if (pawn_col_count[i] > 1)
                {
                    double_pawns += (pawn_col_count[i] - 1);
                }

                // isolated
                if (pawn_col_count[i - 1] == 0 && pawn_col_count[i + 1] == 0)
                {
                    isolated_pawns += pawn_col_count[i];
                }
            }
        }

        // Bishop pair or not.
        private void bishops()
        {
            Boolean idx_0 = false;
            Boolean idx_1 = false;
            for (int i = 0; i < b_len; i++)
            {
                if (b[i] % 2 == 0) idx_0 = true; else idx_1 = true;
            }
            if (idx_0 && idx_1) bishop_pair = true;
        }

        // Analyse position for positional aspects.
        public void analyse()
        {
            pawns();
            bishops();
        }
    }
}
