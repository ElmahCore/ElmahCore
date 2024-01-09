Push-Location -Path .\ui

npm ci
npm run build

Pop-Location

Remove-Item -Path .\artifacts -Recurse -Force -ErrorAction SilentlyContinue

dotnet clean -c Release
dotnet build -c Release
