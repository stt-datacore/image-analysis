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
using System.Linq;
using CommandLine;

using DataCore.Library;

namespace DataCore.CLI
{
    [Verb("train", HelpText = "Train from a dataset (produce descriptors)")]
    public class Train
    {
        [Option('n', "noimages", Default = false,
            HelpText = "Don't save the individual source images")]
        public bool NoImages { get; set; }

        [Option('d', "datacorepath", Default = "../../../website",
                    HelpText = "Path to where you checked out the datacore git repo")]
        public string DataCorePath { get; set; }
    }

    [Verb("search", HelpText = "Given an input behold image, attempt to find the 3 crew")]
    public class Search
    {
        [Option('m', "minimumconfidence", Default = 9,
            HelpText = "The minimum level of confidence to consider a match")]
        public int MinConfidence { get; set; }

        [Value(0, Required = true)]
        public string Url { get; set; }
    }

    [Verb("searchcrew", HelpText = "Given an input string, attempt to find the crew")]
    public class SearchCrew
    {
        [Value(0, Required = true)]
        public string Term { get; set; }
    }

    [Verb("test", HelpText = "Run test code")]
    public class Test
    {
        [Option('t', "type", Default = 0,
            HelpText = "Type of test to run")]
        public int TestType { get; set; }

        [Value(0, Required = true)]
        public string TestString { get; set; }
    }

    [Verb("testvoyage", HelpText = "Run test code")]
    public class TestVoyage
    {
        [Value(0, Required = true)]
        public string TestString { get; set; }
    }

    [Verb("meme", HelpText = "Test the imgflip meme generator")]
    public class Meme
    {
        [Value(0, Required = false)]
        public string Input { get; set; }
    }

    class Program
    {
        private static readonly System.Threading.AutoResetEvent _closing = new System.Threading.AutoResetEvent(false);

        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<Train, Search, SearchCrew, Test, TestVoyage, Meme>(args).MapResult(
                (Train opts) => TrainDataset(opts),
                (Search opts) => PerformSearch(opts),
                (SearchCrew opts) => PerformSearchCrew(opts),
                (Test opts) => PerformTest(opts),
                (TestVoyage opts) => PerformTestVoyage(opts),
                (Meme opts) => PerformTestMeme(opts),
                errs => 1
            );
        }

        static int TrainDataset(Train opts)
        {
            var outPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..");
            Trainer.ParseDataset(opts.DataCorePath, outPath, opts.NoImages, (string msg) =>
            {
                Console.WriteLine(msg);
            });

            Trainer.ParseSingleImage(System.IO.Path.Combine(outPath, "data", "behold_title.png"), "behold_title", outPath);

            return 0;
        }

        static int PerformSearch(Search opts)
        {
            var outPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..");
            Searcher searcher = new Searcher(outPath);

            SearchResults result;

            if (opts.Url.StartsWith("http"))
            {
                result = searcher.SearchUrl(opts.Url);
            }
            else
            {
                result = searcher.Search(opts.Url);
            }

            Console.WriteLine($"Valid: {result.IsValid(opts.MinConfidence)}");
            if (result.top != null)
            {
                Console.WriteLine($"Title match: {result.top.symbol} ({result.top.score})");
            }

            if (result.crew1 != null)
            {
                Console.WriteLine($"Crew 1 match: {result.crew1.symbol} ({result.crew1.stars} stars); score: {result.crew1.score} ({((result.crew1.score < opts.MinConfidence) ? "BELOW THRESHOLD" : "OK")})");
            }

            if (result.crew2 != null)
            {
                Console.WriteLine($"Crew 2 match: {result.crew2.symbol} ({result.crew2.stars} stars); score: {result.crew2.score} ({((result.crew2.score < opts.MinConfidence) ? "BELOW THRESHOLD" : "OK")})");
            }

            if (result.crew3 != null)
            {
                Console.WriteLine($"Crew 3 match: {result.crew3.symbol} ({result.crew3.stars} stars); score: {result.crew3.score} ({((result.crew3.score < opts.MinConfidence) ? "BELOW THRESHOLD" : "OK")})");
            }

            return 0;
        }

