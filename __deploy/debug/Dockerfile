FROM microsoft/aspnet:latest

COPY . /app/
WORKDIR /app

RUN mkdir ffmpeg && \
	curl -O http://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz && \
	tar xf ffmpeg-release-64bit-static.tar.xz -C ffmpeg --strip-components=1 && \
	rm ffmpeg-release-64bit-static.tar.xz
ENV REMOTECACHE_FFMPEG_DIR /app/ffmpeg

RUN ["dnu", "restore"]

ENV ASPNET_ENV Development
EXPOSE 8080

ENTRYPOINT ["run.sh"]