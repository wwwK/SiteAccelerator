dotnet publish -c Release -r win-x86 /p:PublishSingleFile=true /p:PublishTrimmed=true -o app
copy appsettings.json app\appsettings.json