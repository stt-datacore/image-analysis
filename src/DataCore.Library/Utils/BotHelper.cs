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
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DataCore.Library
{
    public class GauntletResultEntry
    {
        public CrewData Crew;
        public string[] MatchingTraits;
    }

    public class GauntletResult
    {
        public GauntletResultEntry[] Results;
        public string ErrorMessage;
    }

    public class BotHelper
    {
        private CrewData[] _allcrew;
        private ItemData[] _items;
        private QuestData[] _quests;
        private Dilemma[] _dilemmas;
        private readonly object dataLock = new object();
        private string _datapath;
        private bool _downloadData;
        private string _datacoreURL;
        private string[] _traits;

        public ItemData[] Items
        {
            get
            {
                return _items;
            }
        }

        public QuestData[] Quests
        {
            get
            {
                return _quests;
            }
        }

        public string[] Traits
        {
            get
            {
                return _traits;
            }
        }

        public BotHelper(string datacoreURL, string datapath, bool downloadData = true)
        {
            _datapath = datapath;
            _datacoreURL = datacoreURL;
            _downloadData = downloadData;
        }

        private void DownloadFile(string fileName)
        {
            string jsonUrl = $"{_datacoreURL}structured/{fileName}";
            using (WebClient client = new WebClient())
            using (Stream data = client.OpenRead(jsonUrl))
            {
                using (Stream targetfile = File.Create(Path.Combine(_datapath, fileName)))
                {
                    data.CopyTo(targetfile);
                }
            }
        }

        public void DownloadNewData()
        {
            if (_downloadData)
            {
                DownloadFile("botcrew.json");
                DownloadFile("quests.json");
                DownloadFile("items.json");
            }

            ParseData();
        }

        public void ParseData()
        {
            lock (dataLock)
            {
                _allcrew = JsonConvert.DeserializeObject<CrewData[]>(File.ReadAllText(Path.Combine(_datapath, "botcrew.json")));
                _items = JsonConvert.DeserializeObject<ItemData[]>(File.ReadAllText(Path.Combine(_datapath, "items.json")));
                _quests = JsonConvert.DeserializeObject<QuestData[]>(File.ReadAllText(Path.Combine(_datapath, "quests.json")));
                _dilemmas = JsonConvert.DeserializeObject<Dilemma[]>(File.ReadAllText(Path.Combine(_datapath, "dilemmas.json")));
            }

            // TODO: call this only if the data actually changed
            CalcCrewStats();
        }

        private void CalcCrewStats()
        {
            // Get a list of all unique trait names across all crew
            _traits = _allcrew.SelectMany(crew => crew.traits_named).Distinct().ToArray();
        }

        public int TotalCrew()
        {
            return _allcrew.Count();
        }

        public CrewData GetCrew(string symbol)
        {
            lock (dataLock)
            {
                try
                {
                    return _allcrew.First(crew => crew.symbol == symbol);
                }
                catch (System.InvalidOperationException)
                {
                    return null;
                }
            }
        }

        private bool ExactMatch(string value, string toSearch)
        {
            if ((toSearch.Length >= 2) && (toSearch[0] == '"') && (toSearch[toSearch.Length - 1] == '"'))
            {
                // Exact match if quoted
                return string.Equals(value, toSearch.Substring(1, toSearch.Length - 2), StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return string.Equals(value, toSearch, StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool Predicate(string value, string toSearch)
        {
            if ((toSearch.Length >= 2) && (toSearch[0] == '"') && (toSearch[toSearch.Length - 1] == '"'))
            {
                // Exact match if quoted
                return string.Equals(value, toSearch.Substring(1, toSearch.Length - 2), StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // If it has a space / multiple tokens, also search the beginnings
                var terms = toSearch.Trim().ToLower().Split(' ');
                return terms.All(term => value.ToLower().IndexOf(term) >= 0);
            }
        }

        struct CrewSearchFuzzyHelper
        {
            public CrewData Crew;
            public int Distance;

            public CrewSearchFuzzyHelper(CrewData crew, int distance)
            {
                Crew = crew;
                Distance = distance;
            }
        }

        public List<CrewData> SearchCrew(string input, bool everywhere = false)
        {
            if (!everywhere)
            {
                lock (dataLock)
                {
                    var result = _allcrew.Where(crew => ExactMatch(crew.name, input));
                    // Look for an exact match first
                    if (result.Count() == 1)
                    {
                        return new List<CrewData>(result);
                    }

                    result = _allcrew.Where(crew => Predicate(crew.name, input));
                    if ((result.Count() == 0) && (input.IndexOf(' ') < 0) && (input.Length > 3))
                    {
                        // Let's try a fuzzy search
                        List<CrewSearchFuzzyHelper> data = new List<CrewSearchFuzzyHelper>();
                        foreach (var crew in _allcrew)
                        {
                            var bestMatch = crew.name.Split(' ')
                                .Select(term => (term.Length > input.Length - 2) ? LevenshteinDistance.Compute(term, input) : 99)
                                .OrderBy(m => m)
                                .First();

                            if (bestMatch < 4)
                            {
                                data.Add(new CrewSearchFuzzyHelper(crew, bestMatch));
                            }
                        }
                        result = data.OrderBy(entry => entry.Distance).Select(entry => entry.Crew);
                    }

                    return new List<CrewData>(result);
                }
            }
            else
            {
                bool inQuotes = false;
                var inputs = input.Trim().Split(c =>
                                 {
                                     if (c == '\"')
                                         inQuotes = !inQuotes;

                                     return !inQuotes && c == ' ';
                                 });
                IEnumerable<CrewData> results;
                lock (dataLock)
                {
                    results = _allcrew;
                }
                foreach (var sr in inputs)
                {
                    var srinput = sr.Trim().ToLower();
                    if (srinput == "sci")
                    {
                        results = results.Where(crew => crew.base_skills.science_skill != null);
                    }
                    else if (srinput == "sec")
                    {
                        results = results.Where(crew => crew.base_skills.security_skill != null);
                    }
                    else if (srinput == "eng")
                    {
                        results = results.Where(crew => crew.base_skills.engineering_skill != null);
                    }
                    else if (srinput == "dip")
                    {
                        results = results.Where(crew => crew.base_skills.diplomacy_skill != null);
                    }
                    else if (srinput == "cmd")
                    {
                        results = results.Where(crew => crew.base_skills.command_skill != null);
                    }
                    else if (srinput == "med")
                    {
                        results = results.Where(crew => crew.base_skills.medicine_skill != null);
                    }
                    else
                    {
                        results = results.Where(crew =>
                            Predicate(crew.name, srinput) ||
                            crew.traits_named.Any(trait => Predicate(trait, srinput)) ||
                            crew.traits_hidden.Any(trait => Predicate(trait, srinput))
                        );
                    }
                }

                return results.ToList();
            }
        }

        public List<CrewData> BestCrew(string input, int raritySearch)
        {
            var inputs = input.Split(' ');
            if ((inputs.Count() < 2) || (inputs.Count() > 3))
            {
                return null;
            }

            var type = inputs[0].ToLower();

            if (type == "base")
            {
                if (inputs.Count() != 2)
                {
                    return null;
                }

                return BestHelper.BestBaseCrew(_allcrew, inputs[1].ToLower(), raritySearch);
            }
            else if (type == "avg")
            {
                if (inputs.Count() != 2)
                {
                    return null;
                }

                return BestHelper.BestAvgCrew(_allcrew, inputs[1].ToLower(), raritySearch);
            }
            else if (type == "gauntlet")
            {
                if (inputs.Count() == 3)
                {
                    return BestHelper.BestGauntletCrew(_allcrew, inputs[1].ToLower(), inputs[2].ToLower(), raritySearch);
                }
                else
                {
                    return BestHelper.BestGauntletCrew(_allcrew, inputs[1].ToLower(), "FAKE", raritySearch);
                }
            }
            else if (type == "voyage")
            {
                if (inputs.Count() == 3)
                {
                    return BestHelper.BestVoyageCrew(_allcrew, inputs[1].ToLower(), inputs[2].ToLower(), raritySearch);
                }
                else
                {
                    return BestHelper.BestVoyageCrew(_allcrew, inputs[1].ToLower(), "FAKE", raritySearch);
                }
            }
            else
            {
                // Syntax error
                return null;
            }
        }

        public List<ItemData> SearchItem(string input)
        {
            try
            {
                Regex rgx = new Regex(@"^(\d+) ([""A-Z'a-z ]+)$");
                var m = rgx.Matches(input);
                if ((m.Count == 1) && m[0].Groups.Count == 3)
                {
                    var rarity = Int32.Parse(m[0].Groups[1].Value);
                    var name = m[0].Groups[2].Value.ToLower();
                    return new List<ItemData>(_items.Where(item => Predicate(item.name, name) && (item.rarity == rarity)));
                }
                else
                {
                    // Fuzzy search
                    return new List<ItemData>(_items.Where(item => Predicate(item.name, input)));
                }
            }
            catch (Exception ex)
            {
                // TODO: log ex
                Console.WriteLine($"Exception during item search: {ex.Message}");
                return new List<ItemData>();
            }
        }

        public List<Dilemma> SearchDilemmas(string input)
        {
            return new List<Dilemma>(_dilemmas.Where(dilemma => Predicate(dilemma.title, input.Trim())));
        }

        public GauntletResult Gauntlet(string[] inputs)
        {
            GauntletResult result = new GauntletResult();
            // First, check if the inputs even match known trait names
            if (!_traits.Any(s => s.Equals(inputs[0], StringComparison.OrdinalIgnoreCase)) ||
                !_traits.Any(s => s.Equals(inputs[1], StringComparison.OrdinalIgnoreCase)) ||
                !_traits.Any(s => s.Equals(inputs[2], StringComparison.OrdinalIgnoreCase)))
            {
                result.ErrorMessage = "Please check the trait spelling";
                return result;
            }

            Func<CrewData, string[]> TraitSearch = (CrewData crew) =>
            {
                List<string> matching = new List<string>();
                if (crew.traits_named.Any(s => s.Equals(inputs[0], StringComparison.OrdinalIgnoreCase)))
                    matching.Add(inputs[0]);
                if (crew.traits_named.Any(s => s.Equals(inputs[1], StringComparison.OrdinalIgnoreCase)))
                    matching.Add(inputs[1]);
                if (crew.traits_named.Any(s => s.Equals(inputs[2], StringComparison.OrdinalIgnoreCase)))
                    matching.Add(inputs[2]);
                return matching.ToArray();
            };

            result.ErrorMessage = string.Empty;
            result.Results = _allcrew.Where(crew => (crew.max_rarity > 3))
                .Select(crew => new GauntletResultEntry { Crew = crew, MatchingTraits = TraitSearch(crew) })
                .Where(entry => entry.MatchingTraits.Length > 1)
                .ToArray();

            return result;
        }
    }
}
