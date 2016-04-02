FROM kaerus/bootstrap

ENV ASPNET_ENV Development
ENV REMOTECACHE_FFMPEG_DIR /.bootstrap/ffmpeg
ENV REMOTECACHE_CACHE_DIR /app/RemoteCache.Worker/Cache
ENV OS_TYPE LINUX
EXPOSE 8080

COPY . /app/
WORKDIR /app

RUN dnu restore

ENTRYPOINT ["./run.sh"]