FROM kaerus/bootstrap

ENV ASPNET_ENV Development
ENV REMOTECACHE_FFMPEG_DIR /.bootstrap/ffmpeg
ENV OS_TYPE LINUX
EXPOSE 8080

COPY . /app/
WORKDIR /app

RUN dnu restore

ENTRYPOINT ["dnx", "web"]