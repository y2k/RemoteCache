#!/bin/bash

cd RemoteCache.Worker
dnx run &
cd ..

cd RemoteCache.Web
dnx web