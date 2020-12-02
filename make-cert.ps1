dotnet dev-certs https -v -ep c:\temp\cert-aspnetcore.pfx -p ufo
dotnet dev-certs https -v -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p ufo
dotnet dev-certs https --trust