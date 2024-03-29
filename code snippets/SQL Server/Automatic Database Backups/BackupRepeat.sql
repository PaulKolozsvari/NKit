DECLARE @MyFileName varchar(1000)
SELECT @MyFileName = (SELECT N'C:\Repeat\RepeatDBBackups\Repeat' + convert(varchar(500),GetDate(),112) + '.bak') 

BACKUP DATABASE [Repeat] TO  DISK = @MyFileName WITH  COPY_ONLY, NOFORMAT, INIT,  NAME = N'Repeat-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10
GO
declare @backupSetId as int
DECLARE @MyFileName varchar(1000)
SELECT @MyFileName = (SELECT N'C:\Repeat\RepeatDBBackups\Repeat' + convert(varchar(500),GetDate(),112) + '.bak') 

select @backupSetId = position from msdb..backupset where database_name=N'Repeat' and backup_set_id=(select max(backup_set_id) from msdb..backupset where database_name=N'Repeat' )
if @backupSetId is null begin raiserror(N'Verify failed. Backup information for database ''Repeat'' not found.', 16, 1) end
RESTORE VERIFYONLY FROM  DISK = @MyFileName WITH  FILE = @backupSetId,  NOUNLOAD,  NOREWIND
GO
