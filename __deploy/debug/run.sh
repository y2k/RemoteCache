#!/bin/bash

dnx -p RemoteCache.Worker/project.json run &

cd RemoteCache.Web
dnx web