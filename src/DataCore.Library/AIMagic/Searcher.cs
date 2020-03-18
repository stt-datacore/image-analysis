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
using OpenCvSharp;

using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DataCore.Library
{
    public class SearchResult
    {
        public string symbol { get; set; }
        public int score { get; set; }
        public int stars { get; set; }
    }

    public class SearchResults
    {
        public int input_width { get; set; }
        public int input_height { get; set; }
        public SearchResult top { get; set; }
        public SearchResult crew1 { get; set; }
        public SearchResult crew2 { get; set; }
        public SearchResult crew3 { get; set; }
        public string error { get; set; }

        public int closebuttons { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string GetLogString(int minConfidence)
        {
            var result = $"({input_width}x{input_height}px), ";
            if (top == null)
            {
                result += "failed to discover title, ";
            }
            else
            {
                result += $"title matches {top.symbol} with score:{top.score}, ";
            }

            if (crew1 == null)
            {
                result += "failed to discover crew1, ";
            }
            else
            {
                result += $"crew1 matches {crew1.symbol} with score:{crew1.score} ({((crew1.score < minConfidence) ? "BELOW THRESHOLD" : "OK")}), ";
            }

            if (crew2 == null)
            {
                result += "failed to discover crew2, ";
            }
            else
            {
                result += $"crew2 matches {crew2.symbol} with score:{crew2.score} ({((crew2.score < minConfidence) ? "BELOW THRESHOLD" : "OK")}), ";
            }

            if (crew3 == null)
            {
                result += "failed to discover crew3, ";
            }
            else
            {
                result += $"crew3 matches {crew3.symbol} with score:{crew3.score} ({((crew3.score < minConfidence) ? "BELOW THRESHOLD" : "OK")}), ";
            }

            result += $"{closebuttons} close buttons";

            return result;
        }

        public bool IsValid(int threshold = 10)
        {
            if ((top == null) || (top.symbol != "behold_title") || (top.score < threshold))
            {
                return false;
            }

            if ((crew1 == null) || (crew1.score < threshold))
            {
                return false;
            }

            if ((crew2 == null) || (crew2.score < threshold))
            {
                return false;
            }

            if ((crew3 == null) || (crew3.score < threshold))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(error))
            {
                return false;
            }

            if (closebuttons > 0)
            {
                // If we found something that looks like a close button, but other heuristics rank high, ignore it
                if ((crew1.score + crew2.score + crew3.score + top.score) / 4 < 35)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class Crew
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public string imageUrlFullBody { get; set; }
        public int max_rarity { get; set; }
    }

    public class Searcher
    {
        private IEnumerable<ImageIndex> _indexedDatset;
        private SURFDescriptor _descriptor;
        private DescriptorMatcher _descriptorMatcher;
        private Mat _starFull;
        private Mat _closeButton;

        public Searcher(string mainpath)
        {
            _descriptor = new SURFDescriptor();
            _indexedDatset = Trainer.LoadFromDisk(mainpath);
            _descriptorMatcher = InitMatcher();

            _starFull = new Mat(Path.Combine(mainpath, "data", "starfull.png"));
            _closeButton = new Mat(Path.Combine(mainpath, "data", "closebutton.png"));
        }

        private DescriptorMatcher InitMatcher()
        {
            FlannBasedMatcher matcher = new FlannBasedMatcher(new OpenCvSharp.Flann.KDTreeIndexParams(4), new OpenCvSharp.Flann.SearchParams(50));

            int current = 0;
            foreach (var item in _indexedDatset)
            {
                Mat itemFeatures = new Mat(item.Rows, item.Cols, item.MType);
                itemFeatures.SetArray(item.Features);
                Mat tempItem = itemFeatures;

                itemFeatures.ConvertTo(itemFeatures, MatType.CV_32F);
                matcher.Add(new Mat[] { itemFeatures });
                item.index = current++;
            }

            return matcher;
        }

        private ImageIndex MatchCrew(Mat crew)
        {
            KeyPoint[] keypoints;
            Mat crew3Features = new Mat();
            _descriptor.Describe(crew, out keypoints).ConvertTo(crew3Features, MatType.CV_32F);

            var goodMatches = _descriptorMatcher.Match(crew3Features);
            var indexBest = goodMatches.GroupBy(x => (x.ImgIdx)).Select(
                g => new
                {
                    Key = g.Key,
                    Value = g.Count()
                }).OrderByDescending(x => (x.Value)).First();

            var result = _indexedDatset.Where(x => x.index == indexBest.Key).FirstOrDefault();

            result.score = indexBest.Value;

            return result;
        }

        public SearchResults SearchImage(Mat query)
        {
            SearchResults results = new SearchResults();
            results.input_height = query.Rows;
            results.input_width = query.Cols;

            try
            {
                // First, take the top of the image and look for the title
                Mat top = query.SubMat(0, Math.Min(query.Rows / 13, 80), query.Cols / 3, query.Cols * 2 / 3);
                if (top.Empty())
                {
                    results.top = null;
                    results.error = "Top row was empty";
                    return results;
                }

                var topResult = MatchCrew(top);
                results.top = new SearchResult { symbol = topResult.Symbol, score = topResult.score };

                if (topResult.Symbol != "behold_title")
                {
                    results.error = "Top row doesn't look like a behold title";
                }

                // split in 3, search for each separately
                Mat crew1 = query.SubMat(query.Rows * 2 / 8, (int)(query.Rows * 4.5 / 8), 30, query.Cols / 3);
                Mat crew2 = query.SubMat(query.Rows * 2 / 8, (int)(query.Rows * 4.5 / 8), query.Cols * 1 / 3 + 30, query.Cols * 2 / 3);
                Mat crew3 = query.SubMat(query.Rows * 2 / 8, (int)(query.Rows * 4.5 / 8), query.Cols * 2 / 3 + 30, query.Cols - 30);

                var c1 = MatchCrew(crew1);
                var c2 = MatchCrew(crew2);
                var c3 = MatchCrew(crew3);

                //crew1.SaveImage("crew1.png");
                //crew2.SaveImage("crew2.png");
                //crew3.SaveImage("crew3.png");

                // TODO: only do this part if valid (to not waste time)
                int starCount1 = 0;
                int starCount2 = 0;
                int starCount3 = 0;
                results.closebuttons = 0;
                if ((c1 != null) && (c2 != null) && (c3 != null))
                {
                    int starScale = 72;
                    float scale = query.Cols / 100;
                    Mat stars1 = query.SubMat((int)(scale * 9.2), (int)(scale * 12.8), 30, query.Cols / 3);
                    Mat stars2 = query.SubMat((int)(scale * 9.2), (int)(scale * 12.8), query.Cols * 1 / 3 + 30, query.Cols * 2 / 3);
                    Mat stars3 = query.SubMat((int)(scale * 9.2), (int)(scale * 12.8), query.Cols * 2 / 3 + 30, query.Cols - 30);

                    stars1 = stars1.Resize(new Size(stars1.Cols * starScale / stars1.Rows, starScale));
                    stars2 = stars2.Resize(new Size(stars2.Cols * starScale / stars2.Rows, starScale));
                    stars3 = stars3.Resize(new Size(stars3.Cols * starScale / stars3.Rows, starScale));

                    starCount1 = CountFullStars(stars1, _starFull);
                    starCount2 = CountFullStars(stars2, _starFull);
                    starCount3 = CountFullStars(stars3, _starFull);

                    // If there's a close button, this isn't a behold
                    int upperRightCorner = (int)(Math.Min(query.Rows, query.Cols) * 0.11);
                    Mat corner = query.SubMat(0, upperRightCorner, query.Cols - upperRightCorner, query.Cols);
                    corner = corner.Resize(new Size(78, 78));
                    results.closebuttons = CountFullStars(corner, _closeButton, 0.7);
                }

                results.crew1 = new SearchResult { symbol = c1.Symbol, score = c1.score, stars = starCount1 };
                results.crew2 = new SearchResult { symbol = c2.Symbol, score = c2.score, stars = starCount2 };
                results.crew3 = new SearchResult { symbol = c3.Symbol, score = c3.score, stars = starCount3 };

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during search: {ex.Message}");
                results.error = $"Exception during search: {ex.Message}";
                return results;
            }
        }

        private int CountFullStars(Mat refMat, Mat tplMat, double threshold = 0.8)
        {
            using (Mat res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
            {
                // Threshold out the faded stars
                Cv2.Threshold(refMat, refMat, 100, 1.0, ThresholdTypes.Tozero);

                Cv2.MatchTemplate(refMat, tplMat, res, TemplateMatchModes.CCorrNormed);
                Cv2.Threshold(res, res, threshold, 1.0, ThresholdTypes.Tozero);

                int numStars = 0;
                while (true)
                {
                    double minval, maxval;
                    Point minloc, maxloc;
                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                    if (maxval >= threshold)
                    {
                        numStars++;
                        // Fill in the res Mat so we don't find the same area again in the MinMaxLoc
                        Rect outRect;
                        Cv2.FloodFill(res, maxloc, new Scalar(0), out outRect, new Scalar(0.1), new Scalar(1.0), FloodFillFlags.Link4);
                    }
                    else
                    {
                        break;
                    }
                }

                return numStars;
            }
        }

        public SearchResults Search(string fileName)
        {
            return SearchImage(Cv2.ImRead(fileName));
        }

        public SearchResults SearchUrl(string imageUrl)
        {
            DownloadResult downloadResult = ImgDownload.Download(imageUrl);
            if (downloadResult.image != null)
            {
                return SearchImage(downloadResult.image);
            }

            return null;
        }
    }
}
