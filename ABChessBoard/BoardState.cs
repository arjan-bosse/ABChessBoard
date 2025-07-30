using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;

// ABChess (GUI logic behind)
//
// Version: 0.1 (original)
// Date   : 2015-08-15
// Author : Arjan Bosse
//
// Version: 0.2 (fix)
// Date   : 2016-11-06
// Author : Arjan Bosse

namespace ABChessBoard
{
    /// <summary>
    /// Model behind the ABChessBoard.
    /// It contains the position, a playlist and the state of the mouse clicking.
    /// Instantiates the engine for calculation in a worker thread. 
    /// Handle mouse and worker events from the BoardForm.
    /// Create and paint bitmaps for the BoardForm.
    /// </summary>
    public class BoardState
    {
        // callback to parent form
        private BoardForm form;

        private int squareWidth;
        private int squareHeight;

        private Bitmap board;
        private Bitmap promotion;

        private Bitmap WhiteRook;
        private Bitmap WhiteKnight;
        private Bitmap WhiteBishop;
        private Bitmap WhiteQueen;
        private Bitmap WhiteKing;
        private Bitmap WhitePawn;

        private Bitmap BlackRook;
        private Bitmap BlackKnight;
        private Bitmap BlackBishop;
        private Bitmap BlackQueen;
        private Bitmap BlackKing;
        private Bitmap BlackPawn;

        // position
        private ABChessCommon.Position position;
        private LinkedList<string> playlist;

        // selected square
        private int px;
        private int py;

        // engine
        private ABChessCommon.Engine engine;
        private Boolean infinite = false;
        private int depth = ABChessCommon.Value.search_depth;

        // Constructor
        public BoardState(int squareWidth, int squareHeight, BoardForm form)
        {
            this.form = form;
            this.squareWidth = squareWidth;
            this.squareHeight = squareHeight;

            // New game. No square is clicked.
            ResetPosition();
            px = py = -1;

            // Create bitmaps.
            CreatePieces();
            CreateBoard();
            CreatePromotion();
        }

        // Search depth used by the engine when calculating. 
        public int GetDepth()
        {
            return depth;
        }

        // Last played move. Add some information about the move count.
        public string LastMove()
        {
            int i = playlist.Count;
            if (i == 0) return "";
            return (i + 1) / 2 + (i % 2 == 1 ? ". " : "... ") + playlist.Last();
        }


        // WORKER EVENT HANDLING

        // Worker thread: create engine and start calculating move.
        public void DoWork(BoardReport boardReport)
        {
            engine = new ABChessCommon.Engine(boardReport);
            engine.Depth(depth);
            engine.Infinite(infinite);
            engine.calculateBestMove(position);
        }

        // Get calculated move from the engine.
        public void WorkDone()
        {
            string move = engine.getBestMove();

            ABChessCommon.Position pos = new ABChessCommon.Position(position, true);
            pos.Play(move);
            position = pos;

            playlist.AddLast(move);
        }


        // MOUSE EVENT HANDLING: BUTTONS AND SQUARES

        // Calculate next move.
        private Boolean NextMove()
        {
            ABChessCommon.Moves moves = position.getMoves();

            if (moves.length == 0)
            {
                form.EndOfGame(position.check() ? "CHECKMATE" : "STALEMATE");
                return false;
            }
            if (position.threefold_repetition())
            {
                form.EndOfGame("DRAW by threefold repetition");
                return false;
            }
            if (position.fifty_move_rule())
            {
                form.EndOfGame("DRAW by 50 move rule");
                return false;
            }

            // calculate in a worker thread, expect DoWork and WorkDone to be called
            return true;
        }

        // Takeback move.
        private void UndoMove()
        {
            if (playlist.Count > 0)
            {
                // replay game except last move
                playlist.RemoveLast();
                position = new ABChessCommon.Position();
                foreach (string move in playlist)
                {
                    ABChessCommon.Position pos = new ABChessCommon.Position(position, true);
                    pos.Play(move);
                    position = pos;
                }
            }
        }

        // New game.
        private void ResetPosition()
        {
            position = new ABChessCommon.Position();
            playlist = new LinkedList<string>();
        }

