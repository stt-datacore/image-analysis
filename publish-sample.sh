MAIN_PATH=/home/user/datacore-bot

sudo service supervisor stop

pushd $MAIN_PATH
rm -rf $MAIN_PATH/out
git pull
dotnet publish src/DataCore.Daemon/DataCore.Daemon.csproj -c Release -o $MAIN_PATH/out
ln -s /usr/local/lib/libtesseract.so.3.0.5 $MAIN_PATH/out/x64/libtesseract3052.so
ln -s /usr/local/lib/libleptonica.so.1.75.3 $MAIN_PATH/out/x64/liblept1753.so
popd

sudo service supervisor start
