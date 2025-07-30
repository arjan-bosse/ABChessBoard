using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABChessCommon
{
    /// <summary>
    /// Lookup (and create predefined) values for Zobrish hashing.
    /// </summary>
    public static class Zobrist
    {
        private static int[] chars;
        private static ulong[] table;

        private static ulong black_to_move;
        private static ulong[] ep_line;
        private static ulong white_a_moved;
        private static ulong white_h_moved;
        private static ulong black_a_moved;
        private static ulong black_h_moved;

        private static readonly Random rnd = new Random();

        // CREATE VALUES

        private static ulong rand()
        {
            byte[] buffer = new byte[8];
            rnd.NextBytes(buffer);
            return (ulong)BitConverter.ToInt64(buffer, 0);
        }

        private static void fill_chars()
        {
            chars = new int[256];
            chars['p'] = 0;
            chars['n'] = 1;
            chars['b'] = 2;
            chars['r'] = 3;
            chars['q'] = 4;
            chars['k'] = 5;
            chars['P'] = 6;
            chars['N'] = 7;
            chars['B'] = 8;
            chars['R'] = 9;
            chars['Q'] = 10;
            chars['K'] = 11;
        }

        private static void fill_table(int size)
        {
            table = new ulong[size];
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = rand();
                if (table[i] == 0)
                {
                    i--;
                    continue;
                }
                for (int j = 0; j < i; j++)
                {
                    if (table[j] == table[i])
                    {
                        i--;
                        break;
                    }
                }
            }
        }

        private static void fill_ep()
        {
            ep_line = new ulong[10];

            for (int i = 0; i < 10; i++)
            {
                ep_line[i] = table[table.Length - 1 - i];
            }
        }

        private static void fill_black()
        {
            black_to_move = table[table.Length - 11];
        }

        private static void fill_castle()
        {
            white_a_moved = table[table.Length - 12];
            white_h_moved = table[table.Length - 13];
            black_a_moved = table[table.Length - 14];
            black_h_moved = table[table.Length - 15];
        }

        // Constructor.
        // Create predefined values.
        static Zobrist()
        {
            fill_chars();
            fill_table((Sq.h1 - Sq.a8 + 1) * 12 + 15);
            fill_ep();
            fill_castle();
            fill_black();
        }

        // LOOKUP VALUES

        public static ulong getep(int ep_idx)
        {
            return ep_line[ep_idx % 10];
        }

        public static ulong castle_a_moved(Boolean white_to_move)
        {
            return white_to_move ? white_a_moved : black_a_moved;
        }

        public static ulong castle_h_moved(Boolean white_to_move)
        {
            return white_to_move ? white_h_moved : black_h_moved;
        }

        public static ulong getblack()
        {
            return black_to_move;
        }

        public static ulong getvalue(int idx, char piece)
        {
            int i = idx - Sq.a8;
            int p = chars[piece];
            return table[12 * i + p];
        }    
    }
}
