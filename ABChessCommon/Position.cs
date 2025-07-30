using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Implement chess board and the position of pieces.
    ///
    /// Multiple parts:
    ///
    /// Common. Internal presentation of board, pieces, setup position, board update when move, check check. 
    /// List of moves. Create a list of legal moves in the given position.
    ///
    /// NOTE: This class ONLY knows the internals of the position. It does NOT do any judgement about
    /// the position. Any scoring and evaluation is delegated to the Evaluator.
    /// </summary>
    public class Position
    {
        // hashing
        private ulong hash;
        private Position[] threefold_hist;
        private int threefold_hist_len;
        private readonly int threefold_hist_max = 150;

        // dynamic elements
        private char[] squares;
        private Boolean white_to_move;
        private int ep_idx;
        private Boolean white_a_moved;
        private Boolean white_h_moved;
        private Boolean black_a_moved;
        private Boolean black_h_moved;
        private int fifty;
        private readonly int fifty_max = 100;
        private int fullmove;
        private Boolean material_updated;

        // king locations
        private int white_king_idx;
        private int black_king_idx;

        // movelist part
        private Moves moves;

        // COMMON PART

        // find out whether or not a square is attacked in a specific direction by one of two specific pieces
        private Boolean check_direction(int idx, int offset, char p1, char p2)
        {
            char piece;
            do
            {
                idx += offset;
                piece = squares[idx];
                if (piece == p1 || piece == p2) return true;
            } while (piece == ' ');

            return false;
        }

        // find out whether or not a square of the active player is attacked
        private Boolean square_attacked(int idx)
        {
            // knight
            char enemy_n = white_to_move ? 'n' : 'N';
            if (squares[idx - 21] == enemy_n) return true;
            if (squares[idx - 19] == enemy_n) return true;
            if (squares[idx - 12] == enemy_n) return true;
            if (squares[idx - 8] == enemy_n) return true;
            if (squares[idx + 8] == enemy_n) return true;
            if (squares[idx + 12] == enemy_n) return true;
            if (squares[idx + 19] == enemy_n) return true;
            if (squares[idx + 21] == enemy_n) return true;

            // bishop (or queen)
            char enemy_b = white_to_move ? 'b' : 'B';
            char enemy_q = white_to_move ? 'q' : 'Q';
            if (check_direction(idx, -11, enemy_b, enemy_q)) return true;
            if (check_direction(idx, 11, enemy_b, enemy_q)) return true;
            if (check_direction(idx, -9, enemy_b, enemy_q)) return true;
            if (check_direction(idx, 9, enemy_b, enemy_q)) return true;

            // rook (or queen)
            char enemy_r = white_to_move ? 'r' : 'R';
            if (check_direction(idx, -10, enemy_r, enemy_q)) return true;
            if (check_direction(idx, 10, enemy_r, enemy_q)) return true;
            if (check_direction(idx, -1, enemy_r, enemy_q)) return true;
            if (check_direction(idx, 1, enemy_r, enemy_q)) return true;

            // pawn
            if (white_to_move)
            {
                if (squares[idx - 9] == 'p') return true;
                if (squares[idx - 11] == 'p') return true;
            }
            else
            {
                if (squares[idx + 9] == 'P') return true;
                if (squares[idx + 11] == 'P') return true;
            }

            // king
            char enemy_k = white_to_move ? 'k' : 'K';
            if (squares[idx - 11] == enemy_k) return true;
            if (squares[idx - 10] == enemy_k) return true;
            if (squares[idx - 9] == enemy_k) return true;
            if (squares[idx - 1] == enemy_k) return true;
            if (squares[idx + 1] == enemy_k) return true;
            if (squares[idx + 9] == enemy_k) return true;
            if (squares[idx + 10] == enemy_k) return true;
            if (squares[idx + 11] == enemy_k) return true;

            // not attacked
            return false;
        }

        // find out whether or not the king of the active player is attacked
        public Boolean check()
        {
            return square_attacked(white_to_move ? white_king_idx : black_king_idx);
        }

        // find out whether or not two positiona are the same
        public Boolean equals(Position pos)
        {
            if (pos == null) return false;
            if (pos == this) return true;

            if (hash != pos.hash) return false;

            if (white_to_move != pos.white_to_move) return false;
            if (ep_idx != pos.ep_idx) return false;

            if (white_a_moved != pos.white_a_moved) return false;
            if (white_h_moved != pos.white_h_moved) return false;
            if (black_a_moved != pos.black_a_moved) return false;
            if (black_h_moved != pos.black_h_moved) return false;

            for (int i = Sq.a8; i <= Sq.h1; i++)
            {
                if (squares[i] != pos.squares[i]) return false;
            }

            return true;
        }

        // Find out if this position occurred two times before in the history
        public Boolean threefold_repetition()
        {
            if (threefold_hist_len >= 3)
            {
                int hit = 0;
                for (int i = threefold_hist_len - 1; i >= 0; i--)
                {
                    if (this.equals(threefold_hist[i]))
                    {
                        if (++hit == 2) return true;
                    }
                }
            }
            return false;
        }

        // Find out if fifty move rule is applicable
        public Boolean fifty_move_rule()
        {
            // Number of half-moves is counted
            return fifty >= fifty_max;
        }

        private ulong refresh_hash()
        {
            ulong hash = 0;

            for (int i = Sq.a8; i <= Sq.a1; i++)
            {
                if (Char.IsLower(squares[i]) || Char.IsUpper(squares[i]))
                {
                    hash ^= Zobrist.getvalue(i, squares[i]);
                }
            }

            if (!white_to_move) hash ^= Zobrist.getblack();
            if (ep_idx != -1) hash ^= Zobrist.getep(ep_idx);

            if (white_a_moved) hash ^= Zobrist.castle_a_moved(true);
            if (white_h_moved) hash ^= Zobrist.castle_h_moved(true);
            if (black_a_moved) hash ^= Zobrist.castle_a_moved(false);
            if (black_h_moved) hash ^= Zobrist.castle_h_moved(false);

            return hash;
        }

        // construct start position
        public Position()
        {
            squares = new char[Sq.start_position.Length];
            Array.Copy(Sq.start_position, squares, Sq.start_position.Length);
            white_to_move = true;
            ep_idx = -1;
            white_a_moved = white_h_moved = black_a_moved = black_h_moved = false;
            fifty = 0;
            fullmove = 1;
            material_updated = false;

            white_king_idx = Sq.e1;
            black_king_idx = Sq.e8;

            hash = refresh_hash();
            threefold_hist = new Position[threefold_hist_max];
            threefold_hist_len = 0;
        }

        // construct copy of position
        public Position(Position pos, Boolean add_to_hist)
        {
            squares = new char[pos.squares.Length];
            Array.Copy(pos.squares, squares, pos.squares.Length);
            white_to_move = pos.white_to_move;
            ep_idx = pos.ep_idx;
            white_a_moved = pos.white_a_moved;
            white_h_moved = pos.white_h_moved;
            black_a_moved = pos.black_a_moved;
            black_h_moved = pos.black_h_moved;
            fifty = pos.fifty;
            fullmove = pos.fullmove;
            material_updated = pos.material_updated;

            white_king_idx = pos.white_king_idx;
            black_king_idx = pos.black_king_idx;

            hash = pos.hash;
            threefold_hist = new Position[threefold_hist_max];
            Array.Copy(pos.threefold_hist, threefold_hist, pos.threefold_hist_len);
            threefold_hist_len = pos.threefold_hist_len;

            if (add_to_hist)
            {
                threefold_hist[threefold_hist_len] = pos;
                threefold_hist_len++;
            }
        }

        // setup position
        public void Fen(string pos, string color, string castle, string ep, string fifty, string fullmove)
        {
            // position
            int idx = Sq.a8;
            for (int i = 0; i < pos.Length; i++)
            {
                if (pos[i] == 'k') black_king_idx = idx;
                if (pos[i] == 'K') white_king_idx = idx;

                if (pos[i] == '/') idx += 2;
                else if (pos[i] >= '1' && pos[i] <= '8')
                {
                    for (int j = 0; j < pos[i] - '0'; j++) squares[idx++] = ' ';
                }
                else squares[idx++] = pos[i];
            }

            // color
            white_to_move = (color == "w" || color == "W");

            // castle
            white_a_moved = white_h_moved = black_a_moved = black_h_moved = true;

            if (castle.LastIndexOf('K') >= 0) white_h_moved = false;
            if (castle.LastIndexOf('Q') >= 0) white_a_moved = false;
            if (castle.LastIndexOf('k') >= 0) black_h_moved = false;
            if (castle.LastIndexOf('q') >= 0) black_a_moved = false;

            // en-passant
            ep_idx = (ep == "-") ? -1 : Sq.rcidx(ep[0]) + Sq.rcidx(ep[1]);

            // fifty rule count
            this.fifty = Convert.ToInt32(fifty);

            // full move being played
            this.fullmove = Convert.ToInt32(fullmove);

            // refresh hash
            hash = refresh_hash();
            threefold_hist = new Position[threefold_hist_max];
            threefold_hist_len = 0;
        }

        // play legal(!) move (from movelist) and update position
        public void Play(string move)
        {
            // move piece from square to square
            int idx_from = Sq.rcidx(move[0]) + Sq.rcidx(move[1]);
            int idx_to = Sq.rcidx(move[2]) + Sq.rcidx(move[3]);

            // moved piece and possible captured piece
            char piece_from = squares[idx_from];
            char piece_to = squares[idx_to];

            // default set when something is captured
            material_updated = (piece_to != ' ');
            if (material_updated) hash ^= Zobrist.getvalue(idx_to, piece_to);

            // king movement
            if (idx_from == white_king_idx) white_king_idx = idx_to;
            if (idx_from == black_king_idx) black_king_idx = idx_to;

            // en passant
            if ((piece_from == 'P' || piece_from == 'p') && move[0] != move[2] && piece_to == ' ')
            {
                int idx_ep = Sq.rcidx(move[2]) + Sq.rcidx(move[1]);
                hash ^= Zobrist.getvalue(idx_ep, squares[idx_ep]);
                squares[idx_ep] = ' ';

                // a pawn is captured
                material_updated = true;
            }

            // castle
            if (piece_from == 'K')
            {
                if (move == "e1g1")
                {
                    hash ^= Zobrist.getvalue(Sq.h1, squares[Sq.h1]);
                    squares[Sq.f1] = squares[Sq.h1];
                    squares[Sq.h1] = ' ';
                    hash ^= Zobrist.getvalue(Sq.f1, squares[Sq.f1]);
                }
                if (move == "e1c1")
                {
                    hash ^= Zobrist.getvalue(Sq.a1, squares[Sq.a1]);
                    squares[Sq.d1] = squares[Sq.a1];
                    squares[Sq.a1] = ' ';
                    hash ^= Zobrist.getvalue(Sq.d1, squares[Sq.d1]);
                }
            }
            if (piece_from == 'k')
            {
                if (move == "e8g8")
                {
                    hash ^= Zobrist.getvalue(Sq.h8, squares[Sq.h8]);
                    squares[Sq.f8] = squares[Sq.h8];
                    squares[Sq.h8] = ' ';
                    hash ^= Zobrist.getvalue(Sq.f8, squares[Sq.f8]);
                }
                if (move == "e8c8")
                {
                    hash ^= Zobrist.getvalue(Sq.a8, squares[Sq.a8]);
                    squares[Sq.d8] = squares[Sq.a8];
                    squares[Sq.a8] = ' ';
                    hash ^= Zobrist.getvalue(Sq.d8, squares[Sq.d8]);
                }
            }

            // castle ability
            if (!white_a_moved && (idx_from == Sq.a1 || idx_to == Sq.a1 || idx_from == Sq.e1 || idx_to == Sq.e1))
            {
                white_a_moved = true;
                hash ^= Zobrist.castle_a_moved(true);
            }
            if (!white_h_moved && (idx_from == Sq.h1 || idx_to == Sq.h1 || idx_from == Sq.e1 || idx_to == Sq.e1))
            {
                white_h_moved = true;
                hash ^= Zobrist.castle_h_moved(true);
            }
            if (!black_a_moved && (idx_from == Sq.a8 || idx_to == Sq.a8 || idx_from == Sq.e8 || idx_to == Sq.e8))
            {
                black_a_moved = true;
                hash ^= Zobrist.castle_a_moved(false);
            }
            if (!black_h_moved && (idx_from == Sq.h8 || idx_to == Sq.h8 || idx_from == Sq.e8 || idx_to == Sq.e8))
            {
                black_h_moved = true;
                hash ^= Zobrist.castle_h_moved(false);
            }

            // update 50 move rule count
            fifty++;
            if (piece_to != ' ' || piece_from == (white_to_move ? 'P' : 'p'))
            {
                fifty = 0;

                // also reset threefold repetition history
                // because any preceeding position cannot reoccur in the future
                threefold_hist_len = 0;
            }

            // pawn double jump
            if (ep_idx != -1) hash ^= Zobrist.getep(ep_idx);
            ep_idx = -1;
            if (piece_from == 'p' && idx_to == idx_from + 20 ||
                piece_from == 'P' && idx_to == idx_from - 20)
            {
                // keep square in between for en-passant
                ep_idx = (idx_from + idx_to) / 2;
                hash ^= Zobrist.getep(ep_idx);
            }

            // move piece
            hash ^= Zobrist.getvalue(idx_from, piece_from);
            squares[idx_to] = piece_from;
            squares[idx_from] = ' ';

            // promoted
            if (piece_from == 'P' && move[3] == '8')
            {
                if (move[4] == 'q') squares[idx_to] = 'Q';
                if (move[4] == 'r') squares[idx_to] = 'R';
                if (move[4] == 'b') squares[idx_to] = 'B';
                if (move[4] == 'n') squares[idx_to] = 'N';

                // a pawn is promoted
                material_updated = true;
            }
            if (piece_from == 'p' && move[3] == '1')
            {
                if (move[4] == 'q') squares[idx_to] = 'q';
                if (move[4] == 'r') squares[idx_to] = 'r';
                if (move[4] == 'b') squares[idx_to] = 'b';
                if (move[4] == 'n') squares[idx_to] = 'n';

                // a pawn is promoted
                material_updated = true;
            }

            hash ^= Zobrist.getvalue(idx_to, squares[idx_to]);

            // if black move done then full move increment
            if (!white_to_move) fullmove++;

            // toggle player
            white_to_move = !white_to_move;
            hash ^= Zobrist.getblack();
        }


        // material has been updated on last move
        public Boolean MaterialUpdated()
        {
            return material_updated;
        }

        // white to move or not
        public Boolean whitetomove()
        {
            return white_to_move;
        }

        // get piece at square
        public char pieceat(int idx)
        {
            return squares[idx];
        }

        // castle stuff
        public Boolean get_a_moved(Boolean white)
        {
            return white ? white_a_moved : black_a_moved;
        }
        public Boolean get_h_moved(Boolean white)
        {
            return white ? white_h_moved : black_h_moved;
        }

        // hashing stuff
        public ulong gethash()
        {
            return hash;
        }


        // BEGIN OF MOVELIST PART

        // Just to get a list with legal moves from the position.
        // A simple ordering based on material gain, move direction and castling is used

        // play move (and take back) to check if it's legal
        private Boolean im_check_after_move(int idx_from, int idx_to, string promotion, int from2, int to2)
        {
            // play move
            char tmp_removed_piece = squares[idx_to];
            squares[idx_to] = squares[idx_from];
            squares[idx_from] = ' ';

            // NOTE: ignore promotion, let it stay a pawn
            // it doesn't matter for the result, but it does matter for the undo

            // special case when en-passant or castle, remove to2 (from2 is 0) or swap (from2 > 0)
            char piece2 = ' ';
            if (to2 > 0)
            {
                piece2 = squares[to2];
                squares[to2] = (from2 > 0) ? squares[from2] : ' ';
                if (from2 > 0) squares[from2] = ' ';
            }

            // king movement
            if (idx_from == white_king_idx) white_king_idx = idx_to;
            if (idx_from == black_king_idx) black_king_idx = idx_to; 

            // move is done, check if we are in check
            // if so then the move is illegal
            Boolean im_check = check();

            // undo king movement
            if (idx_to == white_king_idx) white_king_idx = idx_from;
            if (idx_to == black_king_idx) black_king_idx = idx_from;

            // undo special case
            if (to2 > 0)
            {
                if (from2 > 0) squares[from2] = squares[to2];
                squares[to2] = piece2;
            }

            // undo play move
            squares[idx_from] = squares[idx_to];
            squares[idx_to] = tmp_removed_piece;

            return im_check;
        }

        // add move + basic score to the list, but only if it's legal 
        private void addmove(int idx_from, int idx_to, string promotion, int from2, int to2)
        {
            // if the move is illegal then skip
            if (im_check_after_move(idx_from, idx_to, promotion, from2, to2))
            {
                return;
            }

            // special capture
            Boolean enpassant = (from2 == 0 && to2 > 0);

            // give move a basic score
            int score = Evaluator.basic_move_score(this, idx_from, idx_to, promotion, enpassant);

            // add move to list
            string move = new StringBuilder().Append(Sq.sq[idx_from]).Append(Sq.sq[idx_to]).Append(promotion).ToString();
            moves.add(String.Intern(move), score);
        }

        private Boolean enemy_piece(char piece)
        {
            return white_to_move ? Char.IsLower(piece) : Char.IsUpper(piece);
        }

        // find availabe moves for queen, rook, bishop
        private void handle_direction(int i, int factor)
        {
            int ix = i;
            char piece;
            do
            {
                ix += factor;
                piece = squares[ix];
                if (piece == ' ' || enemy_piece(piece))
                {
                    addmove(i, ix, "", 0, 0);
                }
            } while (piece == ' ');
        }

        // find availabe move for knight, king
        private void handle_one(int i, int factor)
        {
            int ix = i + factor;
            char piece = squares[ix];
            if (piece == ' ' || enemy_piece(piece))
            {
                addmove(i, ix, "", 0, 0);
            }
        }

        // construct list with possible moves in this position
        public Moves getMoves()
        {
            // some help variables to make this color independant
            int pawn_row = white_to_move ? Sq.white_2nd_row : Sq.black_2nd_row;
            int prom_row = white_to_move ? Sq.white_7th_row : Sq.black_7th_row;
            int plm = white_to_move ? -1 : 1;
            int plm_9 = white_to_move ? -9 : 9;
            int plm_10 = white_to_move ? -10 : 10;
            int plm_11 = white_to_move ? -11 : 11;
            int plm_20 = white_to_move ? -20 : 20;

            // list of possible moves (110 is max?)
            moves = new Moves();

            // visit all squares
            for (int i = Sq.a8; i <= Sq.h1; i++)
            {
                char ch = squares[i];

                // empty square, outside board, wrong color
                if (ch == ' ' || ch == '#' || enemy_piece(ch))
                {
                    continue;
                }

                // pawn
                if (ch == (white_to_move ? 'P' : 'p'))
                {
                    // advance one row
                    if (squares[i + plm_10] == ' ')
                    {
                        if (i >= prom_row + 1 && i <= prom_row + 8)
                        {
                            // promotion
                            addmove(i, i + plm_10, "q", 0, 0);
                            addmove(i, i + plm_10, "r", 0, 0);
                            addmove(i, i + plm_10, "b", 0, 0);
                            addmove(i, i + plm_10, "n", 0, 0);
                        }
                        else
                        {
                            addmove(i, i + plm_10, "", 0, 0);
                        }
                    }

                    // advance two rows
                    if (i >= pawn_row + 1 && i <= pawn_row + 8 && squares[i + plm_10] == ' ' && squares[i + plm_20] == ' ')
                    {
                        addmove(i, i + plm_20, "", 0, 0);
                    }

                    // capture
                    char piece = squares[i + plm_9];
                    if (enemy_piece(piece))
                    {
                        if (i >= prom_row + 1 && i <= prom_row + 8)
                        {
                            // promotion
                            addmove(i, i + plm_9, "q", 0, 0);
                            addmove(i, i + plm_9, "r", 0, 0);
                            addmove(i, i + plm_9, "b", 0, 0);
                            addmove(i, i + plm_9, "n", 0, 0);
                        }
                        else
                        {
                            addmove(i, i + plm_9, "", 0, 0);
                        }
                    }
                    piece = squares[i + plm_11];
                    if (enemy_piece(piece))
                    {
                        if (i >= prom_row + 1 && i <= prom_row + 8)
                        {
                            // promotion
                            addmove(i, i + plm_11, "q", 0, 0);
                            addmove(i, i + plm_11, "r", 0, 0);
                            addmove(i, i + plm_11, "b", 0, 0);
                            addmove(i, i + plm_11, "n", 0, 0);
                        }
                        else
                        {
                            addmove(i, i + plm_11, "", 0, 0);
                        }
                    }

                    // en-passant
                    if (ep_idx != -1)
                    {
                        if (ep_idx == i + plm_9)
                        {
                            addmove(i, ep_idx, "", 0, i - plm);
                        }
                        if (ep_idx == i + plm_11)
                        {
                            addmove(i, ep_idx, "", 0, i + plm);
                        }
                    }

                    continue;
                }

                // knight
                if (ch == (white_to_move ? 'N' : 'n'))
                {
                    // possible jumps
                    handle_one(i, -21);
                    handle_one(i, -19);
                    handle_one(i, -12);
                    handle_one(i, -8);
                    handle_one(i, 8);
                    handle_one(i, 12);
                    handle_one(i, 19);
                    handle_one(i, 21);

                    continue;
                }

                // bishop
                if (ch == (white_to_move ? 'B' : 'b'))
                {
                    //diagonals
                    handle_direction(i, -11);
                    handle_direction(i, 11);
                    handle_direction(i, -9);
                    handle_direction(i, 9);

                    continue;
                }

                // rook
                if (ch == (white_to_move ? 'R' : 'r'))
                {
                    //rows and columns
                    handle_direction(i, -10);
                    handle_direction(i, 10);
                    handle_direction(i, -1);
                    handle_direction(i, 1);

                    continue;
                }

                // queen
                if (ch == (white_to_move ? 'Q' : 'q'))
                {
                    //diagonals
                    handle_direction(i, -11);
                    handle_direction(i, 11);
                    handle_direction(i, -9);
                    handle_direction(i, 9);

                    //rows and columns
                    handle_direction(i, -10);
                    handle_direction(i, 10);
                    handle_direction(i, -1);
                    handle_direction(i, 1);

                    continue;
                }

                // king
                if (ch == (white_to_move ? 'K' : 'k'))
                {
                    //diagonals
                    handle_one(i, -11);
                    handle_one(i, 11);
                    handle_one(i, -9);
                    handle_one(i, 9);

                    //rows and columns
                    handle_one(i, -10);
                    handle_one(i, 10);
                    handle_one(i, -1);
                    handle_one(i, 1);
                    
                    // castle
                    if (i == (white_to_move ? Sq.e1 : Sq.e8))
                    {
                        Boolean castle_a_moved = white_to_move ? white_a_moved : black_a_moved;
                        Boolean castle_h_moved = white_to_move ? white_h_moved : black_h_moved;

                        // long
                        if (!castle_a_moved && squares[i - 1] == ' ' && squares[i - 2] == ' ' && squares[i - 3] == ' ')
                        {
                            if (!square_attacked(i) && !square_attacked(i - 1))
                            {
                                addmove(i, i - 2, "", i - 4, i - 1);
                            }
                        }
                        // short
                        if (!castle_h_moved && squares[i + 1] == ' ' && squares[i + 2] == ' ')
                        {
                            if (!square_attacked(i) && !square_attacked(i + 1))
                            {
                                addmove(i, i + 2, "", i + 3, i + 1);
                            }
                        }
                    }

                    continue;
                }
            }

            return moves;
        }

        // END OF MOVELIST PART
    }
}
