FROM microsoft/aspnet:latest

ENV ASPNET_ENV Development

COPY . /app/
WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 8501
ENTRYPOINT ["dnx", "-p", "project.json", "web"]
