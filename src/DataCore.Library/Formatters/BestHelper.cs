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
    public static class BestHelper
    {
        static double STARBASE_BONUS_CORE = 1.15;
        static double STARBASE_BONUS_RANGE = 1.13;
        static double THIRD_SKILL_MULTIPLIER = 0.25;

        private static Skill GetCrewSkill(CrewData crew, string skill)
        {
            Skill result = null;
            if (skill == "sci")
            {
                result = crew.base_skills.science_skill;
            }
            else if (skill == "sec")
            {
                result = crew.base_skills.security_skill;
            }
            else if (skill == "eng")
            {
                result = crew.base_skills.engineering_skill;
            }
            else if (skill == "dip")
            {
                result = crew.base_skills.diplomacy_skill;
            }
            else if (skill == "cmd")
            {
                result = crew.base_skills.command_skill;
            }
            else if (skill == "med")
            {
                result = crew.base_skills.medicine_skill;
            }

            if (result == null)
            {
                result = new Skill();
                result.core = 0;
                result.range_max = 0;
                result.range_min = 0;
            }

            return result;
        }

        private static IEnumerable<CrewData> ForRarity(IEnumerable<CrewData> allcrew, int raritySearch)
        {
            IEnumerable<CrewData> results;
            if ((raritySearch < 1) || (raritySearch > 5)) // Overall
            {
                results = allcrew;
            }
            else
            {
                results = allcrew.Where(crew => crew.max_rarity == raritySearch);
            }

            return results;
        }

        private static double GetRangeAvgSkill(CrewData crew, string skill)
        {
            var skillData = GetCrewSkill(crew, skill);
            return ((skillData.range_max + skillData.range_min) * STARBASE_BONUS_RANGE) / 2;
        }

        private static double GetAvgSkill(CrewData crew, string skill)
        {
            var skillData = GetCrewSkill(crew, skill);
            return skillData.core * STARBASE_BONUS_CORE + GetRangeAvgSkill(crew, skill);
        }

        public static List<CrewData> BestBaseCrew(IEnumerable<CrewData> allcrew, string skill, int raritySearch)
        {
            return ForRarity(allcrew, raritySearch)
                .OrderByDescending(crew => GetCrewSkill(crew, skill).core * STARBASE_BONUS_CORE)
                .Take(10)
                .ToList();
        }

        public static List<CrewData> BestAvgCrew(IEnumerable<CrewData> allcrew, string skill, int raritySearch)
        {
            return ForRarity(allcrew, raritySearch)
                .OrderByDescending(crew => GetAvgSkill(crew, skill))
                .Take(10)
                .ToList();
        }

        public static List<CrewData> BestGauntletCrew(IEnumerable<CrewData> allcrew, string skill1, string skill2, int raritySearch)
        {
            return ForRarity(allcrew, raritySearch)
                .OrderByDescending(crew => {
                    return GetRangeAvgSkill(crew, skill1) + GetRangeAvgSkill(crew, skill2);
                    })
                .Take(10)
                .ToList();
        }

        public static List<CrewData> BestVoyageCrew(IEnumerable<CrewData> allcrew, string skill1, string skill2, int raritySearch)
        {
            return ForRarity(allcrew, raritySearch)
                .OrderByDescending(crew => {
                    double total = 0;

                    var skills = new List<string> {"sci", "sec", "eng", "dip", "cmd", "med"};
                    foreach (string value in skills)
                    {
                        if ((skill1 == value) || (skill2 == value))
                        {
                            total += GetAvgSkill(crew, value);
                        }
                        else
                        {
                            total += GetAvgSkill(crew, value) * THIRD_SKILL_MULTIPLIER;
                        }
                    }

                    return total;
                    })
                .Take(10)
                .ToList();
        }
    }
}