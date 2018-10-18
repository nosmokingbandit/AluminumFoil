#!/bin/bash

GOARCH="amd64" go build -ldflags "-linkmode external -extldflags -static" -o "./releases/AluminumFoil.x86_64.exe"