mkdir -p publish
root="$(pwd)"
cd src/TodaysAbsences.Lambda
dotnet publish -c Release
cd bin/Release/netcoreapp2.0/publish
zip -r "$root/publish/deploy-package.zip" *
cd "$root"