        // Button is clicked.
        // Change search depth, start or stop calculating, take back move or reset game.
        public Boolean HandleButton(string button)
        {
            Boolean dowork = false;

            switch (button)
            {
                case "+":
                    if (depth < 10) depth++;
                    break;
                case "-":
                    if (depth > 1) depth--;
                    break;
                case "start":
                    dowork = NextMove();
                    break;
                case "stop":
                    engine.Stop();
                    break;
                case "undo":
                    UndoMove();
                    break;
                case "reset":
                    if (playlist.Count > 0) ResetPosition();
                    break;
            }

            // reset pick
            px = py = -1;

            return dowork;
        }

        // Square is clicked.
        // Pick (piece on) square (left click), or
        // Drop piece on square (left click), play move (when legal) and start calculate next move, or
        // Takeback move (double left click), or
        // Calculate move (right click).
        public Boolean HandleMouse(Boolean left, Boolean right, int x, int y, int clicks)
        {
            Boolean dowork = false;

            // pick and drop: play human move
            if (left && clicks == 1)
            {
                if (px == -1 || py == -1)
                {
                    // new pick
                    px = x; py = y;
                }
                else
                {
                    if (x != px || y != py)
                    {
                        // it's a drop, construct move
                        string move = ABChessCommon.Sq.sq[px + 1 + 10 * (py + 2)] + ABChessCommon.Sq.sq[x + 1 + 10 * (y + 2)];

                        // possible promotion
                        char promotion = ' ';

                        // if legal then play
                        ABChessCommon.Moves moves = position.getMoves();

                        // End of game
                        Boolean end_of_game = false;
                        if (moves.length == 0)
                        {
                            form.EndOfGame(position.check() ? "CHECKMATE" : "STALEMATE");
                            end_of_game = true;
                        }
                        else if (position.threefold_repetition())
                        {
                            form.EndOfGame("DRAW by threefold repetition");
                            end_of_game = true;
                        }
                        else if (position.fifty_move_rule())
                        {
                            form.EndOfGame("DRAW by 50 move rule");
                            end_of_game = true;
                        }

                        for (int i = 0; i < moves.length && !end_of_game; i++)
                        {
                            // also handle possible promotion
                            if (moves.get(i).Substring(0, 4) == move)
                            {
                                // promotion
                                if (moves.get(i).Length == 5)
                                {
                                    if (promotion == ' ')
                                    {
                                        // ask for piece to promote
                                        promotion = form.Promotion(position.whitetomove());
                                    }
                                    if (moves.get(i)[4] != promotion)
                                    {
                                        // not the chosen piece
                                        continue;
                                    }
                                }

                                // play legal move
                                ABChessCommon.Position pos = new ABChessCommon.Position(position, true);
                                pos.Play(moves.get(i));
                                position = pos;

                                playlist.AddLast(moves.get(i));

                                // calculate and play next move
                                dowork = NextMove();
                                break;
                            }
                        }
                    }

                    // reset pick
                    px = py = -1;
                }
            }

            // calculate and play move
            if (right)
            {
                dowork = NextMove();

                // reset pick
                px = py = -1;
            }

            // take back last move
            if (left && clicks > 1)
            {
                UndoMove();

                // reset pick
                px = py = -1;
            }

            return dowork;
        }


        // BITMAP HANDLING: PAINT AND CREATE

        // Draw position squares and pieces on board.
        public Bitmap DrawPosition()
        {
            for (int idx = ABChessCommon.Sq.a8; idx <= ABChessCommon.Sq.h1; idx++)
            {
                char piece = position.pieceat(idx);

                if (piece == '#') continue;

                int x = idx % 10 - 1;
                int y = idx / 10 - 2;

                switch (piece)
                {
                    case ' ':
                        DrawPiece(x, y, null);
                        break;
                    case 'k':
                        DrawPiece(x, y, BlackKing);
                        break;
                    case 'q':
                        DrawPiece(x, y, BlackQueen);
                        break;
                    case 'r':
                        DrawPiece(x, y, BlackRook);
                        break;
                    case 'b':
                        DrawPiece(x, y, BlackBishop);
                        break;
                    case 'n':
                        DrawPiece(x, y, BlackKnight);
                        break;
                    case 'p':
                        DrawPiece(x, y, BlackPawn);
                        break;
                    case 'K':
                        DrawPiece(x, y, WhiteKing);
                        break;
                    case 'Q':
                        DrawPiece(x, y, WhiteQueen);
                        break;
                    case 'R':
                        DrawPiece(x, y, WhiteRook);
                        break;
                    case 'B':
                        DrawPiece(x, y, WhiteBishop);
                        break;
                    case 'N':
                        DrawPiece(x, y, WhiteKnight);
                        break;
                    case 'P':
                        DrawPiece(x, y, WhitePawn);
                        break;
                }
            }

            return board;
        }

