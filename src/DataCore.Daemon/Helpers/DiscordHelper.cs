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
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Discord;
using Discord.WebSocket;

using DataCore.Library;
using System.Text;

namespace DataCore.Daemon
{
    public class DiscordHelper
    {
        private ILogger _logger;
        private VoyImage _voyImage;
        private Searcher _searcher;
        private BotHelper _botHelper;
        private ItemFormatter _itemFormatter;
        private string _datacoreURL;
        public DiscordHelper(string datacoreURL, ILogger logger, Searcher searcher, VoyImage voyImage, BotHelper botHelper)
        {
            _datacoreURL = datacoreURL;
            _logger = logger;
            _voyImage = voyImage;
            _searcher = searcher;
            _botHelper = botHelper;

            _itemFormatter = new ItemFormatter(_botHelper);
        }

        private static string GetEmoteOrString(SocketUserMessage message, string emoteName, string defaultString)
        {
            if (message.Channel is SocketGuildChannel)
            {
                SocketGuild guild = (message.Channel as SocketGuildChannel).Guild;
                if ((guild.Emotes != null) && (guild.Emotes.Count > 0))
                {
                    IEmote emote = guild.Emotes.First(e => e.Name == emoteName);
                    if (emote != null)
                    {
                        return emote.ToString();
                    }
                }
            }

            return defaultString;
        }

        private static string FormatCrewStatsWithEmotes(SocketUserMessage message, CrewData crew, int raritySearch = 0, bool forGauntlet = false)
        {
            return string.Join(" ", CrewFormatter.FormatCrewStats(crew, true, raritySearch, forGauntlet).Select(s => $"{s.Replace("^", " ")}"))
                .Replace("SCI", GetEmoteOrString(message, "sci", "SCI"))
                .Replace("SEC", GetEmoteOrString(message, "sec", "SEC"))
                .Replace("ENG", GetEmoteOrString(message, "eng", "ENG"))
                .Replace("DIP", GetEmoteOrString(message, "dip", "DIP"))
                .Replace("CMD", GetEmoteOrString(message, "cmd", "CMD"))
                .Replace("MED", GetEmoteOrString(message, "med", "MED"));
        }

        private string GetMessageGuild(SocketUserMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                return (message.Channel as SocketGuildChannel).Guild.Name;
            }

            return null;
        }

        private static Color FromRarity(int rarity)
        {
            switch(rarity)
            {
                case 1: // Common
                    return new Color(155, 155, 155);

                case 2: // Uncommon
                    return new Color(80, 170, 60);

                case 3: // Rare
                    return new Color(90, 170, 255);

                case 4: // Super Rare
                    return new Color(170, 45, 235);

                case 5: // Legendary
                    return new Color(253, 210, 106);

                default:
                    return Color.LightGrey;
            }
        }

