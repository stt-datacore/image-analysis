/*
 Copyright (C) 2019-2020 TemporalAgent7 <https://github.com/TemporalAgent7>

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

using System.IO;
using System.Net;

namespace DataCore.Library
{
    public class DownloadResult
    {
        public Mat image { get; set; }
        public int size { get; set; }
    }

    public static class ImgDownload
    {
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

        public static DownloadResult Download(string imageUrl)
        {
            var result = new DownloadResult();
            result.size = -1;

            try
            {
                using (var client = new WebClient())
                {
                    using (BinaryReader reader = new BinaryReader(client.OpenRead(imageUrl)))
                    {
                        byte[] bufferImage = ReadAllBytes(reader);
                        result.size = bufferImage.Length;
                        result.image = Cv2.ImDecode(bufferImage, ImreadModes.Color);
                    }
                }
            }
            catch
            {
            }

            return result;
        }
    }
}