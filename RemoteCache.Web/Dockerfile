FROM microsoft/aspnet:latest

ENV ASPNET_ENV Development

RUN apt-get -qq update && apt-get -qqy install nginx
COPY nginx.config /etc/nginx/sites-enabled/
RUN mkdir -p /tmp/cache
RUN nginx -s reload 

COPY . /app/
WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 8501
ENTRYPOINT ["dnx", "-p", "project.json", "web"]
