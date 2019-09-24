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
using Tesseract;

using System;
using System.IO;
using System.Net;

namespace DataCore.Library
{
    public struct ParsedSkill
    {
        public int SkillValue;
        public int Primary;
    }

    public struct VoyImageResult
    {
        public bool valid;
        public int antimatter;
        public ParsedSkill cmd;
        public ParsedSkill dip;
        public ParsedSkill eng;
        public ParsedSkill med;
        public ParsedSkill sci;
        public ParsedSkill sec;

        public override string ToString()
        {
            return $"AM: {antimatter}; cmd: {cmd.SkillValue}, {cmd.Primary}; dip: {dip.SkillValue}, {dip.Primary}; eng: {eng.SkillValue}, {eng.Primary}; med: {med.SkillValue}, {med.Primary}; sci: {sci.SkillValue}, {sci.Primary}; sec: {sec.SkillValue}, {sec.Primary}";
        }

        public string AsSearchString()
        {
            string primary = string.Empty;
            string secondary = string.Empty;
            string others = string.Empty;
            bool invalid = false;

            void scanSkill(ParsedSkill skill)
            {
                if (skill.Primary == 1)
                {
                    if (!string.IsNullOrEmpty(primary))
                    {
                        invalid = true; 
                    }
                    primary = skill.SkillValue.ToString();
                }
                else if (skill.Primary == 2)
                {
                    if (!string.IsNullOrEmpty(secondary))
                    {
                        invalid = true; 
                    }
                    secondary = skill.SkillValue.ToString();
                }
                else
                {
                    others += $"{skill.SkillValue} ";
                }
            };

            scanSkill(cmd);
            scanSkill(dip);
            scanSkill(eng);
            scanSkill(med);
            scanSkill(sci);
            scanSkill(sec);

            if (invalid)
            {
                return string.Empty;
            }

            return $"{primary} {secondary} {others}{antimatter}";
        }

        public static VoyImageResult Invalid()
        {
            VoyImageResult r = new VoyImageResult();
            r.valid = false;
            return r;
        }
    }

    public class VoyImage
    {
        private Mat _skill_cmd;
        private Mat _skill_dip;
        private Mat _skill_eng;
        private Mat _skill_med;
        private Mat _skill_sci;
        private Mat _skill_sec;
        private Mat _antimatter;
        private TesseractEngine _tessEngine;

        public VoyImage(string mainpath)
        {
            _skill_cmd = new Mat(Path.Combine(mainpath, "data", "cmd.png"));
            _skill_dip = new Mat(Path.Combine(mainpath, "data", "dip.png"));
            _skill_eng = new Mat(Path.Combine(mainpath, "data", "eng.png"));
            _skill_med = new Mat(Path.Combine(mainpath, "data", "med.png"));
            _skill_sci = new Mat(Path.Combine(mainpath, "data", "sci.png"));
            _skill_sec = new Mat(Path.Combine(mainpath, "data", "sec.png"));
            _antimatter = new Mat(Path.Combine(mainpath, "data", "antimatter.png"));

            var dataPath = Path.GetFullPath(Path.Combine(mainpath, "data", "tessdata"));
            _tessEngine = new TesseractEngine(dataPath, "Eurostile", EngineMode.Default);
            _tessEngine.DefaultPageSegMode = PageSegMode.SingleWord;
            _tessEngine.SetVariable("tessedit_char_whitelist", "0123456789");
            _tessEngine.SetVariable("classify_bln_numeric_mode", "1");
        }

        // TODO: share with Searcher
        private static byte[] ReadAllBytes(BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }

        public VoyImageResult SearchUrl(string imageUrl)
        {
            try
            {
                using (var client = new WebClient())
                {
                    lock (this)
                    {
                        using (BinaryReader reader = new BinaryReader(client.OpenRead(imageUrl)))
                        {
                            return SearchMat(Cv2.ImDecode(ReadAllBytes(reader), ImreadModes.Color));
                        }
                    }
                }
            }
            catch
            {
                return VoyImageResult.Invalid();
            }
        }

        public VoyImageResult SearchImage(string fileName)
        {
            return SearchMat(Cv2.ImRead(fileName));
        }

