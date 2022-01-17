param(
  [Parameter(Mandatory = $true, Position = 0)]
  [string]$targetFramework
)

dotnet clean -c Release

$repositoryUrl = "https://github.com/$env:GITHUB_REPOSITORY"

# Building for a specific framework.
dotnet build .\ElmahCore -c Release -f $targetFramework /p:RepositoryUrl=$repositoryUrl
dotnet build .\ElmahCore.Mvc -c Release -f $targetFramework /p:RepositoryUrl=$repositoryUrl
dotnet build .\ElmahCore.MySql -c Release -f $targetFramework /p:RepositoryUrl=$repositoryUrl
dotnet build .\ElmahCore.MsSql -c Release -f $targetFramework /p:RepositoryUrl=$repositoryUrl
dotnet build .\ElmahCore.Postgresql -c Release -f $targetFramework /p:RepositoryUrl=$repositoryUrl