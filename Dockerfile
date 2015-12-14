FROM microsoft/aspnet:latest

COPY . /app/
WORKDIR /app
RUN ["dnu", "restore"]

RUN mkdir ffmpeg && \
	cd ffmpeg && \
	curl -O http://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz && \
	tar xf ffmpeg-release-64bit-static.tar.xz && \
	rm ffmpeg-release-64bit-static.tar.xz

ENV REMOTECACHE_FFMPEG_DIR $PWD/ffmpeg-2.8.3-64bit-static
ENV ASPNET_ENV Development

RUN apt-get -qq update && \
	apt-get -qqy install nginx && \
	mkdir -p /tmp/cache

COPY nginx.conf /etc/nginx/sites-enabled/nginx.conf

EXPOSE 8010 8012

RUN echo "#!/bin/bash \n nginx & \n dnx -p RemoteCache.Worker/project.json run & \n dnx -p RemoteCache.Web/project.json web" >> /run.sh && \
	chmod +x /run.sh
ENTRYPOINT ["/run.sh"]
