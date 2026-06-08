@ECHO OFF

REM Get log file name with timestamp
for /f %%a in ('wmic os get LocalDateTime ^| findstr ^[0-9]') do (set ts=%%a)
set LogName="%cd%\logs\installEmkService%ts:~0,8%%ts:~8,4%%ts:~12,2%.log"

REM Install service
set servicePath="%cd%\EmkWinService.exe" 
"%cd%\installutil.exe" %servicePath% >> %LogName%

REM Make start automatically
sc config EmkService start= auto >> %LogName%

REM On crash, restart after 1 minute
sc failure EmkService actions= restart/60000/restart/60000// reset= 86400 >> %LogName%

REM Start the service
net start EmkService >> %LogName%