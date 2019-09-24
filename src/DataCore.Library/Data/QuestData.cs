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
    public class QuestMission
    {
        public int episode { get; set; }
        public string episode_title { get; set; }
        public bool? cadet { get; set; }
    }

    public class QuestData
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string quest_type { get; set; }
        public QuestMission mission { get; set; }
    }
}
