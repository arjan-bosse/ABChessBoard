using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Keep best line of moves.
    /// Keep score of first (actually last) move in line.
    /// </summary>
    public class Score
    {
        private static int MAX = 64;
        private string[] move;
        private int score;
        private int length;

        public Score(string move, int score)
        {
            this.move = new string[MAX];
            this.move[0] = move;
            this.score = score;
            length = 1;
        }

        public Score(int score)
        {
            move = new string[MAX];
            this.score = score;
            length = 0;
        }

        public Score()
        {
            move = new string[MAX];
            score = 0;
            length = 0;
        }

        public void add(string move, int score)
        {
            if (length == MAX)
            {
                for (int i = 0; i < length - 1; i++)
                {
                    this.move[i] = this.move[i + 1];
                }
                length--;
            }

            this.move[length] = move;
            this.score = score;
            length++;
        }

        public int getScore()
        {
            return score;
        }

        public string getMoves()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = length; i > 0; i--)
            {
                sb.Append(move[i - 1]);
                if (i > 1) sb.Append(' ');
            }
            return sb.ToString();
        }
    }
}
