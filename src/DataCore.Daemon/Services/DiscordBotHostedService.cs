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

using Discord;
using Discord.WebSocket;

namespace DataCore.Daemon
{
    public class DiscordBotHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private CrewDataSingletonService _crewData;
        private SearcherSingletonService _searcher;
        private DiscordSocketClient _client;
        private DiscordHelper _discordHelper;

        public DiscordBotHostedService(IConfiguration config, ILogger<DiscordBotHostedService> logger, CrewDataSingletonService crewData, SearcherSingletonService searcher)
        {
            _logger = logger;
            _config = config;
            _crewData = crewData;
            _searcher = searcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting discord bot");

            if (_config.GetValue("bot:DISCORD_TOKEN", string.Empty) == string.Empty)
            {
                _logger.LogWarning("Missing configuration entry bot:DISCORD_TOKEN");
                return;
            }

            _client = new DiscordSocketClient();

            _client.Log += (LogMessage msg) =>
            {
                _logger.LogInformation(msg.ToString());
                return Task.CompletedTask;
            };

            _client.MessageReceived += MessageReceived;

            _discordHelper = new DiscordHelper(_config["DATACORE_WEB"], _logger, _searcher.Searcher, _searcher.VoyImage, _crewData.BotHelper, _config["bot:IMGFLIP_PASSWORD"]);

            await _client.LoginAsync(TokenType.Bot, _config["bot:DISCORD_TOKEN"]);
            await _client.StartAsync();
            await _client.SetActivityAsync(new Game("with Spot", ActivityType.Playing));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping discord bot");

            if (_client != null)
            {
                _client.MessageReceived -= MessageReceived;
                await _client.LogoutAsync();
                await _client.StopAsync();
                _client = null;
                _discordHelper = null;
            }
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null)
                return;

            // Make sure other bots don't trigger commands
            if (message.Author.IsBot)
                return;

            await _discordHelper.MessageReceived(message);
        }

        public void Dispose()
        {
        }
    }
}