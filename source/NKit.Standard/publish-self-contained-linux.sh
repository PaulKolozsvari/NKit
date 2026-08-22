#!/bin/bash
dotnet clean
dotnet publish -c Release -r linux-x64 --self-contained true
