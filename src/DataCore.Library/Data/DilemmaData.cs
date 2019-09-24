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
namespace DataCore.Library
{
    public class Choice
    {
        public string text { get; set; }
        public string[] reward { get; set; }
    }

    public class Dilemma
    {
        public string title { get; set; }
        public Choice choiceA { get; set; }
        public Choice choiceB { get; set; }
        public Choice choiceC { get; set; }
    }

}
