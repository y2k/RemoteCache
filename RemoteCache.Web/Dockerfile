FROM microsoft/aspnet:latest

ENV ASPNET_ENV Development

RUN apt-get -qq update && apt-get -qqy install nginx
COPY nginx.conf /etc/nginx/sites-enabled/nginx.conf
RUN mkdir -p /tmp/cache

COPY . /app/
WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 8010 8012

RUN echo "#!/bin/bash \n nginx & \n dnx -p project.json web" >> /run.sh
RUN chmod +x /run.sh
ENTRYPOINT ["/run.sh"]
