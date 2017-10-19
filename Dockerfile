FROM microsoft/dotnet:2.0.0-sdk-2.0.2

WORKDIR /dotnetapp

RUN apt-get update && apt-get install -y ffmpeg

# fix to SkiaSharp on Linux
RUN apt-get update && apt-get install -y libfontconfig1

# copy project.json and restore as distinct layers
COPY RemoteCache.fsproj .
RUN dotnet restore

# copy and build everything else
COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/RemoteCache.dll"]