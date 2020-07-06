# DataCore Bot
Discord / Reddit bot for Star Trek Timelines, using image recognition and OCR to parse behold / voyage screenshots as well as provide crew, items and dilemma information.

## Help needed
If you want to see the bot improve or just want to practice some coding skills, please consider submitting a PR. If you don't have ideas for what to work on, pick from this list of known issues / TODOs:
- [ ] Use [Discord.Net commands](https://github.com/discord-net/Discord.Net) instead of manually parsing input
- [ ] Improve edge case recommendations (when all 3 crew are same tier, when one or two are FF, etc.)
- [ ] Use Tesseract OCR on the Behold images to distinguish between crew states (Not owned vs. Frozen - needs to work on all supported languages like German, Spanish, etc.)
- [ ] Improve "best" search (support for variable number of skills, etc.)
- [ ] Improve fuzzy search (Levenshtein, etc.) to identify common misspells 
- [ ] Try to detect screenshots that include a Windows titlebar (Windows 10 app or Steam) and strip it out to improve recognition
- [ ] Investigate / experiment with using different Feature Detector algorithms with different settings to get better accuracy for smaller descriptor matrix size (reduce RAM usage, increase perf)
- [ ] Upgrade to [.NET Core 3.0](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-0/) once dependencies are available
- [ ] Localization support
- [ ] Add proper publish scripts / documentation, with optional Docker support (and sharding for better scaling)
- [ ] More tests, comments and documentation

## Setup

### Windows

All dependencies should come via NuGet, no custom builds necessary.

### Linux

This was tested on x64 Ubuntu 18.04.

#### OpenCV

See instructions [here](https://www.learnopencv.com/install-opencv-4-on-ubuntu-18-04/). Build opencv with **OPENCV_ENABLE_NONFREE**

```
~/opencv/build$ cmake -D CMAKE_BUILD_TYPE=RELEASE -D CMAKE_INSTALL_PREFIX=/usr/local -D INSTALL_C_EXAMPLES=ON -D WITH_TBB=ON -D WITH_V4L=ON -D WITH_QT=ON -D WITH_OPENGL=ON -D OPENCV_EXTRA_MODULES_PATH=../../opencv_contrib/modules -D BUILD_EXAMPLES=ON .. -DENABLE_PRECOMPILED_HEADERS=OFF -DOPENCV_ENABLE_NONFREE=ON
```

#### [7/5] Building with OpenCV 4.3 from source
```
git clone https://github.com/shimat/opencvsharp.git

manually edit \opencvsharp\src\OpenCvSharpExtern\xfeatures2d.h and replace xfeatures2d::SIFT with SIFT

mkdir build && cd build
cmake ..
make -j8
sudo make install
sudo ldconfig
```
Remove the runtime reference from the csproj [here](https://github.com/TemporalAgent7/datacore-bot/blob/master/src/DataCore.Library/DataCore.Library.csproj#L16).

#### Tesseract OCR

Build and install Leptonica (this particular tag which is compatible with the NuGet library)
```
git clone https://github.com/DanBloomberg/leptonica.git
cd leptonica
git checkout 1.75.3
mkdir build
cd build
cmake ..
make
sudo make install
```

Build and install Tesseract
```
git clone https://github.com/tesseract-ocr/tesseract.git
cd tesseract
git checkout 3.05
./autogen.sh
./configure
make
sudo make install
sudo ldconfig
```

Symlink the built libraries in your deployment location. For example:

```
cd datacore-behold/src/DataCore.CLI/bin/Debug/netcoreapp2.1/x64
ln -s /usr/local/lib/libtesseract.so.3.0.5 libtesseract3052.so
ln -s /usr/local/lib/libleptonica.so.1.75.3 liblept1753.so
```

## Build

First, clone the repo to your machine; clone it in the same folder as the main [DataCore](https://github.com/TemporalAgent7/datacore) repo for best default configration:
```
git clone https://github.com/TemporalAgent7/datacore-behold.git
cd datacore-behold
```

To install and build, go to the folder where you cloned the repo and run:
```
dotnet restore
dotnet build
```

To train crew descriptors:
```
cd src/DataCore.CLI
dotnet run train --noimages <path_to_datacore>
```

## Deployment

**TODO** More details

### Configuration

There are a few options that need to be set up for the bot to connect to Discord and/or Reddit in /src/DataCore.Daemon/appsettings.json ; specifically:
* DISCORD_TOKEN: the discord application token
* REDDIT_CLIENT_ID, REDDIT_CLIENT_SECRET, REDDIT_REFRESH_TOKEN: authentication tokens for reddit; you can use the /src/DataCore.Daemon/prawtoken.py script to get these values for your reddit bot account.

### Publish and deploy

You need to run the daemon, preferrably with an auto-restarting process monitor like PM2. Something like this will work (this runs the debug build by default, which is not what you want in a real deployment scenario):

```
dotnet build
cd src/DataCore.Daemon/
pm2 start "dotnet run" --name "DataCoreBot"
```

## License
This code is released under the GPLv3 open source license. See the [LICENSE](/LICENSE) file or this plain english [TLDR](https://tldrlegal.com/license/gnu-general-public-license-v3-(gpl-3)) for details.
