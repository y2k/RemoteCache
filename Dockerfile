FROM microsoft/aspnet:latest

COPY . /app/
WORKDIR /app

RUN 	curl -O http://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz && \
	tar xf ffmpeg-release-64bit-static.tar.xz && \
	rm ffmpeg-release-64bit-static.tar.xz

ENV REMOTECACHE_FFMPEG_DIR /app/ffmpeg-2.8.4-64bit-static
ENV ASPNET_ENV Development

RUN 	curl -O "http://nginx.org/keys/nginx_signing.key" && \
	apt-key add nginx_signing.key && \
	echo "deb http://nginx.org/packages/debian/ wheezy nginx" >> /etc/apt/sources.list && \
	echo "deb-src http://nginx.org/packages/debian/ wheezy nginx" >> /etc/apt/sources.list && \
	apt-get -qq update && \
	apt-get -qqy install nginx && \
	mkdir -p /tmp/cache

COPY nginx.conf /etc/nginx/conf.d/

EXPOSE 8010 8012

RUN echo "#!/bin/bash \n nginx & \n dnx -p RemoteCache.Worker/project.json run & \n dnx -p RemoteCache.Web/project.json web" >> /run.sh && \
	chmod +x /run.sh

RUN ["dnu", "restore"]
ENTRYPOINT ["/run.sh"]
