if [ "$#" != "2" ]; then
	if [ "$#" != "3" ]; then
		echo "docker-deploy.sh <debug|production> <cache-dir> <hostname>"
		exit 1
	fi
fi

docker stop "remote-cache"
docker rm -f "remote-cache"
docker rmi -f "remote-cache"

rm -f Dockerfile
rm -f nginx.conf
rm -f run.sh

CACHE_PATH=$2

if [ "$1" == "debug" ]; then
	cp __deploy/debug/Dockerfile .
	cp __deploy/debug/run.sh .

	docker build -t "remote-cache" .
	docker run -v $CACHE_PATH:/app/Cache --name "remote-cache" -d -p 8011:8080 "remote-cache"
elif [ "$1" == "production" ]; then
	docker build --build-arg SSL_DIR=/etc/letsencrypt/live/$3 -t "remote-cache" .
	docker run -v /etc/letsencrypt:/etc/letsencrypt -v $(realpath $2):/app/Cache --name "remote-cache" -d -p 8081:8011 --restart on-failure "remote-cache"
fi

rm -f Dockerfile
rm -f nginx.conf
rm -f run.sh