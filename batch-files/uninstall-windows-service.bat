sc stop "My Service"
timeout /t 5 /nobreak > NUL
sc delete "My Service"