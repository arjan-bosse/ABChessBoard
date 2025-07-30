using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Keep list of possible moves.
    /// Keep score per move.
    /// Sort moves based on their scores, hightest first.
    /// </summary>
    public class Moves
    {
        private static int MAX = 128;
        private string[] move;
        private int[] score;

        public int length { get; private set; }

        public Moves()
        {
            move = new string[MAX];
            score = new int[MAX];
            length = 0;
        }

        // Insert move in list sorted on score.
        public void add(string move, int score)
        {
            if (length == MAX)
            {
                if (this.score[length - 1] >= score) return;
                length--;
            }
            int j;
            for (j = length; j > 0; j--)
            {
                if (this.score[j - 1] >= score) break;
                this.score[j] = this.score[j - 1];
                this.move[j] = this.move[j - 1];
            }
            this.score[j] = score;
            this.move[j] = move;
            length++;
        }

        public void set_score(int i, int score)
        {
            if (i >= 0 && i < length) this.score[i] = score;
        }

        public string get(int i)
        {
            if (i >= 0 && i < length) return move[i];
            return "";
        }

        // Sort list on score, this shouldn't be called often so bubble sort is OK. 
        public void sort_score()
        {
            for (int j = 0; j < length - 1; j++)
            {
                for (int i = length - 1; i > j; i--)
                {
                    if (score[i] > score[i - 1])
                    {
                        string s = move[i];
                        move[i] = move[i - 1];
                        move[i - 1] = s;

                        int t = score[i];
                        score[i] = score[i - 1];
                        score[i - 1] = t;
                    }
                }
            }
        }
    }
}
