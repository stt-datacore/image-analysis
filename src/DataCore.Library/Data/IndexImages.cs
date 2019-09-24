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
    [Serializable]
    public class ImageIndex
    {
        public float[] Features { get; set; }
        public string Symbol { get; set; }
        public int MType { get; set; }
        public int Cols { get; set; }
        public int Rows { get; set; }
        public int Rarity { get; set; }

        [NonSerialized]
        public int index;

        [NonSerialized]
        public int score;
    }
}
