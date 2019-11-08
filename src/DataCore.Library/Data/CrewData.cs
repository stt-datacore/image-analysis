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
    public class Skill
    {
        public int core { get; set; }
        public int range_min { get; set; }
        public int range_max { get; set; }

        public int GetBaseScore(int starbase_bonus_core = 15)
        {
            return core * ((100 + starbase_bonus_core) / 100);
        }

        public int GetGauntletScore(int starbase_bonus_range = 13)
        {
            return ((range_max + range_min) / 2) * ((100 + starbase_bonus_range) / 100);
        }

        public int GetVoyageScore(int starbase_bonus_core = 15, int starbase_bonus_range = 13)
        {
            return GetBaseScore(starbase_bonus_core) + GetGauntletScore(starbase_bonus_range);
        }
    }

    public class Skills
    {
        public Skill command_skill { get; set; }
        public Skill science_skill { get; set; }
        public Skill security_skill { get; set; }
        public Skill engineering_skill { get; set; }
        public Skill diplomacy_skill { get; set; }
        public Skill medicine_skill { get; set; }

        public int GetGauntletScore(int starbase_bonus_range = 13)
        {
            return ((command_skill != null) ? command_skill.GetGauntletScore(starbase_bonus_range) : 0) +
                ((science_skill != null) ? science_skill.GetGauntletScore(starbase_bonus_range) : 0) +
                ((security_skill != null) ? security_skill.GetGauntletScore(starbase_bonus_range) : 0) +
                ((engineering_skill != null) ? engineering_skill.GetGauntletScore(starbase_bonus_range) : 0) +
                ((diplomacy_skill != null) ? diplomacy_skill.GetGauntletScore(starbase_bonus_range) : 0) +
                ((medicine_skill != null) ? medicine_skill.GetGauntletScore(starbase_bonus_range) : 0);
        }

        public int GetVoyageScore(int starbase_bonus_core = 15, int starbase_bonus_range = 13)
        {
            return ((command_skill != null) ? command_skill.GetVoyageScore(starbase_bonus_core, starbase_bonus_range) : 0) +
                ((science_skill != null) ? science_skill.GetVoyageScore(starbase_bonus_core, starbase_bonus_range) : 0) +
                ((security_skill != null) ? security_skill.GetVoyageScore(starbase_bonus_core, starbase_bonus_range) : 0) +
                ((engineering_skill != null) ? engineering_skill.GetVoyageScore(starbase_bonus_core, starbase_bonus_range) : 0) +
                ((diplomacy_skill != null) ? diplomacy_skill.GetVoyageScore(starbase_bonus_core, starbase_bonus_range) : 0) +
                ((medicine_skill != null) ? medicine_skill.GetVoyageScore(starbase_bonus_core, starbase_bonus_range) : 0);
        }
    }

    public class Ranks
    {
        public int voyRank { get; set; }
        public int gauntletRank { get; set; }
        public int chronCostRank { get; set; }
        public int V_CMD_SCI { get; set; }
        public int V_CMD_SEC { get; set; }
        public int V_CMD_ENG { get; set; }
        public int V_CMD_DIP { get; set; }
        public int V_CMD_MED { get; set; }
        public int V_SCI_SEC { get; set; }
        public int V_SCI_ENG { get; set; }
        public int V_SCI_DIP { get; set; }
        public int V_SCI_MED { get; set; }
        public int V_SEC_ENG { get; set; }
        public int V_SEC_DIP { get; set; }
        public int V_SEC_MED { get; set; }
        public int V_ENG_DIP { get; set; }
        public int V_ENG_MED { get; set; }
        public int V_DIP_MED { get; set; }
        public int G_CMD_SCI { get; set; }
        public int G_CMD_SEC { get; set; }
        public int G_CMD_ENG { get; set; }
        public int G_CMD_DIP { get; set; }
        public int G_CMD_MED { get; set; }
        public int G_SCI_SEC { get; set; }
        public int G_SCI_ENG { get; set; }
        public int G_SCI_DIP { get; set; }
        public int G_SCI_MED { get; set; }
        public int G_SEC_ENG { get; set; }
        public int G_SEC_DIP { get; set; }
        public int G_SEC_MED { get; set; }
        public int G_ENG_DIP { get; set; }
        public int G_ENG_MED { get; set; }
        public int G_DIP_MED { get; set; }
        public int B_SCI { get; set; }
        public int B_SEC { get; set; }
        public int B_ENG { get; set; }
        public int B_DIP { get; set; }
        public int B_CMD { get; set; }
        public int B_MED { get; set; }
        public int A_SCI { get; set; }
        public int A_SEC { get; set; }
        public int A_ENG { get; set; }
        public int A_DIP { get; set; }
        public int A_CMD { get; set; }
        public int A_MED { get; set; }
    }

    public class CrewSkillData
    {
        public int rarity { get; set; }
        public Skills base_skills { get; set; }
    }

    public class CrewData
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public string imageUrlPortrait { get; set; }
        public string[] traits_named { get; set; }
        public string[] traits_hidden { get; set; }
        public string[] collections { get; set; }
        public int max_rarity { get; set; }
        public int totalChronCost { get; set; }
        public int factionOnlyTotal { get; set; }
        public int craftCost { get; set; }
        public int? bigbook_tier { get; set; }
        public int? events { get; set; }
        public Skills base_skills { get; set; }
        public Ranks ranks { get; set; }
        public string markdownContent { get; set; }
        public CrewSkillData[] skill_data { get; set; }
    }
}
