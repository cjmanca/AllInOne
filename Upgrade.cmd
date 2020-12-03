@ECHO OFF
REM Run this file to upgrade the solution to a new version.

:start
SET /p NewVersionInput="Enter the new version number (X.X.X.X): "
ECHO.
FOR /F "tokens=*" %%i in ('RegExSelect "." "\d+\.\d+\.\d+\.\d+" "%NewVersionInput%"') do SET NewVersion=%%i
IF "%NewVersion%"=="" GOTO start
ECHO.
FOR /F "tokens=*" %%i in ('RegExReplace . "(\d+)\.(\d+)\.(\d+)\.(\d+)" "$1.$2.$4" "%NewVersion%"') do SET NewVersionMin=%%i

RegExReplace ".\BalancingPlugin\Properties\AssemblyInfo.cs" "^\[assembly: AssemblyVersion\("".*""\)" "[assembly: AssemblyVersion(""%NewVersion%"")"
RegExReplace ".\BalancingPlugin\Properties\AssemblyInfo.cs" "^\[assembly: AssemblyFileVersion\("".*""\)" "[assembly: AssemblyFileVersion(""%NewVersion%"")"

RegExReplace ".\BalancingPluginSetup\AddIn.xml" "<Version>.*</Version>" "<Version>%NewVersionMin%</Version>"
RegExReplace ".\BalancingPluginSetup\Server.ddf" "_\d+\.\d+\.\d+\.\d+\.wssx" "_%NewVersion%.wssx"
RegExReplace ".\BalancingPluginSetup\Server.wxs" " ProductVersion="".*""" " ProductVersion=""%NewVersionMin%"""

RegExReplace ".\BalancingPluginWindowsSetup\BalancingPlugin.wxs" " ProductVersion="".*""" " ProductVersion=""%NewVersionMin%"""
RegExReplace ".\BalancingPluginWindowsSetup\BalancingPluginWindowsSetup.wixproj" "_\d+\.\d+\.\d+\.\d+</OutputName>" "_%NewVersion%</OutputName>"

REM If you want to sign your plug-ins, create a Sign.cmd to do that and uncomment this line to upgrade it as well.
REM RegExReplace "Sign.cmd" "_\d+\.\d+\.\d+\.\d+(?=[._])" "_%NewVersion%"

:end
ECHO Done.
ECHO.
ECHO Open the solution and build it.
ECHO.
PAUSE