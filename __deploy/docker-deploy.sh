if [ "$#" != "2" ]; then
	echo "docker-deploy.sh <cache-dir> <hostname>"
	exit 1
fi

docker stop "remote-cache"

docker rm -f "remote-cache"
docker rmi -f "remote-cache"

docker build --build-arg SSL_DIR=/etc/letsencrypt/live/$2 -t "remote-cache" .

docker run -v /etc/letsencrypt:/etc/letsencrypt -v $(realpath $1):/app/Cache --name "remote-cache" -d -p 8011:8011 --restart on-failure "remote-cache"
