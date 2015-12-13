FROM microsoft/aspnet:latest

COPY . /app/
WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 8501
ENTRYPOINT ["dnx", "-p", "project.json", "web"]