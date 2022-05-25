@echo off

if "%~1" == "" (
	echo No number was given.
	goto exit_label
	)
	
set exe=%CD%\Debug\PR%num%.exe
set txt=%CD%\Debug\PR%num%.txt

echo Launch .EXE-file
echo %exe%
start %exe%

echo Open .TXT-file
echo %txt%
start %txt%

:exit_label

pause