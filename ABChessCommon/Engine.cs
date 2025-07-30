using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Calculate best move for Position.
    /// The position and parameters (depth, infinite, report object) are received from the calling interface.
    /// Information is reported back.
    /// The calculated best move can also be read later, e.g. after stop.
    /// </summary>
    public class Engine
    {
        // engine settings
        private Boolean stop;
        private string all_best_move;
        private Boolean infinite;
        private int default_max_depth;
        private long nodes;
        private DateTime start;
        private EngineReport output;

        // transpositions
        private Boolean do_hash = false;
        private HashEntry[] ttable = null;
        private ulong mask;
        private long ttable_filled;
        private long ttable_read;
        private long ttable_not_read;
        private long ttable_collisions;
        private long ttable_hash_collisions;
        private long ttable_depth_collisions;

        // Global functions to receive parameters/commands

        // constructor
        public Engine()
        {
            output = null;
        }

        public Engine(EngineReport output)
        {
            this.output = output;
        }

        // search depth in plies
        public void Depth(int depth)
        {
            default_max_depth = depth;
        }

        // run infinite or not
        public void Infinite(Boolean infinite)
        {
            this.infinite = infinite;
        }

        // stop calculating
        public void Stop()
        {
            stop = true;
        }

        // get bestmove in case of a stopped infinite search
        public string getBestMove()
        {
            return all_best_move;
        }

        // Local functions for info reporting
        private void Report(string line)
        {
            if (output != null) output.Report(line);
        }
        private void Percentage(int percentage)
        {
            if (output != null) output.Percentage(percentage);
        }

        // nodes
        private void info_nodes()
        {
            nodes++;
            if (nodes % Value.info_per_nodes == 0)
            {
                DateTime now = DateTime.Now;
                TimeSpan span = now - start;
                long ms = (long)span.TotalMilliseconds;
                long nps = ms > 0 ? 1000 * nodes / ms : 0;
                Report("info nodes " + nodes + " nps " + nps);
            }
        }

        // bestline
        private void info_bestline(int depth, Score sa)
        {
            DateTime now = DateTime.Now;
            TimeSpan span = now - start;
            long ms = (long)span.TotalMilliseconds;
            long nps = ms > 0 ? 1000 * nodes / ms : 0;
            Report("info depth " + depth + " time " + ms + " nodes " + nodes + " nps " + nps + " score cp " + sa.getScore() + " pv " + sa.getMoves());
        }

        // report bestmove in case of a finished finite search
        private void play_bestmove()
        {
            if (!infinite) Report("bestmove " + all_best_move);
        }

        // Local functions to support CALCULATE BESTMOVE

        private void tposition_reset()
        {
            if (!do_hash) return;

            // start at tree depth 3
            // only from depth 3 and on transpositions can occur

            if (ttable != null)
            {
                Report("info string filled " + ttable_filled + " read " + ttable_read + " not_read " + ttable_not_read + " collisions " + ttable_collisions + " hash_collisions " + ttable_hash_collisions + " depth_collisions " + ttable_depth_collisions);
            }

            mask = 1024 * 1024;
            ttable = new HashEntry[mask + 1000];

            ttable_filled = 0;
            ttable_read = 0;
            ttable_not_read = 0;
            ttable_collisions = 0;
            ttable_hash_collisions = 0;
            ttable_depth_collisions = 0;
        }

        private Score tposition_lookup(int tdepth, Position position)
        {
            if (!do_hash) return null;

            // find position and score
            if (tdepth >= 3)
            {
                int uidx = (int)(position.gethash() % mask);
                for (int i = uidx; i < ttable.Length; i++)
                {
                    HashEntry h = ttable[i];
                    if (h == null) break;
                    if (h.tdepth > tdepth) { ttable_depth_collisions++; continue; }
                    if (position.gethash() != h.position.gethash()) { ttable_hash_collisions++; continue; }
                    if (position.equals(h.position)) { ttable_read++; return h.score; }
                    ttable_collisions++;
                }
                ttable_not_read++;
            }
            return null;
        }

        private void tposition_insert(int tdepth, Position position, Score score)
        {
            if (!do_hash) return;

            // keep position and score
            if (tdepth >= 3 && tdepth <= 6)
            {
                int uidx = (int)(position.gethash() % mask);
                for (int i = uidx; i < ttable.Length; i++)
                {
                    HashEntry h = ttable[i];
                    if (h == null)
                    {
                        ttable[i] = new HashEntry(position, score, tdepth); ;
                        ttable_filled++;
                        break;
                    }
                    if (h.tdepth > tdepth && h.position.gethash() == position.gethash() && position.equals(h.position))
                    {
                        ttable[i] = new HashEntry(position, score, tdepth);
                        break;
                    }
                }
            }
            return;
        }

        // search all unquiet positions until non-material-updated depth 
        private Score quiescence_eval(Position position, int alpha, int beta, int depth, int tdepth)
        {
            // if threefold repetition or fifty moves rule then return draw
            if (position.threefold_repetition() || position.fifty_move_rule())
            {
                return new Score(0);
            }

            // transposition lookup
            Score tsc = tposition_lookup(tdepth, position);
            if (tsc != null) return tsc;

            // get list with possible moves
            Moves moves = position.getMoves();

            // mate or stalemate
            if (moves.length == 0)
            {
                return new Score(position.check() ? -Value.mate : 0);
            }

            int eval_score = Evaluator.evaluate(position);

            if (depth == 0)
            {
                return new Score(eval_score);
            }

            if (eval_score >= beta) return new Score(beta);
            if (eval_score > alpha) alpha = eval_score;

            Boolean im_check = position.check();

            Score sa = new Score(alpha);

            for (int m = 0; m < moves.length && !stop; m++)
            {
                string move = moves.get(m);
                Position pos = new Position(position, true);
                pos.Play(move);

                // only proceed if position is not quiet
                if (im_check || pos.MaterialUpdated() || pos.check())
                {
                    // reset depth when material has been updated
                    int new_depth = moves.length > 1 ? depth - 1 : depth;
                    if (pos.MaterialUpdated()) new_depth = Value.quiescence_depth;

                    Score sc = quiescence_eval(pos, -beta, -alpha, new_depth, tdepth + 1);
                    int score = -sc.getScore();
                    info_nodes();

                    if (score >= beta) return new Score(beta);
                    if (score > alpha) { alpha = score; sa = sc; sa.add(move, alpha);  }
                }
            }

            return sa;
        }

        // full search until depth, then start quiescence search
        private Score eval(Position position, int alpha, int beta, int depth, int tdepth)
        {
            // if threefold repetition or fifty moves rule then return draw
            if (position.threefold_repetition() || position.fifty_move_rule())
            {
                return new Score(0);
            }

            // transposition lookup
            Score tsc = tposition_lookup(tdepth, position);
            if (tsc != null) return tsc;

            if (depth == 0)
            {
                Score sc = quiescence_eval(position, alpha, beta, Value.quiescence_depth, tdepth);
                tposition_insert(tdepth, position, sc);
                return sc;
            }

            // get list with possible moves
            Moves moves = position.getMoves();

            // mate or stalemate
            if (moves.length == 0)
            {
                Score sc = new Score(position.check() ? -Value.mate : 0);
                tposition_insert(tdepth, position, sc);
                return sc;
            }

            Score sa = new Score(alpha);

            for (int m = 0; m < moves.length && !stop; m++)
            {
                string move = moves.get(m);
                Position pos = new Position(position, true);
                pos.Play(move);

                Score sc = eval(pos, -beta, -alpha, moves.length > 1 ? depth - 1 : depth, tdepth + 1);
                int score = -sc.getScore();
                info_nodes();

                if (score >= beta) { Score sb = new Score(beta); tposition_insert(tdepth, position, sb); return sb; }
                if (score > alpha) { alpha = score; sa = sc; sa.add(move, alpha); }
            }

            tposition_insert(tdepth, position, sa);
            return sa;
        }

        // CALCULATE BESTMOVE
        //
        // Calculate best move using alpha-beta, quiescence and iterative deepening
        // Information is written to the interface
        //
        // At least one move should be possible...
        //
        // Chess specific functions like evaluation and generating list of moves
        // are delegated to the position
        // 
        public void calculateBestMove(Position position)
        {
            // initialise
            stop = false;
            all_best_move = "0000";
            nodes = 0;
            start = DateTime.Now;

            // if threefold repetition or fifty moves rule then play nullmove
            if (position.threefold_repetition() || position.fifty_move_rule())
            {
                play_bestmove();
                return;
            }

            // get list with possible moves
            Moves moves = position.getMoves();

            // if no moves then play nullmove
            if (moves.length == 0)
            {
                play_bestmove();
                return;
            }

            // if only one move then just play it
            if (moves.length == 1)
            {
                if (!stop) all_best_move = moves.get(0);
                play_bestmove();
                return;
            }

            // iterative deepening
            int max_depth = default_max_depth;

            int depth;
            Score all_best_score = new Score(-Value.max);

            // actual node depth in tree
            int tdepth = 1;

            for (depth = 1; (depth <= max_depth || infinite) && !stop; depth++)
            {
                int alpha = -Value.max;
                int beta = Value.max;
                string best_move = "";

                tposition_reset();

                Score sa = new Score(alpha);

                for (int m = 0; m < moves.length && !stop; m++)
                {
                    string move = moves.get(m);
                    Position pos = new Position(position, true);
                    pos.Play(move);

                    // only report when working at given depth (or beyond)
                    if (depth >= max_depth) Percentage((m + 1) * 100 / moves.length);
                    if (depth >= max_depth) Report("info currmove " + move);

                    Score sc = eval(pos, -beta, -alpha, depth - 1, tdepth + 1);

                    int score = -sc.getScore();
                    info_nodes();

                    moves.set_score(m, score);

                    if (score > alpha)
                    {
                        alpha = score; sa = sc; sa.add(move, alpha);
                        best_move = move;

                        info_bestline(depth, sa);

                        if (alpha >= Value.mate) break;
                    }
                }

                if (!stop)
                {
                    all_best_move = best_move;
                    all_best_score = sa;
                }

                if (alpha >= Value.mate) { depth++; break; }

                // sort for next iteration
                moves.sort_score();
            }

            // send latest statistics
            Report("info currmove " + all_best_move);
            info_bestline(depth - 1, all_best_score);

            tposition_reset();

            // report best move (if needed) 
            play_bestmove();
            return;
        }
    }
}
