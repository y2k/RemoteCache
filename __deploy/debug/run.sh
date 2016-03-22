#!/bin/bash

dnx -p RemoteCache.Worker/project.json run &
dnx -p RemoteCache.Web/project.json web