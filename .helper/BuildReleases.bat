@echo off
title Builder
set BuilderPath="%~dp0BuildHelper.exe"
if not exist %BuilderPath% (
    %BuilderPath% not found!
    pause
    exit
)
%BuilderPath% "%~dp0..\AppsLauncher" "%~dp0..\AppsDownloader" "%~dp0..\AppsLauncherUpdater"
exit