@echo off
msbuild scripts\build.proj /p:fx=NET35
msbuild scripts\build.proj /p:fx=NET40
