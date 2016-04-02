FROM kaerus/bootstrap

ENV ASPNET_ENV Development
ENV REMOTECACHE_FFMPEG_DIR /.bootstrap/ffmpeg
ENV REMOTECACHE_CACHE_DIR /app/RemoteCache.Worker/Cache
ENV OS_TYPE LINUX
EXPOSE 8081 8082

COPY . /app/
WORKDIR /app

ARG	SSL_DIR
RUN	sed "s|___SSL_DIR___|${SSL_DIR}|g" nginx.conf >> /etc/nginx/conf/nginx.conf

RUN dnu restore

ENTRYPOINT ["./run.sh"]