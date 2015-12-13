FROM microsoft/aspnet:latest

COPY . /app/

RUN mkdir ffmpeg
RUN cd ffmpeg
RUN curl -O http://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz
RUN tar xf ffmpeg-release-64bit-static.tar.xz
RUN rm ffmpeg-release-64bit-static.tar.xz
RUN export REMOTECACHE_FFMPEG_DIR=$PWD/ffmpeg-2.8.3-64bit-static
RUN cd ..

WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 8500
ENTRYPOINT ["dnx", "-p", "project.json", "run"]