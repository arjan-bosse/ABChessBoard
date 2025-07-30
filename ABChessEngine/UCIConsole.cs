using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

// ABChess (command line engine)
//
// Version: 0.1 (original)
// Date   : 2015-08-15
// Author : Arjan Bosse
//
// Version: 0.2 (fix)
// Date   : 2016-11-06
// Author : Arjan Bosse

namespace ABChessEngine
{
    /// <summary>
    /// Implementation (subset) of the UCI interface
    ///
    /// On Position create an Engine, initialize fen and/or play moves
    /// On Go let the Engine calculate the best move in a worker thread, parameters are infinite and/or depth
    /// On Stop (and Go had infinite parameter) the best move is read from the Engine,
    /// otherwise the best move is written when the worker finishes
    ///
    /// The Engine shows info about depth, nodes, time, nodes per second, best line, current move (at depth),
    /// and also the best move in case of a finished finite search
    /// </summary>
    public class UCIConsole
    {
        private Boolean running = false;
        private Thread worker;
        private ABChessCommon.Position position;

        // engine
        private ABChessCommon.Engine engine;
        private Boolean infinite = false;
        private int depth = ABChessCommon.Value.search_depth;

        // Starting point of new thread.
        private void threadStarted()
        {
            engine.calculateBestMove(position);
        }

        private void startCalculate()
        {
            // Start running worker.
            if (!running)
            {
                running = true;
                engine.Infinite(infinite);
                engine.Depth(depth);
                worker = new Thread(new ThreadStart(threadStarted));
                worker.Start();
            }
        }

        private void stopCalculate()
        {
            // Stop running worker.
            if (running)
            {
                running = false;
                engine.Stop();
                worker.Join();
            }
        }

        private void HandlePosition(string[] words)
        {
            if (words.Length >= 8 && words[1] == "fen")
            {
                position.Fen(words[2], words[3], words[4], words[5], words[6], words[7]);

                if (words.Length >= 9 && words[8] == "moves")
                {
                    for (int i = 9; i < words.Length; i++)
                    {
                        ABChessCommon.Position pos = new ABChessCommon.Position(position, true);
                        pos.Play(words[i]);
                        position = pos;
                    }
                }
            }
            if (words.Length >= 2 && words[1] == "startpos")
            {
                if (words.Length >= 3 && words[2] == "moves")
                {
                    for (int i = 3; i < words.Length; i++)
                    {
                        ABChessCommon.Position pos = new ABChessCommon.Position(position, true);
                        pos.Play(words[i]);
                        position = pos;
                    }
                }
            }
        }

        private void HandleGo(string[] words)
        {
            infinite = false;
            depth = ABChessCommon.Value.search_depth;

            for (int i = 1; i < words.Length; i++)
            {
                switch (words[i])
                {
                    case "searchmoves":
                        break;
                    case "ponder":
                        break;
                    case "wtime":
                        i++;
                        break;
                    case "btime":
                        i++;
                        break;
                    case "winc":
                        i++;
                        break;
                    case "binc":
                        i++;
                        break;
                    case "movestogo":
                        i++;
                        break;
                    case "depth":
                        i++;
                        depth = Int32.Parse(words[i]);
                        break;
                    case "nodes":
                        i++;
                        break;
                    case "mate":
                        i++;
                        break;
                    case "movetime":
                        i++;
                        break;
                    case "infinite":
                        infinite = true;
                        break;
                }
            }
        }

        // Start handling UCI commands.
        public void Start()
        {
            while (true)
            {
                // Read words from command line.
                string input = Console.ReadLine();
                string[] separators = { " " };
                string[] words = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length == 0)
                {
                    continue;
                }
                if (words[0] == "uci")
                {
                    Console.WriteLine("id name AB Chess Engine 0.2");
                    Console.WriteLine("id author Arjan Bosse");
                    Console.WriteLine("uciok");
                    continue;
                }
                if (words[0] == "debug")
                {
                    continue;
                }
                if (words[0] == "isready")
                {
                    Console.WriteLine("readyok");
                    continue;
                }
                if (words[0] == "setoption")
                {
                    continue;
                }
                if (words[0] == "register")
                {
                    continue;
                }
                if (words[0] == "ucinewgame")
                {
                    stopCalculate();
                    Console.WriteLine("readyok");
                    continue;
                }
                if (words[0] == "position")
                {
                    stopCalculate();
                    engine = new ABChessCommon.Engine(new UCIReport());
                    position = new ABChessCommon.Position();
                    HandlePosition(words);
                    continue;
                }
                if (words[0] == "go")
                {
                    HandleGo(words);
                    startCalculate();
                    continue;
                }
                if (words[0] == "stop")
                {
                    stopCalculate();
                    if (infinite)
                    {
                        string bestmove = engine.getBestMove();
                        Console.WriteLine("bestmove " + bestmove);
                    }
                    continue;
                }
                if (words[0] == "ponderhit")
                {
                    continue;
                }
                if (words[0] == "quit")
                {
                    stopCalculate();
                    break;
                }
            }
        }
    }
}
