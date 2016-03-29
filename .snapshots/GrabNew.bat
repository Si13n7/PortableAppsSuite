::+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
::-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
::                                                           -+
::       _________.__  ____ ________        _________        +-
::      /   _____/|__|/_   |\_____  \   ____\______  \       -+
::      \_____  \ |  | |   |  _(__  <  /    \   /    /       +-
::      /        \|  | |   | /       \|   |  \ /    /        -+
::     /_______  /|__| |___|/______  /|___|  //____/         +-
::             \/                  \/      \/                -+
::                                                           +-
::                  D E V E L O P M E N T S                  -+
::                                                           +-
::+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
::-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
:: www.si13n7.com


@echo off

title Snapshot Helper

for /f %%i in ('""%~dp0..\.helper\DateTime.exe"" yyyy-MM-dd') do set dateVar=%%i
for /f %%i in ('""%~dp0..\.helper\DateTime.exe"" HH:mm:ss.fff') do set timeVar=%%i
set fileTimeVar=%timeVar::=.%

copy /y "%~dp0Template\PortableAppsSuite_Snapshot.ini" "%~dp0PortableAppsSuite_%dateVar%_%fileTimeVar%.ini"

"%~dp0..\.helper\IniWriter.exe" SNAPSHOT Date "%dateVar%" "%~dp0PortableAppsSuite_%dateVar%_%fileTimeVar%.ini"
"%~dp0..\.helper\IniWriter.exe" SNAPSHOT Time "%timeVar%" "%~dp0PortableAppsSuite_%dateVar%_%fileTimeVar%.ini"

call "%~dp0..\.helper\7zHelper.bat" a -t7z """%~dp0PortableAppsSuite_%dateVar%_%fileTimeVar%.7z""" """%~dp0Template\PortableAppsSuite_Snapshot\*""" -ms -mmt -mx=9

"%~dp0..\.helper\IniWriter.exe" INFO LastStamp "PortableAppsSuite_%dateVar%_%fileTimeVar%" "%~dp0Last.ini"
"%~dp0..\.helper\FileHasher.exe" MD5 "%~dp0PortableAppsSuite_%dateVar%_%fileTimeVar%.ini" "%~dp0PortableAppsSuite_%dateVar%_%fileTimeVar%.7z"

exit /b