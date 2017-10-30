FROM microsoft/dotnet:2.0.0-runtime

EXPOSE 8080
WORKDIR /app

# ffmpeg for convert gif to mp4
# RUN apt-get update && apt-get install -y ffmpeg
# fix to SkiaSharp on Linux
RUN apt-get update && apt-get install -y libfontconfig1

COPY out .
ENTRYPOINT ["dotnet", "RemoteCache.dll"]