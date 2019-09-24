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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using DataCore.Library;

namespace DataCore.Daemon
{
    public class CrewDataSingletonService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public CrewDataSingletonService(IConfiguration config, ILogger<CrewDataSingletonService> logger)
        {
            _logger = logger;
            _config = config;

            string mainpath = _config["mainpath"];
            if (string.IsNullOrEmpty(mainpath))
            {
                mainpath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..");
            }

            BotHelper = new BotHelper(_config["DATACORE_WEB"], mainpath);
        }

        public BotHelper BotHelper { get; }
    }
}