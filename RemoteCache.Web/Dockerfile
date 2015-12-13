FROM microsoft/aspnet:latest

ENV ASPNET_ENV Development
ENV worker_host $WORKER_PORT_8500_TCP_ADDR

COPY . /app/
WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 8501
ENTRYPOINT ["dnx", "-p", "project.json", "web"]
