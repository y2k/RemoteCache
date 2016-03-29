#!/bin/bash

nginx &

cd RemoteCache.Worker
dnx run &
cd ..

cd RemoteCache.Web
dnx web