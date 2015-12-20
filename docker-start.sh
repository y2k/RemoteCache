docker rm -f "remote-cache"
docker run -v /tmp/remote-cache:/app/Cache --name "remote-cache" -d -p 8010-8012:8010-8012 "remote-cache"
