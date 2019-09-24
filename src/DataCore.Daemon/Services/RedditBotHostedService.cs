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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reddit;
using Reddit.Controllers.EventArgs;

namespace DataCore.Daemon
{
    public class RedditBotHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CrewDataSingletonService _crewData;
        private SearcherSingletonService _searcher;
        private RedditAPI _reddit;
        private Reddit.Controllers.Subreddit _subReddit;
        private RedditHelper _redditHelper;

        public RedditBotHostedService(IConfiguration config, ILogger<RedditBotHostedService> logger, CrewDataSingletonService crewData, SearcherSingletonService searcher)
        {
            _logger = logger;
            _config = config;
            _crewData = crewData;
            _searcher = searcher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting reddit bot");

            if ((_config.GetValue("bot:REDDIT_CLIENT_ID", string.Empty) == string.Empty) ||
                (_config.GetValue("bot:REDDIT_REFRESH_TOKEN", string.Empty) == string.Empty) ||
                (_config.GetValue("bot:REDDIT_CLIENT_SECRET", string.Empty) == string.Empty))
            {
                _logger.LogWarning("Missing configuration entries for reddit");
                return Task.CompletedTask;
            }

            if ((_config.GetValue("bot:REDDIT_SUBREDDIT", string.Empty) == string.Empty))
            {
                _logger.LogWarning("Missing configuration entry bot:REDDIT_SUBREDDIT");
                return Task.CompletedTask;
            }

            _redditHelper = new RedditHelper(_config["DATACORE_WEB"], _logger, _searcher.Searcher, _crewData.BotHelper);

            _reddit = new RedditAPI(_config.GetValue<string>("bot:REDDIT_CLIENT_ID"), _config.GetValue<string>("bot:REDDIT_REFRESH_TOKEN"), _config.GetValue<string>("bot:REDDIT_CLIENT_SECRET"));

            _subReddit = _reddit.Subreddit(_config.GetValue<string>("bot:REDDIT_SUBREDDIT")).About();

            _subReddit.Posts.GetNew();
            _subReddit.Posts.MonitorNew();
            _subReddit.Posts.NewUpdated += c_NewPostAdded;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping reddit bot");

            if (_subReddit != null)
            {
                _subReddit.Posts.MonitorNew();
                _subReddit.Posts.NewUpdated -= c_NewPostAdded;
                _subReddit = null;
                _reddit = null;
                _redditHelper = null;
            }

            return Task.CompletedTask;
        }

        private void c_NewPostAdded(object sender, PostsUpdateEventArgs args)
        {
            foreach (var post in args.Added)
            {
                if (post is Reddit.Controllers.LinkPost)
                {
                    _redditHelper.ProcessPost(post as Reddit.Controllers.LinkPost);
                }
                else
                {
                    _logger.LogInformation($"New post '{post.Fullname}' ({post.Title}) is not a link!");
                }
            }
        }

        public void Dispose()
        {
        }
    }
}