        private VoyImageResult SearchMat(Mat query)
        {
            try
            {
                VoyImageResult result = new VoyImageResult();

                // First, take the top of the image and look for the antimatter
                Mat top = query.SubMat(0, Math.Max(query.Rows / 5, 80), query.Cols / 3, query.Cols * 2 / 3);
                Cv2.Threshold(top, top, 100, 1.0, ThresholdTypes.Tozero);
                result.antimatter = MatchTop(top);
                if (result.antimatter == 0)
                {
                    // Not found
                    return VoyImageResult.Invalid();
                }

                if (result.antimatter > 8000)
                {
                    result.antimatter = result.antimatter / 10;
                }

                double standardScale = (double)query.Cols / query.Rows;
                double scaledPercentage = query.Rows * (standardScale * 1.2) / 9;

                Mat bottom = query.SubMat((int)(query.Rows - scaledPercentage), query.Rows, query.Cols / 6, query.Cols * 5 / 6);
                Cv2.Threshold(bottom, bottom, 100, 1.0, ThresholdTypes.Tozero);

                if (!MatchBottom(bottom, ref result))
                {
                    // Not found
                    return VoyImageResult.Invalid();
                }

                result.valid = true;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during VoyImage: {ex.Message}");
                return VoyImageResult.Invalid();
            }
        }

        private int MatchTop(Mat top)
        {
            int minHeight = top.Rows / 4;
            int maxHeight = top.Rows / 2;
            int stepHeight = top.Rows / 32;

            Point maxloc = new Point();
            int scaledWidth = 0;
            int height = minHeight;
            for (; height <= maxHeight; height += stepHeight)
            {
                Mat scaled = _antimatter.Resize(new Size(_antimatter.Cols * height / _antimatter.Rows, height));

                double maxval = ScaleInvariantTemplateMatch(top, scaled, out maxloc);
                //Console.WriteLine($"For height {height} we got {maxval} at location {maxloc.X} x {maxloc.Y}");

                if (maxval > 0.8)
                {
                    scaledWidth = scaled.Cols;
                    break;
                }
            }

            if (scaledWidth == 0)
            {
                return 0;
            }

            return OCRNumber(top.SubMat(maxloc.Y, maxloc.Y + height, maxloc.X + scaledWidth, maxloc.X + (int)(scaledWidth * 6.75)));
        }

        private bool MatchBottom(Mat bottom, ref VoyImageResult result)
        {
            int minHeight = bottom.Rows * 3 / 15;
            int maxHeight = bottom.Rows * 5 / 15;
            int stepHeight = bottom.Rows / 30;

            Point maxlocCmd = new Point();
            Point maxlocSci = new Point();
            int scaledWidth = 0;
            int height = minHeight;
            for (; height <= maxHeight; height += stepHeight)
            {
                Mat scaledCmd = _skill_cmd.Resize(new Size(_skill_cmd.Cols * height / _skill_cmd.Rows, height));
                Mat scaledSci = _skill_sci.Resize(new Size(_skill_sci.Cols * height / _skill_sci.Rows, height));

                double maxvalCmd = ScaleInvariantTemplateMatch(bottom, scaledCmd, out maxlocCmd);
                //Console.WriteLine($"For CMD height {height} we got {maxvalCmd} at location {maxlocCmd.X} x {maxlocCmd.Y}");

                double maxvalSci = ScaleInvariantTemplateMatch(bottom, scaledSci, out maxlocSci);
                //Console.WriteLine($"For SCI height {height} we got {maxvalSci} at location {maxlocSci.X} x {maxlocSci.Y}");

                if ((maxvalCmd > 0.9) && (maxvalSci > 0.9))
                {
                    scaledWidth = scaledSci.Cols;
                    break;
                }
            }

            if (scaledWidth == 0)
            {
                return false;
            }

            double widthScale = (double)scaledWidth / _skill_sci.Width;

            result.cmd.SkillValue = OCRNumber(bottom.SubMat(maxlocCmd.Y, maxlocCmd.Y + height, maxlocCmd.X - (scaledWidth * 5), maxlocCmd.X - (scaledWidth / 8)), "cmd");
            result.cmd.Primary = HasStar(bottom.SubMat(maxlocCmd.Y, maxlocCmd.Y + height, maxlocCmd.X + (scaledWidth * 9 / 8), maxlocCmd.X + (scaledWidth * 5 / 2)), "cmd");

            result.dip.SkillValue = OCRNumber(bottom.SubMat(maxlocCmd.Y + height, maxlocSci.Y, maxlocCmd.X - (scaledWidth * 5), (int)(maxlocCmd.X - (_skill_dip.Width - _skill_sci.Width)* widthScale)), "dip");
            result.dip.Primary = HasStar(bottom.SubMat(maxlocCmd.Y + height, maxlocSci.Y, maxlocCmd.X + (scaledWidth * 9 / 8), maxlocCmd.X + (scaledWidth * 5 / 2)), "dip");

            result.eng.SkillValue = OCRNumber(bottom.SubMat(maxlocSci.Y, maxlocSci.Y + height, maxlocCmd.X - (scaledWidth * 5), (int)(maxlocCmd.X - (_skill_eng.Width - _skill_sci.Width) * widthScale)), "eng");
            result.eng.Primary = HasStar(bottom.SubMat(maxlocSci.Y, maxlocSci.Y + height, maxlocCmd.X + (scaledWidth * 9 / 8), maxlocCmd.X + (scaledWidth * 5 / 2)), "eng");

            result.sec.SkillValue = OCRNumber(bottom.SubMat(maxlocCmd.Y, maxlocCmd.Y + height, (int)(maxlocSci.X + scaledWidth * 1.4), maxlocSci.X + (scaledWidth * 6)), "sec");
            result.sec.Primary = HasStar(bottom.SubMat(maxlocCmd.Y, maxlocCmd.Y + height, maxlocSci.X - (scaledWidth * 12 / 8), maxlocSci.X - (scaledWidth / 6)), "sec");

            result.med.SkillValue = OCRNumber(bottom.SubMat(maxlocCmd.Y + height, maxlocSci.Y, (int)(maxlocSci.X + scaledWidth * 1.4), maxlocSci.X + (scaledWidth * 6)), "med");
            result.med.Primary = HasStar(bottom.SubMat(maxlocCmd.Y + height, maxlocSci.Y, maxlocSci.X - (scaledWidth * 12 / 8), maxlocSci.X - (scaledWidth / 6)), "med");

            result.sci.SkillValue = OCRNumber(bottom.SubMat(maxlocSci.Y, maxlocSci.Y + height, (int)(maxlocSci.X + scaledWidth * 1.4), maxlocSci.X + (scaledWidth * 6)), "sci");
            result.sci.Primary = HasStar(bottom.SubMat(maxlocSci.Y, maxlocSci.Y + height, maxlocSci.X - (scaledWidth * 12 / 8), maxlocSci.X - (scaledWidth / 6)), "sci");

            return true;
        }