        private async Task HandleMessageStats(string searchString, SocketUserMessage message, bool extended)
        {
            int raritySearch = 0;
            if (searchString.IndexOf("-s") > 4)
            {
                var paramStart = searchString.IndexOf("-s");
                var paramValue = searchString.Substring(paramStart + 2);
                searchString = searchString.Substring(0, paramStart);

                if (int.TryParse(paramValue.Trim(), out int pv))
                {
                    raritySearch = pv;
                }
            }

            if ((raritySearch < 0) || (raritySearch > 5))
            {
                await message.Channel.SendMessageAsync($"If you're looking for stats at a specific number of stars, use a valid number for the -s option ({raritySearch} is invalid)");
                return;
            }

            var results = _botHelper.SearchCrew(searchString);
            if (results.Count == 0)
            {
                await message.Channel.SendMessageAsync($"Sorry, I couldn't find a crew matching '{searchString}'");
            }
            else if (results.Count == 1)
            {
                var crew = results[0];
                var embed = new EmbedBuilder()
                {
                    Title = crew.name,
                    Color = FromRarity(crew.max_rarity),
                    ThumbnailUrl = $"{_datacoreURL}media/assets/{crew.imageUrlPortrait}",
                    Url = $"{_datacoreURL}crew/{crew.symbol}/"
                };

                embed = embed.AddField("Traits", $"{string.Join(", ", crew.traits_named)}*, {string.Join(", ", crew.traits_hidden)}*");

                if ((raritySearch <= 0) || (raritySearch >= crew.max_rarity))
                {
                    raritySearch = 1;
                }

                string statLine = new string('⭐', raritySearch) + new string('X', crew.max_rarity - raritySearch).Replace("X", "🌑") + " " + FormatCrewStatsWithEmotes(message, crew, raritySearch) + "\n\n" +
                    new string('⭐', crew.max_rarity) + " " + FormatCrewStatsWithEmotes(message, crew);

                embed = embed.AddField("Stats", statLine)
                    .AddField("Voyage Rank", $"{crew.ranks.voyRank} of {_botHelper.TotalCrew()}", true)
                    .AddField("Gauntlet Rank", $"{crew.ranks.gauntletRank} of {_botHelper.TotalCrew()}", true)
                    .AddField("Estimated Cost", $"{crew.totalChronCost} {GetEmoteOrString(message, "chrons", "chrons")}, {crew.factionOnlyTotal} faction", true)
                    .AddField("Difficulty", $"{(100 - (crew.ranks.chronCostRank * 100) / _botHelper.TotalCrew())}%", true)
                    .WithFooter(footer => footer.Text = CrewFormatter.FormatCrewCoolRanks(crew));

                if (crew.bigbook_tier.HasValue && crew.events.HasValue)
                {
                    embed = embed.AddField("Bigbook Tier", crew.bigbook_tier.Value, true)
                        .AddField("Events", crew.events.Value, true);
                }

                if (crew.collections.Count() > 0)
                {
                    embed = embed.AddField("Collections", string.Join(", ", crew.collections.Select(c => $"[{c}]({_datacoreURL}collection/{c.Replace(" ", "%20")}/)")));
                }

                if (extended && !string.IsNullOrEmpty(crew.markdownContent) && (crew.markdownContent.Length < 980))
                {
                    embed = embed.AddField("Book contents", crew.markdownContent);
                }

                await message.Channel.SendMessageAsync("", false, embed.Build());

                if (extended && !string.IsNullOrEmpty(crew.markdownContent) && (crew.markdownContent.Length >= 980))
                {
                    await message.Channel.SendMessageAsync(crew.markdownContent);
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($"There is more than one crew matching that: {string.Join(", ", results.Take(10).Select(crew => crew.name))}. Which one did you mean?");
            }
        }

        private async Task HandleMessageSearch(string searchString, SocketUserMessage message)
        {
            var results = _botHelper.SearchCrew(searchString, true);
            if (results.Count == 0)
            {
                await message.Channel.SendMessageAsync($"Sorry, I couldn't find any crew matching '{searchString}'");
            }
            else
            {
                string reply = $"Here's what I found for '{searchString}':\n";
                reply += string.Join("\n", results.Take(10).Select(crew => (new string('⭐', crew.max_rarity) + $" {crew.name}")));
                if (results.Count > 10)
                {
                    reply += $"\nand {results.Count - 10} more...";
                }

                await message.Channel.SendMessageAsync(reply);
            }
        }

        private async Task HandleMessageFarm(string searchString, SocketUserMessage message)
        {
            var results = _botHelper.SearchItem(searchString);
            if (results.Count == 0)
            {
                await message.Channel.SendMessageAsync($"Sorry, I couldn't find any item matching '{searchString}'");
            }
            else if (results.Count == 1)
            {
                var reply = _itemFormatter.ToReplyText(results[0]);
                reply = reply.Replace("chronitons", GetEmoteOrString(message, "chrons", "chronitons"));
                await message.Channel.SendMessageAsync(reply);
            }
            else
            {
                await message.Channel.SendMessageAsync($"There is more than one item matching that: {string.Join(", ", results.Take(10).Select(item => $"'{item.rarity} {item.name}'"))}. Which one did you mean?");
            }
        }

        private async Task HandleMessageBest(string searchString, SocketUserMessage message)
        {
            int raritySearch = 0;
            if (searchString.IndexOf("-s") > 4)
            {
                var paramStart = searchString.IndexOf("-s");
                var paramValue = searchString.Substring(paramStart + 2);
                searchString = searchString.Substring(0, paramStart).Trim();

                if (int.TryParse(paramValue.Trim(), out int pv))
                {
                    raritySearch = pv;
                }
            }
            var results = _botHelper.BestCrew(searchString, raritySearch);
            if ((results == null) || (results.Count == 0))
            {
                await message.Channel.SendMessageAsync($"Sorry, I couldn't run command 'best {searchString}'; try **-d best base <skill>** - check **-d help** for details");
            }
            else
            {
                string reply = $"Here's what I found for '{searchString}':\n";
                reply += string.Join("\n", results.Take(10).Select(crew => (new string('⭐', crew.max_rarity) + $" {crew.name} - " + FormatCrewStatsWithEmotes(message, crew))));

                await message.Channel.SendMessageAsync(reply);
            }
        }

        private async Task HandleMessageDilemma(string searchString, SocketUserMessage message)
        {
            var dilemmas = _botHelper.SearchDilemmas(searchString);
            if (dilemmas.Count > 0)
            {
                foreach (var dilemma in dilemmas)
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = dilemma.title,
                        Color = Color.DarkGreen
                    };

                    embed = embed.AddField("Choice A", FormatChoice(message, dilemma.choiceA))
                        .AddField("Choice B", FormatChoice(message, dilemma.choiceB));
                    
                    if (dilemma.choiceC != null)
                    {
                        embed = embed.AddField("Choice C", FormatChoice(message, dilemma.choiceC));
                    }

                    await message.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        private async Task HandleMessageGauntlet(string searchString, SocketUserMessage message)
        {
            var inputs = searchString.Split(' ');
            if ((inputs.Count() < 2) || (inputs.Count() > 3))
            {
                await message.Channel.SendMessageAsync($"The gauntlet command expects 3 traits as input; try something like **-d gauntlet borg resourceful interrogator** - check **-d help** for details");
            }

            var results = _botHelper.Gauntlet(inputs);
            if ((results == null) || (results.Count == 0))
            {
                await message.Channel.SendMessageAsync($"Sorry, I couldn't run command 'gauntlet {searchString}'; try something like **-d gauntlet borg resourceful interrogator** - check **-d help** for details");
            }
            else
            {
                StringBuilder sbReply = new StringBuilder($"**Traits: {string.Join(", ", inputs)} ({results.Count} total)**");
                sbReply.AppendLine("*45% or better 4 and 5 star crew:*");
                sbReply.AppendLine(string.Join("\n", results.Take(10).Select(crew => (new string('⭐', crew.max_rarity) + $" {crew.name} - " + FormatCrewStatsWithEmotes(message, crew, 0, true)))));

                await message.Channel.SendMessageAsync(sbReply.ToString());
            }
        }

        private async Task HandleMessageVoytime(string searchString, SocketUserMessage message, bool fromImage)
        {
            var inputs = searchString.Trim().Split(' ');
            if ((inputs.Count() < 6) || (inputs.Count() > 7))
            {
                await message.Channel.SendMessageAsync($"Expected format is <primary> <secondary> <any skill> <any skill> <any skill> <any skill> [<antimmatter=2500>]");
            }

            if (int.TryParse(inputs[0], out int primary) &&
                int.TryParse(inputs[1], out int secondary) &&
                int.TryParse(inputs[2], out int skill3) &&
                int.TryParse(inputs[3], out int skill4) &&
                int.TryParse(inputs[4], out int skill5) &&
                int.TryParse(inputs[5], out int skill6))
            {
                int antimatter = 2500;
                if (inputs.Count() == 7)
                {
                    int.TryParse(inputs[6], out antimatter);
                }

                var extendResults = VoyageCalculator.CalculateVoyage(primary, secondary, skill3, skill4, skill5, skill6, antimatter);

                string reply;
                if (fromImage)
                {
                    reply = $@"Did I get the numbers wrong? I'm still learning, please let @TemporalAgent7 know. Double-check the values, re-run the command with `-d voytime {primary} {secondary} {skill3} {skill4} {skill5} {skill6} {antimatter}` if it needs corrections.
*If you're on a laptop / desktop, try the online Voyage tool at {_datacoreURL}voyage for crew recommendations and more.*";
                }
                else
                {
                    reply = $"*If you're on a laptop / desktop, try the online Voyage tool at {_datacoreURL}voyage for crew recommendations and more.*";
                }

                reply = $@"Estimated voyage length of {TimeFormat(extendResults[0].result)}
{extendResults[0].dilChance}% chance to reach the {extendResults[0].lastDil}hr dilemma; refill with {extendResults[1].refillCostResult} dil for a {extendResults[1].dilChance}% chance to reach the {extendResults[1].lastDil}hr dilemma.
{reply}";

                await message.Channel.SendMessageAsync(reply);
            }
            else
            {
                await message.Channel.SendMessageAsync($"Expected format is <primary> <secondary> <any skill> <any skill> <any skill> <any skill> [<antimmatter=2500>], with all the parameters being numbers");
            }
        }

        private async Task HandleMessageHelp(string searchString, SocketUserMessage message)
        {
            await message.Channel.SendMessageAsync($@"Here are some things you can try:
**-d stats <crew_name>** - gives you stats for the crew. *crew_name* can be the full name or part of it ('ator sp' will find 'Gladiator Spock')
**-d search <trait> <name>** - search the crew list using combinations of traits or part of the name ('prisoner med coy' will find all McCoys that have the MED skill and the prisoner trait)
**-d farm <rarity> <name>** - will search for items of the given rarity (0 - 5) and name, and list out sources for it
**-d best [base <skill>]|[gauntlet <skill1> <skill2>]|[voyage <skill1> <skill2>]** - finds the top 10 best crew with the specified skillset
**-d voytime <primary> <secondary> <any skill> <any skill> <any skill> <any skill> [<antimmatter=2500>] - does a quick estimation of voyage length
**-d dilemma [text]** - will search dilemmas for the given text
**-d gauntlet <trait1> <trait2> <trait3>** - will give suggestions for crew to use in gauntlet that match at least 2 of the given traits
");
        }

        private async Task HandleMessageBehold(string url, SocketUserMessage message, bool debug)
        {
            if (!url.EndsWith("png", StringComparison.CurrentCultureIgnoreCase) && !url.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogInformation("Not an image link, so don't care");
                if (debug)
                {
                    await message.Channel.SendMessageAsync($"Usage **-d behold <url>**, where <url> is a link to a png or jpg image");
                }
                else
                {
                    return;
                }
            }

            // We don't want to await here, so we don't keep the channel occupied
            var _ = ProcessImage(message, url, debug);
        }

        public async Task MessageReceived(SocketUserMessage message)
        {
            // TODO: Use the command framework in Discord.net instead of manual parsing
            _logger.LogInformation("Message received from '{Username}' on channel {Channel} ({Guild}): {Content}", message.Author.Username, message.Channel.Name, GetMessageGuild(message), message.Content);
            if (message.Content.StartsWith("-d "))
            {
                var command = message.Content.Substring(3);
                if (command.StartsWith("stats "))
                {
                    await HandleMessageStats(command.Substring(6), message, false);
                }
                else if (command.StartsWith("estats "))
                {
                    await HandleMessageStats(command.Substring(7), message, true);
                }
                else if (command.StartsWith("search "))
                {
                    await HandleMessageSearch(command.Substring(7), message);
                }
                else if (command.StartsWith("help"))
                {
                    await HandleMessageHelp(command.Substring(5), message);
                }
                else if (command.StartsWith("farm "))
                {
                    await HandleMessageFarm(command.Substring(5), message);
                }
                else if (command.StartsWith("best "))
                {
                    await HandleMessageBest(command.Substring(5), message);
                }
                else if (command.StartsWith("dilemma "))
                {
                    await HandleMessageDilemma(command.Substring(8), message);
                }
                else if (command.StartsWith("voytime "))
                {
                    await HandleMessageVoytime(command.Substring(8), message, false);
                }
                else if (command.StartsWith("gauntlet "))
                {
                    await HandleMessageGauntlet(command.Substring(9), message);
                }
                else if (command.StartsWith("behold ") || command.StartsWith("voyimg "))
                {
                    var searchString = command.Substring(7);
                    var matches = Regex.Matches(searchString, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
                    if (matches.Count == 1)
                    {
                        var url = matches[0].Value;
                        await HandleMessageBehold(url, message, true);
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync($"Usage **-d behold <url>**, where <url> is a link to a png or jpg image");
                    }
                }
                else if (command.StartsWith("behold") && message.Attachments.Count > 0)
                {
                    // A behold request as image caption
                    var url = message.Attachments.First().Url;
                    await HandleMessageBehold(url, message, true);
                }
                else
                {
                    await message.Channel.SendMessageAsync($"Sorry; I'm a simple bot who knows few things ('{command}' is not one of them).\nTry **-d help**!");
                }
            }
            else if (message.Content.StartsWith("!dd "))
            {
                var command = message.Content.Substring(4).Trim();
                await HandleMessageDilemma(command, message);
            }
            else if (message.Attachments.Count > 0)
            {
                var url = message.Attachments.First().Url;
                await HandleMessageBehold(url, message, false);
            }
            else
            {
                // Another way to embed images (with URL)
                var matches = Regex.Matches(message.Content, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
                if (matches.Count == 1)
                {
                    var url = matches[0].Value;
                    await HandleMessageBehold(url, message, false);
                }
            }
        }

        private async Task ProcessImageVoy(SocketUserMessage message, string url, bool debug)
        {
            var result = _voyImage.SearchUrl(url);
            if (!result.valid)
            {
                if (debug)
                {
                    await message.Channel.SendMessageAsync($"That was not a valid voyage screen image (or I couldn't process it).");
                }
                return;
            }

            if ((result.cmd.SkillValue == 0) ||
                (result.dip.SkillValue == 0) ||
                (result.eng.SkillValue == 0) ||
                (result.med.SkillValue == 0) ||
                (result.sci.SkillValue == 0) ||
                (result.sec.SkillValue == 0))
            {
                if (debug)
                {
                    await message.Channel.SendMessageAsync($"Unable to process voyage screen image (AM: {result.antimatter}; cmd: {result.cmd.SkillValue}, {result.cmd.Primary}; dip: {result.dip.SkillValue}, {result.dip.Primary}; eng: {result.eng.SkillValue}, {result.eng.Primary}; med: {result.med.SkillValue}, {result.med.Primary}; sci: {result.sci.SkillValue}, {result.sci.Primary}; sec: {result.sec.SkillValue}, {result.sec.Primary}).");
                }
                return;
            }

            var searchString = result.AsSearchString();
            if (!string.IsNullOrEmpty(searchString))
            {
                await HandleMessageVoytime(searchString, message, true);
            }
        }

        private async Task ProcessImage(SocketUserMessage message, string url, bool debug)
        {
            var result = _searcher.SearchUrl(url);
            if (result == null)
            {
                if (debug)
                {
                    await message.Channel.SendMessageAsync($"That was not a valid behold image (or I couldn't process it).");
                }
                await ProcessImageVoy(message, url, debug);
                return;
            }

            bool isValidBehold = result.IsValid(9);

            _logger.LogInformation($"Processed an image url:'{url}' {result.GetLogString(9)} overall {(isValidBehold ? "VALID" : "INVALID")}");

            // Not a behold, or couldn't find the crew
            if (!isValidBehold && !debug)
            {
                await ProcessImageVoy(message, url, debug);
                return;
            }

            if (!isValidBehold)
            {
                await message.Channel.SendMessageAsync($"That was not a valid behold image (or I couldn't process it). Image {result.GetLogString(9)}");
                if (!result.IsValid(2))
                {
                    await ProcessImageVoy(message, url, debug);
                    return;
                }
            }

            CrewData[] crew = { _botHelper.GetCrew(result.crew1.symbol), _botHelper.GetCrew(result.crew2.symbol), _botHelper.GetCrew(result.crew3.symbol) };

            if ((crew[0] == null) || (crew[1] == null) || (crew[2] == null))
            {
                // Not a behold, or couldn't find the crew
                if (debug)
                {
                    await message.Channel.SendMessageAsync($"That was not a valid behold image (or I couldn't find the crew). Image {result.GetLogString(9)}");
                }
                await ProcessImageVoy(message, url, debug);
                return;
            }

            if ((crew[0].max_rarity != crew[1].max_rarity) || (crew[1].max_rarity != crew[2].max_rarity))
            {
                // Not a behold, or couldn't find the crew
                if (debug)
                {
                    await message.Channel.SendMessageAsync($"That was not a valid behold image (or I couldn't find correct crew). Image {result.GetLogString(9)}");
                }
                return;
            }

            string title;
            var best = BeholdFormatter.GetBest(result, crew, out title);

            var embed = new EmbedBuilder()
            {
                Title = title + " (but read the detailed comparison to see what makes sense for your roster).",
                Color = Color.DarkOrange,
                ThumbnailUrl = $"{_datacoreURL}media/assets/{best.imageUrlPortrait}"
            };

            embed = embed.AddField("Behold helper", $"Hey {message.Author.Mention}, that looks like a behold. Here are some stats for your choices:")
                .AddField($"{crew[0].name}{((crew[0] == best) ? " - best bet" : "")}", FormatBeholdLine(message, crew[0]))
                .AddField($"{crew[1].name}{((crew[1] == best) ? " - best bet" : "")}", FormatBeholdLine(message, crew[1]))
                .AddField($"{crew[2].name}{((crew[2] == best) ? " - best bet" : "")}", FormatBeholdLine(message, crew[2]))
                .AddField("More stats", $"[In-depth comparison of all three choices]({_datacoreURL}behold/?crew={crew[0].symbol}&crew={crew[1].symbol}&crew={crew[2].symbol})")
                .WithFooter(footer => footer.Text = "Did I get the crew wrong? I'm still learning, please let @TemporalAgent7 know.");

            await message.Channel.SendMessageAsync("", false, embed.Build());
        }

        private string FormatBeholdLine(SocketUserMessage message, CrewData crew)
        {
            List<string> result = new List<string>();
            result.Add($"[Full details for {crew.name}]({_datacoreURL}crew/{crew.symbol}/)");

            result.Add(FormatCrewStatsWithEmotes(message, crew));

            var rankLine = $"Voyage #{crew.ranks.voyRank}, Gauntlet #{crew.ranks.gauntletRank}, {(crew.events.HasValue ? crew.events.Value : 0)} events, {crew.collections.Count()} collections";
            if (crew.bigbook_tier.HasValue)
            {
                rankLine += $", Big book **tier {crew.bigbook_tier.Value}**";
            }
            result.Add(rankLine);

            var coolRanks = CrewFormatter.FormatCrewCoolRanks(crew, true);
            if (!string.IsNullOrEmpty(coolRanks))
            {
                result.Add($"*{coolRanks}*");
            }

            return string.Join("\n", result);
        }

        private static string FormatChoice(SocketUserMessage message, Choice choice)
        {
            var result = choice.text + "\n" + string.Join(", ", choice.reward);
            result = result.Replace(":honor:", GetEmoteOrString(message, "honor", "honor"))
                .Replace(":chrons:", GetEmoteOrString(message, "chrons", "chrons"));
            return result;
        }

        private static string TimeFormat(double duration)
        {
            int hours = (int)Math.Floor(duration);
            int minutes = (int)Math.Floor((duration-hours)*60);
  
            return string.Format("{0}h {1}m", hours, minutes);
        }
    }
}