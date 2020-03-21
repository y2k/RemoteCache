FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-bionic

WORKDIR /app
COPY . /app

RUN dotnet publish -c Release -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.3-bionic

# ffmpeg for convert gif to mp4
RUN apt-get update && apt-get install -y ffmpeg
# fix to SkiaSharp on Linux
RUN apt-get update && apt-get install -y libfontconfig1

EXPOSE 8080

WORKDIR /app
COPY --from=0 /app/bin/Release/netcoreapp3.1/linux-x64/publish .

ENTRYPOINT ["dotnet", "RemoteCache.dll"]