        static int PerformSearchCrew(SearchCrew opts)
        {
            var botHelper = new BotHelper("https://datacore.app/", System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..", "data"));
            botHelper.ParseData();
            var result = botHelper.SearchCrew(opts.Term);
            if (result.Count == 0)
            {
                Console.WriteLine("Not found");
            }
            else
            {
                Console.WriteLine($"Found: {string.Join(',', result.Select(r => r.name))}");
            }

            return 0;
        }

        static int PerformTest(Test opts)
        {
            if (opts.TestType == 0)
            {
                var voyImage = new VoyImage(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", ".."));

                VoyImageResult result;

                if (opts.TestString.StartsWith("http"))
                {
                    result = voyImage.SearchUrl(opts.TestString);
                }
                else
                {
                    result = voyImage.SearchImage(opts.TestString);
                }

                Console.WriteLine(result);
            }
            else if (opts.TestType == 1)
            {
                var botHelper = new BotHelper("https://datacore.app/", System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..", "data"));
                botHelper.ParseData();
                var result = botHelper.BestCrew(opts.TestString, 0);
                if (result.Count == 0)
                {
                    Console.WriteLine("Not found");
                }
                else
                {
                    Console.WriteLine(result[0].name);
                }
            }
            else if (opts.TestType == 2)
            {
                var botHelper = new BotHelper("https://datacore.app/", System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..", "data"));
                botHelper.ParseData();
                var result = botHelper.Gauntlet(opts.TestString.Split(' '));
                if (!string.IsNullOrEmpty(result.ErrorMessage) || result.Results.Count() == 0)
                {
                    Console.WriteLine("Not found");
                }
                else
                {
                    Console.WriteLine(string.Join(", ", result.Results.Select(gresult => $"{gresult.Crew.name} [{string.Join(", ", gresult.MatchingTraits)}]")));
                }
            }

            return 0;
        }

        static int PerformTestVoyage(TestVoyage opts)
        {
            var inputs = opts.TestString.Trim().Split(' ');
            if ((inputs.Count() < 6) || (inputs.Count() > 7))
            {
                Console.WriteLine($"Expected format is <primary> <secondary> <any skill> <any skill> <any skill> <any skill> [<antimmatter=2500>]");
                return 1;
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
                    int.TryParse(inputs[5], out antimatter);
                }

                Console.WriteLine(VoyageCalculator.CalculateVoyage(primary, secondary, skill3, skill4, skill5, skill6, antimatter));
            }

            return 0;
        }

        static int PerformTestMeme(Meme opts)
        {
            //var templates = MemeHelper.ListTemplates();
            //Console.WriteLine($"**Meme generator templates: {string.Join(", ", templates.Take(20))} and more ({templates.Count()} total)**");
            
            string searchString = @"khan ""DB!!"" ""sdfg sddsf"" ""asdfg""";

            searchString = searchString.Replace('“', '"').Replace('’', '\'').Trim();
            string pattern = @"(.*?)\""(.*?)\""(?:\W*\""(.*?)\"")?(?:\W*\""(.*?)\"")?(?:\W*\""(.*?)\"")?(?:\W*\""(.*?)\"")?$";
            var res = System.Text.RegularExpressions.Regex.Match(searchString, pattern);
            if (res.Success && res.Groups.Count > 3)
            {
                var results = res.Groups.Skip(2).Select(g => g.Value.Trim()).TakeWhile(v => !string.IsNullOrEmpty(v));

                if (results.Any(v => v.IndexOf('"') >= 0))
                {
                    Console.WriteLine("Invalid format");
                }
                else
                {
                    string rrr = string.Join('&', results.Select((text, index) => $"text{index}={Uri.EscapeDataString(text)}"));
                    Console.WriteLine(rrr);
                }
            }

            return 0;
        }
    }
}