        // Draw possible pieces for promotion.
        public Bitmap DrawPromotion(Boolean white)
        {
            using (Graphics graphics = Graphics.FromImage(promotion))
            {
                if (white)
                {
                    graphics.DrawImage(WhiteQueen, 0, 0, WhiteQueen.Width, WhiteQueen.Height);
                    graphics.DrawImage(WhiteRook, squareWidth, 0, WhiteRook.Width, WhiteRook.Height);
                    graphics.DrawImage(WhiteBishop, 2 * squareWidth, 0, WhiteBishop.Width, WhiteBishop.Height);
                    graphics.DrawImage(WhiteKnight, 3 * squareWidth, 0, WhiteKnight.Width, WhiteKnight.Height);
                }
                else
                {
                    graphics.DrawImage(BlackQueen, 0, 0, BlackQueen.Width, BlackQueen.Height);
                    graphics.DrawImage(BlackRook, squareWidth, 0, BlackRook.Width, BlackRook.Height);
                    graphics.DrawImage(BlackBishop, 2 * squareWidth, 0, BlackBishop.Width, BlackBishop.Height);
                    graphics.DrawImage(BlackKnight, 3 * squareWidth, 0, BlackKnight.Width, BlackKnight.Height);
                }
            }

            return promotion;
        }

        // Draw square color and piece at board.
        private void DrawPiece(int x, int y, Bitmap piece)
        {
            using (Graphics graphics = Graphics.FromImage(board))
            {
                // Coloring the squares.
                Brush brush = (x + y) % 2 == 0 ? Brushes.White : Brushes.DarkCyan;
                // Activated square.
                if (x == px && y == py) brush = Brushes.Yellow;

                graphics.FillRectangle(brush, new Rectangle(x * squareWidth, y * squareHeight, squareWidth, squareHeight));

                if (piece != null)
                {
                    // Draw the transparent bitmap to the screen.
                    graphics.DrawImage(piece, x * squareWidth, y * squareHeight, piece.Width, piece.Height);
                }
            }
        }

        // Extract transparant piece bitmap from template bitmap.
        private Bitmap CreatePiece(Bitmap pieces, int x, int y)
        {
            // Knowledge of predefined pieces bitmap.
            int pieceWidth = 64;
            int pieceHeight = 64;

            // Clone a portion of the pieces bitmap.
            Rectangle cloneRect = new Rectangle(
                (pieceWidth + 1) * x,
                pieceHeight * y,
                pieceWidth > squareWidth ? squareWidth : pieceWidth,
                pieceHeight > squareHeight ? squareHeight : pieceHeight);
            PixelFormat format = pieces.PixelFormat;
            Bitmap piece = pieces.Clone(cloneRect, format);

            // Get the color of a background pixel.
            Color backColor = pieces.GetPixel(1, 1);

            // Make background color transparent for piece.
            piece.MakeTransparent(backColor);

            return piece;
        }

        // Construct piece bitmaps.
        private void CreatePieces()
        {
            // Template bitmap with predefined pieces.
            Bitmap pieces = new Bitmap(ABChessBoard.Properties.Resources.images);

            WhiteRook = CreatePiece(pieces, 0, 0);
            WhiteKnight = CreatePiece(pieces, 1, 0);
            WhiteBishop = CreatePiece(pieces, 2, 0);
            WhiteQueen = CreatePiece(pieces, 3, 0);
            WhiteKing = CreatePiece(pieces, 4, 0);
            WhitePawn = CreatePiece(pieces, 5, 0);

            BlackRook = CreatePiece(pieces, 0, 1);
            BlackKnight = CreatePiece(pieces, 1, 1);
            BlackBishop = CreatePiece(pieces, 2, 1);
            BlackQueen = CreatePiece(pieces, 3, 1);
            BlackKing = CreatePiece(pieces, 4, 1);
            BlackPawn = CreatePiece(pieces, 5, 1);

            // Template bitmap not needed anymore.
            pieces.Dispose();
        }

        // Construct board bitmap.
        private void CreateBoard()
        {
            board = new Bitmap(squareWidth * 8, squareHeight * 8);
        }

        // Construct promotion bitmap.
        private void CreatePromotion()
        {
            promotion = new Bitmap(squareWidth * 4, squareHeight);
        }
    }
}
