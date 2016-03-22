FROM microsoft/aspnet:latest

ENV ASPNET_ENV Development
ENV REMOTECACHE_FFMPEG_DIR /app/ffmpeg
ENV OS_TYPE=LINUX
EXPOSE 8080

COPY . /app/
WORKDIR /app

RUN mkdir ffmpeg && \
	curl -O http://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz && \
	tar xf ffmpeg-release-64bit-static.tar.xz -C ffmpeg --strip-components=1 && \
	rm ffmpeg-release-64bit-static.tar.xz && \
	chmod +x run.sh

RUN ["dnu", "restore"]
ENTRYPOINT ["./run.sh"]