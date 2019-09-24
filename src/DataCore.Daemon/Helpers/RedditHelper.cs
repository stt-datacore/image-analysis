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
using Microsoft.Extensions.Logging;

using DataCore.Library;

namespace DataCore.Daemon
{
    public class RedditHelper
    {
        private ILogger _logger;
        private Searcher _searcher;
        private BotHelper _botHelper;
        private string _datacoreURL;

        public RedditHelper(string datacoreURL, ILogger logger, Searcher searcher, BotHelper botHelper)
        {
            _datacoreURL = datacoreURL;
            _logger = logger;
            _searcher = searcher;
            _botHelper = botHelper;
        }

        public void ProcessPost(Reddit.Controllers.LinkPost post)
        {
            _logger.LogInformation($"Found a new link post ({post.Title}); procesing...");

            string url = post.URL;
            if (!url.EndsWith("png") && !url.EndsWith("jpg"))
            {
                _logger.LogInformation($"Not an image link, so don't care");
                return;
            }

            var result = _searcher.SearchUrl(url);
            if (!result.IsValid(9))
            {
                _logger.LogInformation($"Not a behold, or couldn't find the crew");
                return;
            }

            try
            {
                post.SetFlair("Behold!");
            }
            catch
            {
                _logger.LogWarning($"Failed to set post flair.");
            }

            var postText = FormatReplyPost(result, post.Title);

            if (string.IsNullOrEmpty(postText))
            {
                _logger.LogInformation($"Failed to format post reply");
                return;
            }

            post.Reply(postText);
            _logger.LogInformation($"Replying to post with {postText}");
        }

        private string FormatReplyPost(SearchResults result, string postTitle)
        {
            CrewData[] crew = { _botHelper.GetCrew(result.crew1.symbol), _botHelper.GetCrew(result.crew2.symbol), _botHelper.GetCrew(result.crew3.symbol) };

            if ((crew[0] == null) || (crew[1] == null) || (crew[2] == null))
            {
                return string.Empty;
            }

            string title;
            var best = BeholdFormatter.GetBest(result, crew, out title);

            var c1s = CrewFormatter.FormatCrewStats(crew[0], false);
            var c2s = CrewFormatter.FormatCrewStats(crew[1], false);
            var c3s = CrewFormatter.FormatCrewStats(crew[2], false);

            if (postTitle.StartsWith("Behold", StringComparison.CurrentCultureIgnoreCase) && (postTitle.Split(',').Length > 1))
            {
                // Appears to have correct title format
                Func<int, string> perCrewFormat = index =>
                {
                    var mainRanks = $@"Voyage #{crew[index].ranks.voyRank} | Gauntlet #{crew[index].ranks.gauntletRank}";
                    var statLine = string.Join(" | ", CrewFormatter.FormatCrewStats(crew[index], true));
                    return $@"# [{crew[index].name}]({_datacoreURL}crew/{crew[index].symbol})

Big Book {(crew[index].bigbook_tier.HasValue ? $"**Tier {crew[index].bigbook_tier.Value}**" : "unranked")} | {mainRanks} | {CrewFormatter.FormatCrewCoolRanks(crew[index], false, " | ")} | {crew[index].collections.Length} collection(s) | {(crew[index].events.HasValue ? $"{crew[index].events.Value} event(s)" : "No events")}

{statLine}

{crew[index].markdownContent}";
                };

                return $@"{title} (but read the [detailed comparison]({_datacoreURL}behold/?crew={crew[0].symbol}&crew={crew[1].symbol}&crew={crew[2].symbol}) to see what makes sense for your roster).

{perCrewFormat(0)}

{perCrewFormat(1)}

{perCrewFormat(2)}

Talk to TemporalAgent7 if you have questions or comments!";
            }

            return $@"This post appears to be a behold, but it doesn't follow the [subreddit rule](https://www.reddit.com/r/StarTrekTimelines/comments/cgf25y/new_subreddit_rule_regarding_behold_posts/) for Behold posts; your post title should be `Behold! {crew[0].name}, {crew[1].name}, {crew[2].name}`. You can delete it and repost if you want me to reply with details.";
        }
    }
}