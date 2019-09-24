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
using System.Collections.Generic;

using OpenCvSharp;
using OpenCvSharp.XFeatures2D;

namespace DataCore.Library
{
    class SURFDescriptor
    {
        private Feature2D _featureDetector;
        public SURFDescriptor()
        {
            //_featureDetector = ORB.Create(nFeatures: 200, edgeThreshold: 40, patchSize: 40, wtaK: 3);
            _featureDetector = SURF.Create(1200); // better but a lot slower
        }
        public Mat Describe(Mat image, out KeyPoint[] keypoints)
        {
            Mat descriptors = new Mat();
            _featureDetector.DetectAndCompute(image, null, out keypoints, descriptors);
            return descriptors;
        }

        public IEnumerable<Mat> DescribeMany(IEnumerable<Mat> images, out KeyPoint[][] keypoints)
        {
            List<Mat> descriptors = new List<Mat>();
            KeyPoint[][] keypoints2 = _featureDetector.Detect(images);
            _featureDetector.Compute(images, ref keypoints2, descriptors);
            keypoints = keypoints2;
            return descriptors;
        }
    }
}
