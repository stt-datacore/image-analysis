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
using System.Text;
using System.Collections.Generic;

namespace DataCore.Library
{
    public class ItemFormatter
    {
        private BotHelper _botHelper;

        public ItemFormatter(BotHelper botHelper)
        {
            _botHelper = botHelper;
        }

        public string ToReplyText(ItemData item)
        {
            if (item.item_sources == null || item.item_sources.Count() == 0)
            {
                return FormatRecipe(item);
            }
            else
            {
                return FormatSources(item);
            }
        }

        private ItemData FindItemBySymbol(string symbol)
        {
            return _botHelper.Items.FirstOrDefault(item => item.symbol == symbol);
        }

        private string FormatRecipe(ItemData input)
        {
            if (input.recipe == null || input.recipe.list == null || input.recipe.list.Count() == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder($"You can craft a { new string('⭐', input.rarity) } {input.name} for {input.recipe.craftCost} chronitons using these items:\n");

                List<string> recipe = new List<string>();
                foreach (var entry in input.recipe.list)
                {
                    var item = FindItemBySymbol(entry.symbol);
                    recipe.Add($"{ new string('⭐', item.rarity) } {item.name} x {entry.count}" + (entry.factionOnly ? " (FACTION)" : ""));
                }
                sb.Append(string.Join("\n", recipe));

                return sb.ToString();
            }
        }

        private string FormatSources(ItemData input)
        {
            if (input.item_sources == null || input.item_sources.Count() == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder($"You can get a { new string('⭐', input.rarity) } {input.name} from these places:\n");

                List<string> recipe = new List<string>();
                foreach (var entry in input.item_sources)
                {
                    if (entry.type == 1)
                    {
                        recipe.Add($"{entry.name}, {FormatType(entry.type)} ({entry.chance_grade} / 5 chance)");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(entry.mission_symbol))
                        {
                            var quest = _botHelper.Quests.First(q => q.symbol == entry.mission_symbol);
                            recipe.Add($"{FormatQuestName(quest)}, {FormatMastery(entry.mastery.Value)} {FormatType(entry.type)} **{entry.cost} chronitons** ({entry.chance_grade} / 5 chance)");
                        }
                    }
                }
                sb.Append(string.Join("\n", recipe));

                return sb.ToString();
            }
        }

        private static string FormatType(int type)
        {
            if (type == 0)
            {
                return "Mission";
            }
            else if (type == 1)
            {
                return "Faction mission";
            }
            else if (type == 2)
            {
                return "Ship battle";
            }
            else
            {
                return string.Empty;
            }
        }

        private static string FormatMastery(int mastery)
        {
            if (mastery == 0)
            {
                return "Normal";
            }
            else if (mastery == 1)
            {
                return "Elite";
            }
            else if (mastery == 2)
            {
                return "Epic";
            }
            else
            {
                return string.Empty;
            }
        }

        private static string FormatQuestName(QuestData quest)
        {
            if (quest.mission.episode > 0)
            {
                return $"{quest.name} (EP {quest.mission.episode} - {quest.mission.episode_title})";
            }
            else if (quest.mission.cadet.HasValue)
            {
                return $"{quest.name} (CADET {quest.mission.episode_title})";
            }
            else
            {
                return $"{quest.name} ({quest.mission.episode_title})";
            }
        }
    }
}