        private int HasStar(Mat skillImg, string skillName)
        {
            Mat center = skillImg.SubMat((skillImg.Rows / 2) - 10, (skillImg.Rows / 2) + 10, (skillImg.Cols / 2) - 10, (skillImg.Cols / 2) + 10);
            var mean = center.Mean();
            //Console.WriteLine($"For {skillName} : val0={mean.Val0}; val1={mean.Val1} ; val1={mean.Val2}");
            if (mean.Val0 + mean.Val1 + mean.Val2 < 10)
            {
                return 0;
            }
            else if (mean.Val0 < 5)
            {
                return 1; // Primary
            }
            else if (mean.Val0 + mean.Val1 + mean.Val2 > 100)
            {
                return 2; // Secondary
            }
            else
            {
                // not sure... hmmm
                return -1;
            }
        }

        // Copies in-memory pixels out of an OpenCV Mat into a PixData that Tesseract / Leptonica understands
        private unsafe void TransferData(MatOfByte3 mat3, PixData pixData)
        {
            var indexer = mat3.GetIndexer();

            for (int y = 0; y < mat3.Height; y++)
            {
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);
                for (int x = 0; x < mat3.Width; x++)
                {
                    Vec3b color = indexer[y, x];
                    PixData.SetDataFourByte(pixLine, x, BitmapHelper.EncodeAsRGBA(color.Item0, color.Item1, color.Item2, 255));
                }
            }
        }

        private int OCRNumber(Mat skillValue, string name = "")
        {
            if (!string.IsNullOrEmpty(name))
            {
                //skillValue.SaveImage($"{name}.png");
            }
            Pix pix;

            if (skillValue.Type() == MatType.CV_8UC3)
            {
                // 3 bytes
                pix = Pix.Create(skillValue.Width, skillValue.Height, 32);
                pix.XRes = 72;
                pix.YRes = 72;
                PixData pixData = null;
                try
                {
                    pixData = pix.GetData();
                    MatOfByte3 mat3 = new MatOfByte3(skillValue);
                    TransferData(mat3, pixData);
                }
                catch (Exception)
                {
                    pix.Dispose();
                    //throw;
                    return 0;
                }
            }
            else
            {
                skillValue.SaveImage("temp.png");
                pix = Pix.LoadFromFile("temp.png");
            }

            using (Page resultPage = _tessEngine.Process(pix))
            {
                string data = resultPage.GetText();

                if (int.TryParse(data, out int result))
                {
                    return result;
                }
            }

            return 0;
        }

        private double ScaleInvariantTemplateMatch(Mat refMat, Mat tplMat, out Point maxloc, double threshold = 0.8)
        {
            using (Mat res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
            {
                // Threshold out the faded stars
                Cv2.Threshold(refMat, refMat, 100, 1.0, ThresholdTypes.Tozero);

                Cv2.MatchTemplate(refMat, tplMat, res, TemplateMatchModes.CCorrNormed);
                Cv2.Threshold(res, res, threshold, 1.0, ThresholdTypes.Tozero);

                double minval, maxval;
                Point minloc;
                Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                return maxval;
            }
        }
    }
}
