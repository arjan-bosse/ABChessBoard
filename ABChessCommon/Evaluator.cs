using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Position evaluator giving score, based on material, pieces location and positional analytics.
    /// Special heuristics to implement KA endgames.
    /// A move evaluator can be used to provide a score to a specific move.
    /// </summary>
    public static class Evaluator
    {
        // KA EVALUATOR

        // KA vs K + N + B

        private static int[] KA_black_arr =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 7, 6, 5, 4, 3, 2, 1, 0, 0,
            0, 6, 7, 6, 5, 4, 3, 2, 1, 0,
            0, 5, 6, 7, 6, 5, 4, 3, 2, 0,
            0, 4, 5, 6, 7, 6, 5, 4, 3, 0,
            0, 3, 4, 5, 6, 7, 6, 5, 4, 0,
            0, 2, 3, 4, 5, 6, 7, 6, 5, 0,
            0, 1, 2, 3, 4, 5, 6, 7, 6, 0,
            0, 0, 1, 2, 3, 4, 5, 6, 7, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private static int[] KA_white_arr =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 2, 3, 4, 5, 6, 7, 0,
            0, 1, 2, 3, 4, 5, 6, 7, 6, 0,
            0, 2, 3, 4, 5, 6, 7, 6, 5, 0,
            0, 3, 4, 5, 6, 7, 6, 5, 4, 0,
            0, 4, 5, 6, 7, 6, 5, 4, 3, 0,
            0, 5, 6, 7, 6, 5, 4, 3, 2, 0,
            0, 6, 7, 6, 5, 4, 3, 2, 1, 0,
            0, 7, 6, 5, 4, 3, 2, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private static int[] knight_white_arr =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 1, 1, 1, 1, 1, 0, 0,
            0, 1, 5, 1, 5, 1, 5, 1, 1, 0,
            0, 1, 1, 1, 5, 1, 1, 5, 1, 0,
            0, 1, 5, 5, 1, 5, 1, 1, 1, 0,
            0, 1, 1, 1, 5, 1, 5, 5, 1, 0,
            0, 1, 5, 1, 1, 5, 1, 1, 1, 0,
            0, 1, 1, 5, 1, 5, 1, 5, 1, 0,
            0, 0, 1, 1, 1, 1, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private static int[] knight_black_arr =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 1, 1, 1, 1, 1, 0, 0,
            0, 1, 1, 5, 1, 5, 1, 5, 1, 0,
            0, 1, 5, 1, 1, 5, 1, 1, 1, 0,
            0, 1, 1, 1, 5, 1, 5, 5, 1, 0,
            0, 1, 5, 5, 1, 5, 1, 1, 1, 0,
            0, 1, 1, 1, 5, 1, 1, 5, 1, 0,
            0, 1, 5, 1, 5, 1, 5, 1, 1, 0,
            0, 0, 1, 1, 1, 1, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        // KA vs K + others 

        private static int[] KA_sc_arr =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 2, 3, 3, 2, 1, 0, 0,
            0, 1, 2, 3, 4, 4, 3, 2, 1, 0,
            0, 2, 3, 4, 5, 5, 4, 3, 2, 0,
            0, 3, 4, 5, 6, 6, 5, 4, 3, 0,
            0, 3, 4, 5, 6, 6, 5, 4, 3, 0,
            0, 2, 3, 4, 5, 5, 4, 3, 2, 0,
            0, 1, 2, 3, 4, 4, 3, 2, 1, 0,
            0, 0, 1, 2, 3, 3, 2, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private static int[] king_sc_arr =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 5, 1, 1, 5, 1, 0, 0,
            0, 1, 1, 5, 5, 5, 5, 1, 1, 0,
            0, 5, 5, 5, 5, 5, 5, 5, 5, 0,
            0, 1, 5, 5, 5, 5, 5, 5, 1, 0,
            0, 1, 5, 5, 5, 5, 5, 5, 1, 0,
            0, 5, 5, 5, 5, 5, 5, 5, 5, 0,
            0, 1, 1, 5, 5, 5, 5, 1, 1, 0,
            0, 0, 1, 5, 1, 1, 5, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        // find a 'win' for me against the enemy 'King Alone'
        private static int win_KA(State me, State enemy)
        {
            // make a technical win happen
            int score = Value.winning;

            // strategy: drive KA to the border by all means
            // then 'hope' to see the mate within the horizon
            // special case: when KA vs K+B+N then drive KA to the B corner

            // very specific endgame: penalty non bishop-color corner
            if (me.pieces == 3 && me.n_len == 1 && me.b_len == 1)
            {
                // figure out the good and bad corners

                // if the board index of the bishop is even, it is white squared
                Boolean white_bishop = (me.b[0] % 2 == 0);

                if (white_bishop)
                {
                    // a8 or h1 is the square to reach
                    score -= 20 * KA_white_arr[enemy.k_idx];

                    // knight should help at correct squares
                    score = 10 * knight_white_arr[me.n[0]];
                }
                else
                {
                    // a1 or h8 is the square to reach
                    score -= 20 * KA_black_arr[enemy.k_idx];

                    // knight should help at correct squares
                    score = 10 * knight_black_arr[me.n[0]];
                }
            }
            else
            {
                // penalise a centralized KA, force the KA to the border
                score -= 20 * KA_sc_arr[enemy.k_idx];
            }

            // bonus centralized king, encourage the king to the center
            score += 10 * king_sc_arr[me.k_idx];

            // penalty when short ranged pieces (K and N) are far from the KA

            // let king help, penalise distance
            score -= 5 * Sq.Mdist(me.k_idx, enemy.k_idx);

            // let knights help, penalise distance
            for (int i = 0; i < me.n_len; i++)
            {
                score -= 2 * Sq.Mdist(me.n[i], enemy.k_idx);
            }

            // penalty bishop in corner
            for (int i = 0; i < me.b_len; i++)
            {
                int idx = me.b[i];
                if (idx == Sq.a1 || idx == Sq.a8 || idx == Sq.h1 || idx == Sq.h8) score -= 5;
            }

            return score;
        }

        // EVALUATOR

        /// <summary>
        /// Give score to player to move.
        /// The evaluation function is mostly based on the material.
        /// A bonus is given for the location of each specific piece.
        /// Some positional analytics will be done.
        /// In case of special endgames, specific heuristics will be applied.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static int evaluate(Position position)
        {
            // INITIALIZE

            // white and black state information
            State white = new State(true);
            State black = new State(false);


            // COUNT MATERIAL AND COLLECT ANALYTICS

            // number of pieces
            // list with indices per piece type
            // summarized piece values
            for (int i = Sq.a8; i <= Sq.h1; i++ )
            {
                char piece = position.pieceat(i);
                if (piece == ' ' || piece == '#') continue;
                if (Char.IsUpper(piece)) white.add(piece, i); else black.add(piece, i);
            }

            // check material
            int value = white.value - black.value;

            // game phase
            // Decide whether or not each side is in the opening/middlegame or endgame.
            Boolean white_endgame = 3 * white.q_len + white.r_len + white.b_len + white.n_len <= 4;
            Boolean black_endgame = 3 * black.q_len + black.r_len + black.b_len + black.n_len <= 4;

            // pawns structure: double, isolated
            // bishop pair
            white.analyse();
            black.analyse();


            // LOCATION BONUS

            // global pieces location bonus
            // note: this depends on the game phase of the opponent!
            int white_bonus = 0;
            int black_bonus = 0;
            for (int i = Sq.a8; i <= Sq.h1; i++ )
            {
                char piece = position.pieceat(i);
                switch (piece)
                {
                    case ' ':
                        break;
                    case '#':
                        break;
                    case 'P':
                        white_bonus += black_endgame ? Bonus.w_pawn_endgame[i] : Bonus.w_pawn_middlegame[i];
                        break;
                    case 'N':
                        white_bonus += Bonus.w_knight[i];
                        break;
                    case 'B':
                        white_bonus += Bonus.w_bishop[i];
                        break;
                    case 'R':
                        white_bonus += Bonus.w_rook[i];
                        break;
                    case 'Q':
                        white_bonus += Bonus.w_queen[i];
                        break;
                    case 'K':
                        white_bonus += black_endgame ? Bonus.w_king_endgame[i] : Bonus.w_king_middlegame[i];
                        break;
                    case 'p':
                        black_bonus += white_endgame ? Bonus.b_pawn_endgame[i] : Bonus.b_pawn_middlegame[i];
                        break;
                    case 'n':
                        black_bonus += Bonus.b_knight[i];
                        break;
                    case 'b':
                        black_bonus += Bonus.b_bishop[i];
                        break;
                    case 'r':
                        black_bonus += Bonus.b_rook[i];
                        break;
                    case 'q':
                        black_bonus += Bonus.b_queen[i];
                        break;
                    case 'k':
                        black_bonus += white_endgame ? Bonus.b_king_endgame[i] : Bonus.b_king_middlegame[i];
                        break;
                }
            }

            // The location bonus concept has a flaw.
            // In the beginning the king is not hindered from moving to a castling side WITHOUT castling.
            // And doing so giving up the possible castling ability which is bad.
            // Moving from the original square should only be attactive AFTER or as PART of castling.
            // Based on the tables, moving the king can give 10 bonus and not moving a pawn can save 20.
            // Penalty 40 when king not in place at the bottom two lines with both rooks still in place.

            if (white.k_idx >= Sq.a2 && white.k_idx != Sq.e1 && white.r_len == 2 && white.r[0] == Sq.a1 && white.r[1] == Sq.h1) white_bonus -= 40;
            if (black.k_idx <= Sq.h7 && black.k_idx != Sq.e8 && black.r_len == 2 && black.r[0] == Sq.a8 && black.r[1] == Sq.h8) black_bonus -= 40;

            // location bonus
            int bonus = white_bonus - black_bonus;


            // SPECIAL ENDGAMES

            // KA vs KA
            if (white.pieces == 1 && black.pieces == 1) return 0;

            // KA vs K + light piece
            if (white.pieces == 1 && black.pieces == 2 && (black.n_len == 1 || black.b_len == 1)) return 0;
            if (black.pieces == 1 && white.pieces == 2 && (white.n_len == 1 || white.b_len == 1)) return 0;

            // KA vs K + two knights
            if (white.pieces == 1 && black.pieces == 3 && black.n_len == 2) return 0;
            if (black.pieces == 1 && white.pieces == 3 && white.n_len == 2) return 0;

            // KA vs K + non-pawn mating potential
            // it's a technical win so focus on that
            // By keeping the value in the score the engine will not 'give away' material because it's a win anyway.
            // The location bonus and other analytics however must be ignored.
            if (black.pieces == 1 && (white.q_len >= 1 || white.r_len >= 1 || white.bishop_pair || (white.n_len >= 1 && white.b_len >= 1) || white.n_len >= 3))
            {
                int sc = win_KA(white, black);
                return position.whitetomove() ? value + sc : -value - sc;
            }
            if (white.pieces == 1 && (black.q_len >= 1 || black.r_len >= 1 || black.bishop_pair || (black.n_len >= 1 && black.b_len >= 1) || black.n_len >= 3))
            {
                int sc = win_KA(black, white);
                return position.whitetomove() ? value - sc : -value + sc;
            }


            // EVALUATE POSITIONAL

            // pawns
            int pawns = 10 * (black.double_pawns + black.isolated_pawns - white.double_pawns - white.isolated_pawns);

            // Let material outweigh the positional bonus by a factor 2 making the computer more greedy.
            // Reconsider this when the evaluation function becomes better.

            // total score
            int total = value + (bonus + pawns) / 2;

            // reverse total score if black to move
            return position.whitetomove() ? total : -total;
        }


        // BASIC MOVE EVALUATOR

        // square occupation score
        private static int[] basic_move_sc =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0,10,10,10,10,10,10, 0, 0,
            0, 0,10,20,20,20,20,10, 0, 0,
            0, 0,10,20,30,30,20,10, 0, 0,
            0, 0,10,20,30,30,20,10, 0, 0,
            0, 0,10,20,20,20,20,10, 0, 0,
            0, 0,10,10,10,10,10,10, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        // evaluation per move to start with a nice ordered movelist, especially in the opening
        public static int basic_move_score(Position position, int idx_from, int idx_to, string promotion, Boolean enpassant)
        {
            int bonus = 0;
            char piece;

            // piece captured
            piece = position.pieceat(idx_to);
            if (piece != ' ')
            {
                if (piece == 'p' || piece == 'P') bonus += Value.pawn;
                if (piece == 'n' || piece == 'N') bonus += Value.knight;
                if (piece == 'b' || piece == 'B') bonus += Value.bishop;
                if (piece == 'r' || piece == 'R') bonus += Value.rook;
                if (piece == 'q' || piece == 'Q') bonus += Value.queen;
            }

            // en-passant
            if (enpassant) bonus += Value.pawn;

            // promotion
            if (promotion != "")
            {
                if (promotion == "q") bonus += Value.queen - Value.pawn;
                if (promotion == "r") bonus += Value.rook - Value.pawn;
                if (promotion == "b") bonus += Value.bishop - Value.pawn;
                if (promotion == "n") bonus += Value.knight - Value.pawn;
            }

            // piece moved
            piece = position.pieceat(idx_from);

            // score king and rook for castling
            // score other pieces for centralization
            if (piece == 'K' || piece == 'k')
            {
                if (idx_from == idx_to + 2 || idx_to == idx_from + 2)
                {
                    // castle bonus
                    bonus += 50;
                }
                else if (!position.get_a_moved(piece == 'K') || !position.get_h_moved(piece == 'K'))
                {
                    // castle penalty (when giving up castle ability)
                    bonus -= 50;
                }
            }
            else if (piece == 'R' && (idx_from == Sq.a1 && !position.get_a_moved(true) || idx_from == Sq.h1 && !position.get_h_moved(true)) || piece == 'r' && (idx_from == Sq.a8 && !position.get_a_moved(false) || idx_from == Sq.h1 && !position.get_h_moved(false)))
            {
                // castle penalty (when giving up castle ability)
                bonus -= 30;
            }
            else
            {
                // positional bonus when moving towards centre
                bonus += basic_move_sc[idx_to];
                bonus -= basic_move_sc[idx_from];
            }

            return bonus;
        }
    }
}
