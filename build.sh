#!/bin/bash

GOARCH="amd64" go build -ldflags "-linkmode external -extldflags -static" -o "./releases/Windows.x64/AluminumFoil$ext"