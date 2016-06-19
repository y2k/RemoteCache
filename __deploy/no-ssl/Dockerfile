FROM microsoft/dotnet:latest

ENV ASPNETCORE_ENVIRONMENT Development
ENV REMOTECACHE_FFMPEG_DIR /.bootstrap/ffmpeg
ENV OS_TYPE LINUX
EXPOSE 8081

COPY . /app/
WORKDIR /app

COPY nginx.conf /etc/nginx/conf/nginx.conf

RUN dnu restore

ENTRYPOINT ["./run.sh"]