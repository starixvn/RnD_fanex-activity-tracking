if not exist .git\hooks\pre-push copy build\pre-push .git\hooks\

"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" build\BuildTasks.Dev.targets /fl /flp:logfile=BuildTask.Dev.log;verbosity=normal

exit
