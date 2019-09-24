/*
 Copyright (C) 2019 TemporalAgent7 <https://github.com/TemporalAgent7>

 This file is part of the DataCore Bot open source project.

 This program is free software; you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation; either version 3 of the License, or
 (at your option) any later version.

 This library is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Lesser General Public License for more details.

 You should have received a copy of the GNU Lesser General Public License
 along with DataCore Bot; if not, see <http://www.gnu.org/licenses/>.
*/
using System;

namespace DataCore.Library
{
    /// <summary>
    /// Contains approximate string matching
    /// </summary>
    public static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings; see also https://github.com/kdjones/fuzzystring
        /// </summary>
        public static int Compute(string needle, string haystack)
        {
            int m = needle.Length;
            int n = haystack.Length;
            int[,] d = new int[m + 1, n + 1];

            if (m == 0)
            {
                return n;
            }

            if (n == 0)
            {
                return m;
            }

            for (int i = 0; i <= m; d[i, 0] = i++);
            for (int j = 0; j <= n; d[0, j] = j++);

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    int cost = (haystack[j - 1] == needle[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[m, n];
        }
    }
}