docker stop "remote-cache"

docker rm -f "remote-cache"
docker rmi -f "remote-cache"

docker build -t "remote-cache" .

cd ..
docker run -v $1:/etc/nginx/ssl -v /tmp/remote-cache:/app/Cache --name "remote-cache" -d -p 8010-8012:8010-8012 "remote-cache"
