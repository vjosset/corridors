using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    public static class Utils
    {
        private static Random rand = new Random();

        /// <summary>
        /// Shifts the characters in the provided string toward the right.
        /// Examples:
        /// * ShiftString("ABCD", 1) = "BCDA"
        /// * ShiftString("ABCD", 2) = "CDAB"
        /// </summary>
        /// <param name="str">The string to shift.</param>
        /// <param name="shift">The number of characters to shift.</param>
        /// <returns></returns>
        public static string ShiftString(string str, int shift)
        {
            return str.Substring(str.Length - shift, shift) + str.Substring(0, str.Length - shift);
        }

        /// <summary>
        /// Rotates the specified two-dimensional array clockwise.
        /// </summary>
        /// <param name="matrix">The two-dimensional array to rotate.</param>
        /// <returns></returns>
        public static Tile[,] RotateMatrix(Tile[,] matrix)
        {
            // Prepare the new tileset (flip the dimensions)
            Tile[,] newTiles = new Tile[matrix.GetLength(1), matrix.GetLength(0)];

            // Prepare the indices for the new tile coordinates
            int newCol, newRow = 0;

            // Loop through the old columns - Columns will become rows, and rows will become columns
            for (int oldCol = 0; oldCol < matrix.GetLength(1); oldCol++)
            {
                newCol = 0;
                // Loop through the old rows
                for (int oldRow = matrix.GetLength(0) - 1; oldRow >= 0; oldRow--)
                {
                    // Switch the positions (col/row) in the new tileset
                    newTiles[newRow, newCol] = matrix[oldRow, oldCol];

                    // Move to the next (new) column
                    newCol++;
                }
                newRow++;
            }

            // Update tiles to use our new tileset
            return newTiles;
        }

        /// <summary>
        /// Returns the path to the current executable.
        /// </summary>
        /// <returns></returns>
        public static string GetPathToExecutable()
        {
            return System.Reflection.Assembly.GetEntryAssembly().Location;
        }
        
        /// <summary>
        /// Returns the path to the current executable's parent folder.
        /// </summary>
        /// <returns></returns>
        public static string GetExecutableFolder()
        {
            string exePath = GetPathToExecutable();
            return exePath.Substring(0, exePath.LastIndexOf("\\"));
        }

        public static dynamic GetRandomArrayMember(dynamic[] array)
        {
            return array[rand.Next(0, array.Length)];
        }
    }
}
