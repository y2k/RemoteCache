FROM kaerus/bootstrap

ENV ASPNET_ENV Development
ENV REMOTECACHE_FFMPEG_DIR /app/ffmpeg
ENV OS_TYPE LINUX
EXPOSE 8081

COPY . /app/
WORKDIR /app

RUN apt-get -qq update && \
    apt-get -qqy install wget make && \
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
RUN	sed "s|___SSL_DIR___|${SSL_DIR}|g" nginx.conf >> /etc/nginx/conf/nginx.conf

RUN dnu restore

ENTRYPOINT ["./run.sh"]