killall mono

source $HOME/.dnx/dnvm/dnvm.sh
dnvm use -r mono 1.0.0-rc1-update1

export REMOTECACHE_FFMPEG_DIR=/url/bin

cd RemoteCache.Worker
nohup dnx run &

cd ../RemoteCache.Web
nohup dnx kestrel &