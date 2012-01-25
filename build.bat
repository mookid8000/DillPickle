@echo off
set msbuild=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
%msbuild% scripts\build.proj /p:fx=NET35
%msbuild% scripts\build.proj /p:fx=NET40
