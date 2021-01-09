:: This batch file needs to be copied to the sme directory as the exe path that is being installed as a windows service.
    :: %~dp0 in front of the exe path provides the current directory path. Without it the the windows service will be installed with only the file name and therefore Windows will not be able to find the file to start the Windows service.
sc create "My Service" binPath= %~dp0MyService.exe
sc failure "My Service"" actions= restart60000restart6000060000 reset= 86400
sc start "My Service"
sc config "My Service" start=auto