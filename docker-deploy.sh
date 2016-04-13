if [ "$#" != "2" ]; then
	if [ "$#" != "3" ]; then
		echo "docker-deploy.sh <development|production|no-ssl> <cache-dir> <hostname>"
		exit 1
	fi
fi

CACHE_PATH=$2
HOSTNAME=$3

docker stop "remote-cache"
docker rm -f "remote-cache"
docker rmi -f "remote-cache"

rm -f Dockerfile
rm -f nginx.conf
rm -f run.sh

if [ "$1" == "development" ]; then
	cp __deploy/development/Dockerfile .
	cp __deploy/development/run.sh .

	docker build -t "remote-cache" .
	docker run -v $CACHE_PATH:/app/cache --name "remote-cache" -d -p 8011:8080 "remote-cache"
elif [ "$1" == "production" ]; then
	cp __deploy/production/Dockerfile .
	cp __deploy/production/run.sh .
	cp __deploy/production/nginx.conf .

	docker build --build-arg SSL_DIR=/etc/letsencrypt/live/$HOSTNAME -t "remote-cache" .
	docker run -v /etc/letsencrypt:/etc/letsencrypt -v $CACHE_PATH:/app/cache --name "remote-cache" -d -p 8011:8081 -p 80:8082 --restart on-failure "remote-cache"
elif [ "$1" == "no-ssl" ]; then
	cp __deploy/no-ssl/Dockerfile .
	cp __deploy/no-ssl/run.sh .
	cp __deploy/no-ssl/nginx.conf .

	docker build -t "remote-cache" .
	docker run -v /etc/letsencrypt:/etc/letsencrypt -v $CACHE_PATH:/app/cache --name "remote-cache" -d -p 80:8081 --restart on-failure "remote-cache"
fi

rm -f Dockerfile
rm -f nginx.conf
rm -f run.sh