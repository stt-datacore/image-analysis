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
    public class ItemRecipeEntry
    {
        public int count { get; set; }
        public bool factionOnly { get; set; }
        public string symbol { get; set; }
    }

    public class ItemRecipe
    {
        public int craftCost { get; set; }
        public ItemRecipeEntry[] list { get; set; }
    }

    public class ItemSource
    {
        public int type { get; set; } // 1 - faction, 2 - ship, 0 - conflict
        public string mission_symbol { get; set; }
        public string name { get; set; }
        public int? cost { get; set; }
        public float? avg_cost { get; set; }
        public int? mastery { get; set; }
        public int chance_grade { get; set; }
    }

    public class ItemData
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public int rarity { get; set; }
        public ItemSource[] item_sources { get; set; }
        public ItemRecipe recipe { get; set; }
    }
}
