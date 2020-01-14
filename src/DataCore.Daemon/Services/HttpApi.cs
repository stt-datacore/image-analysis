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
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DataCore.Daemon
{
    public class HttpApi : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private IWebHost _webHost;
        private SearcherSingletonService _searcher;
        private CrewDataSingletonService _crewDataSingletonService;

        public HttpApi(IConfiguration config, ILogger<HttpApi> logger, SearcherSingletonService searcher, CrewDataSingletonService crewDataSingletonService)
        {
            _logger = logger;
            _config = config;
            _searcher = searcher;
            _crewDataSingletonService = crewDataSingletonService;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
            if (_config.GetValue("HTTP_API_ENABLED", false))
            {
                _logger.LogInformation($"Starting api endpoint");
                _webHost = WebHost.CreateDefaultBuilder()
                    .Configure(app => app.Run(RequestDelegate))
                    .Build();

                return _webHost.StartAsync(token);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private Task RequestDelegate(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/api"))
            {
                if (context.Request.Path.Value == "/api/behold" && context.Request.Method == "GET" && context.Request.Query.ContainsKey("url"))
                {
                    context.Response.ContentType = "application/json";
                    string url = context.Request.Query["url"];
                    var results = _searcher.Searcher.SearchUrl(url);
                    string beholdResult = "undefined";
                    string voyResult = "undefined";
                    if ((results != null) && results.IsValid(9))
                    {
                        beholdResult = results.ToJson();
                    } else {
                        // Not a guaranteed behold
                        beholdResult = (results != null) ? results.ToJson() : "undefined";
                        var resultsVoy = _searcher.VoyImage.SearchUrl(url);
                        voyResult = resultsVoy.valid ? resultsVoy.ToJson() : "undefined";
                    }

                    return context.Response.WriteAsync($"{{\"beholdResult\": {beholdResult}, \"voyResult\": {voyResult}}}");
                }
                else if (context.Request.Path.Value == "/api/downloadnow")
                {
                    _crewDataSingletonService.BotHelper.DownloadNewData();
                }

                return context.Response.WriteAsync($"Nothing to see here! ({context.Request.Path.Value})");
            }

            context.Response.StatusCode = 418;
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken token)
        {
            if (_webHost != null)
            {
                return _webHost.StopAsync(token);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}