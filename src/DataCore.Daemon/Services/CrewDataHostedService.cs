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

namespace DataCore.Daemon
{
    public class CrewDataHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private Timer _timer;
        private CrewDataSingletonService _crewDataSingletonService;

        public CrewDataHostedService(IConfiguration config, ILogger<CrewDataHostedService> logger, CrewDataSingletonService crewDataSingletonService)
        {
            _logger = logger;
            _config = config;
            _crewDataSingletonService = crewDataSingletonService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started crewdata: {_config["bot"]}");

            _timer = new Timer(DownloadNewData, null, TimeSpan.Zero, TimeSpan.FromMinutes(20));

            return Task.CompletedTask;
        }

        private void DownloadNewData(object state)
        {
            _logger.LogInformation("Every 20 minutes check for and download new crew data");
            _crewDataSingletonService.BotHelper.DownloadNewData();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopped crewdata.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}