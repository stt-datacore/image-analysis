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

    public class CrewShipBattle
    {
        public int? accuracy { get; set; }
        public int? crit_bonus { get; set; }
        public int? crit_chance { get; set; }
        public int? evasion { get; set; }
    }

    public class CrewAction
    {
        public string name { get; set; }
        public int bonus_amount { get; set; }
        public int bonus_type { get; set; }
        public int initial_cooldown { get; set; }
        public int cooldown { get; set; }
        public int duration { get; set; }
        public int? limit { get; set; }
        public CrewActionPenalty penalty { get; set; }
        public CrewActionAbility ability { get; set; }
        public CrewActionChargePhase[] charge_phases { get; set; }

        public string GetBonusType(int type)
        {
            switch (type)
            {
                case 0: return "attack";
                case 1: return "evasion";
                case 2: return "accuracy";
                case 3: return "shield regen";
                default: return "BUG";
            }
        }

        public string[] GetChargePhases()
        {
            if ((charge_phases != null) && (charge_phases.Length > 0))
            {
                System.Collections.Generic.List<string> cps = new System.Collections.Generic.List<string>();
                int charge_time = 0;
                foreach (CrewActionChargePhase cp in charge_phases)
                {
                    charge_time += cp.charge_time;
                    string phaseDescription = $"After {charge_time}s, ";

                    if (cp.ability_amount.HasValue)
                    {
                        phaseDescription += $"+{cp.ability_amount.Value} {GetBonusType(bonus_type)}";
                    }

                    if (cp.bonus_amount.HasValue)
                    {
                        phaseDescription += $"+{cp.bonus_amount.Value} to {GetBonusType(bonus_type)}";
                    }

                    if (cp.duration.HasValue)
                    {
                        phaseDescription += $", +{cp.duration.Value}s duration";
                    }

                    if (cp.cooldown.HasValue)
                    {
                        phaseDescription += $", +{cp.cooldown.Value}s cooldown";
                    }
                    cps.Add(phaseDescription);
                }

                return cps.ToArray();
            }
            else
            {
                return new string[] { };
            }
        }
    }

    public class CrewActionPenalty
    {
        public int type { get; set; }
        public int amount { get; set; }
    }

    public class CrewActionAbility
    {
        public int? condition { get; set; }
        public int type { get; set; }
        public int amount { get; set; }

        public override string ToString()
        {
            string val;
            switch (type)
            {
                case 0: { val = $"Increase bonus boost by +{amount}"; break; }
                case 1: { val = $"Immediately deals {amount}% damage"; break; }
                case 2: { val = $"Immediately repairs Hulls by {amount}%"; break; }
                case 3: { val = $"Immediately repairs Shields by {amount}%"; break; }
                case 4: { val = $"+{amount} to Crit Rating"; break; }
                case 5: { val = $"+{amount} to Crit Bonus"; break; }
                case 6: { val = $"Shield regeneration +{amount}"; break; }
                case 7: { val = $"+{amount}% to Attack Speed"; break; }
                case 8: { val = $"Increase boarding damage by {amount}%"; break; }
                default: { val = "BUG"; break; }
            }

            if (condition.HasValue && condition.Value > 0)
            {
                val += $" (Trigger: {ConditionToString()})";
            }
            return val;
        }

        private string ConditionToString()
        {
            switch (condition.Value)
            {
                case 0: return "None";
                case 1: return "Position";
                case 2: return "Cloak";
                case 4: return "Boarding";
                default: return "BUG";
            }
        }
    }

    public class CrewActionChargePhase
    {
        public int charge_time { get; set; }
        public int? bonus_amount { get; set; }
        public int? ability_amount { get; set; }
        public int? cooldown { get; set; }
        public int? duration { get; set; }
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
        public CrewShipBattle ship_battle { get; set; }
        public CrewAction action { get; set; }
    }
}
