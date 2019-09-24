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
using System.Linq;
using System.Collections.Generic;

namespace DataCore.Library
{
    public static class CrewFormatter
    {
        public static string FormatSkill(Skill skill, bool useSpace)
        {
            return $"{skill.core}{(useSpace ? " " : "^")}({skill.range_min}-{skill.range_max})";
        }

        static List<string> FormatCrewStatsInternal(Skills skills, bool useSpace)
        {
            List<string> result = new List<string>();
            if (skills.command_skill != null)
            {
                result.Add($"CMD {FormatSkill(skills.command_skill, useSpace)}");
            }

            if (skills.science_skill != null)
            {
                result.Add($"SCI {FormatSkill(skills.science_skill, useSpace)}");
            }

            if (skills.security_skill != null)
            {
                result.Add($"SEC {FormatSkill(skills.security_skill, useSpace)}");
            }

            if (skills.engineering_skill != null)
            {
                result.Add($"ENG {FormatSkill(skills.engineering_skill, useSpace)}");
            }

            if (skills.diplomacy_skill != null)
            {
                result.Add($"DIP {FormatSkill(skills.diplomacy_skill, useSpace)}");
            }

            if (skills.medicine_skill != null)
            {
                result.Add($"MED {FormatSkill(skills.medicine_skill, useSpace)}");
            }

            return result;
        }

        public static List<string> FormatCrewStats(CrewData crew, bool useSpace, int raritySearch = 0)
        {
            var data = crew.skill_data.FirstOrDefault(sd => sd.rarity == raritySearch);
            if (data != null)
            {
                return FormatCrewStatsInternal(data.base_skills, useSpace);
            }
            return FormatCrewStatsInternal(crew.base_skills, useSpace);
        }

        public static string FormatCrewCoolRanks(CrewData crew, bool orEmpty = false, string separator = ", ")
        {
            List<string> result = new List<string>();

            if (crew.ranks.voyRank <= 10)
            {
                result.Add($"Voyage #{crew.ranks.voyRank} overall");
            }

            if (crew.ranks.gauntletRank <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.gauntletRank} overall");
            }

            // TODO: this is silly, there should be a more compact way of writing this.
            // Perhaps we can calculate the ranks locally instead of downloading them
            if (crew.ranks.V_CMD_SCI <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_CMD_SCI} CMD/SCI");
            }

            if (crew.ranks.V_CMD_SEC <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_CMD_SEC} CMD/SEC");
            }

            if (crew.ranks.V_CMD_ENG <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_CMD_ENG} CMD/ENG");
            }

            if (crew.ranks.V_CMD_DIP <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_CMD_DIP} CMD/DIP");
            }

            if (crew.ranks.V_CMD_MED <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_CMD_MED} CMD/MED");
            }

            if (crew.ranks.V_SCI_SEC <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_SCI_SEC} SCI/SEC");
            }

            if (crew.ranks.V_SCI_ENG <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_SCI_ENG} SCI/ENG");
            }

            if (crew.ranks.V_SCI_DIP <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_SCI_DIP} SCI/DIP");
            }

            if (crew.ranks.V_SCI_MED <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_SCI_MED} SCI/MED");
            }

            if (crew.ranks.V_SEC_ENG <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_SEC_ENG} SEC/ENG");
            }

            if (crew.ranks.V_SEC_DIP <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_SEC_DIP} SEC/DIP");
            }

            if (crew.ranks.V_SEC_MED <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_SEC_MED} SEC/MED");
            }

            if (crew.ranks.V_ENG_DIP <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_ENG_DIP} ENG/DIP");
            }

            if (crew.ranks.V_ENG_MED <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_ENG_MED} ENG/MED");
            }

            if (crew.ranks.V_DIP_MED <= 10)
            {
                result.Add($"Voyage #{crew.ranks.V_DIP_MED} DIP/MED");
            }

            if (crew.ranks.G_CMD_SCI <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_CMD_SCI} CMD/SCI");
            }

            if (crew.ranks.G_CMD_SEC <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_CMD_SEC} CMD/SEC");
            }

            if (crew.ranks.G_CMD_ENG <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_CMD_ENG} CMD/ENG");
            }

            if (crew.ranks.G_CMD_DIP <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_CMD_DIP} CMD/DIP");
            }

            if (crew.ranks.G_CMD_MED <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_CMD_MED} CMD/MED");
            }

            if (crew.ranks.G_SCI_SEC <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_SCI_SEC} SCI/SEC");
            }

            if (crew.ranks.G_SCI_ENG <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_SCI_ENG} SCI/ENG");
            }

            if (crew.ranks.G_SCI_DIP <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_SCI_DIP} SCI/DIP");
            }

            if (crew.ranks.G_SCI_MED <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_SCI_MED} SCI/MED");
            }

            if (crew.ranks.G_SEC_ENG <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_SEC_ENG} SEC/ENG");
            }

            if (crew.ranks.G_SEC_DIP <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_SEC_DIP} SEC/DIP");
            }

            if (crew.ranks.G_SEC_MED <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_SEC_MED} SEC/MED");
            }

            if (crew.ranks.G_ENG_DIP <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_ENG_DIP} ENG/DIP");
            }

            if (crew.ranks.G_ENG_MED <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_ENG_MED} ENG/MED");
            }

            if (crew.ranks.G_DIP_MED <= 10)
            {
                result.Add($"Gauntlet #{crew.ranks.G_DIP_MED} DIP/MED");
            }

            if (crew.ranks.B_SCI <= 10)
            {
                result.Add($"Base #{crew.ranks.B_SCI} SCI");
            }

            if (crew.ranks.B_SEC <= 10)
            {
                result.Add($"Base #{crew.ranks.B_SEC} SEC");
            }

            if (crew.ranks.B_ENG <= 10)
            {
                result.Add($"Base #{crew.ranks.B_ENG} ENG");
            }

            if (crew.ranks.B_DIP <= 10)
            {
                result.Add($"Base #{crew.ranks.B_DIP} DIP");
            }

            if (crew.ranks.B_CMD <= 10)
            {
                result.Add($"Base #{crew.ranks.B_CMD} CMD");
            }

            if (crew.ranks.B_MED <= 10)
            {
                result.Add($"Base #{crew.ranks.B_MED} MED");
            }

            result = new List<string>(result.Where(entry => entry.IndexOf("#0") == -1));

            if (result.Count == 0)
            {
                if (orEmpty)
                {
                    return string.Empty;
                }
                else
                {
                    return "No top 10 stats";
                }
            }
            else
            {
                return string.Join(separator, result);
            }
        }
    }

}