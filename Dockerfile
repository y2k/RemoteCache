FROM microsoft/aspnet:latest

COPY . /app/
WORKDIR /app

RUN mkdir ffmpeg && \
	curl -O http://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz && \
	tar xf ffmpeg-release-64bit-static.tar.xz -C ffmpeg --strip-components=1 && \
	rm ffmpeg-release-64bit-static.tar.xz

ENV REMOTECACHE_FFMPEG_DIR /app/ffmpeg
ENV ASPNET_ENV Development

RUN apt-get -qq update && \
    apt-get -qqy install wget && \
    mkdir -p /tmp/cache && \
    mkdir build-nginx && \
    cd build-nginx && \
    wget http://www.openssl.org/source/openssl-1.0.2g.tar.gz && \
    wget http://zlib.net/zlib-1.2.8.tar.gz && \
    wget http://nginx.org/download/nginx-1.9.12.tar.gz && \
    tar -xvzf openssl-1.0.2g.tar.gz && \
    tar -xvzf zlib-1.2.8.tar.gz && \
    tar -xvzf nginx-1.9.12.tar.gz && \
    cd nginx-1.9.12 && \
    ./configure --prefix=/etc/nginx --sbin-path=/usr/sbin/nginx --with-zlib=../zlib-1.2.8 --without-http_rewrite_module --with-http_ssl_module  --with-http_v2_module --with-debug --with-openssl=../openssl-1.0.2g && \
    make && \
    make install && \
    rm /etc/nginx/conf/nginx.conf

ARG	SSL_DIR
RUN	sed "s|___SSL_DIR___|${SSL_DIR}|g" nginx.conf >> /etc/nginx/conf.d/nginx.conf

EXPOSE 8010 8011 8012

RUN echo "#!/bin/bash \n nginx & \n dnx -p RemoteCache.Worker/project.json run & \n dnx -p RemoteCache.Web/project.json web" >> /run.sh && \
	chmod +x /run.sh

RUN ["dnu", "restore"]
ENTRYPOINT ["/run.sh"]
