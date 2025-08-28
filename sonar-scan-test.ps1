param(
    [string]$SonarHostUrl = "http://localhost:9000",
    [string]$SonarToken = "your-sonar-token",
    [string]$ProjectKey = "CleanArchitecture.Northwind",
    [string]$ProjectName = "CleanArchitecture.Northwind"
)

Write-Host "==== Step 1: SonarScanner Begin ====" -ForegroundColor Cyan
dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /n:"$ProjectName" `
    /d:sonar.host.url="$SonarHostUrl" `
    /d:sonar.login="$SonarToken" `
    /d:sonar.cs.vscoveragexml.reportsPaths="**/coverage.cobertura.xml" `
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/Migrations/**,**/wwwroot/lib/**,**/*.g.cs,**/*.designer.cs"

if ($LASTEXITCODE -ne 0) { throw "SonarScanner begin failed" }

Write-Host "==== Step 2: Build Solution ====" -ForegroundColor Cyan
dotnet build CleanArchitecture.Northwind.sln -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

Write-Host "==== Step 3: Run Tests with Coverage ====" -ForegroundColor Cyan
dotnet test tests --configuration Release `
    --collect:"XPlat Code Coverage" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
if ($LASTEXITCODE -ne 0) { throw "Tests failed" }

Write-Host "==== Step 4: SonarScanner End ====" -ForegroundColor Cyan
dotnet sonarscanner end /d:sonar.login="$SonarToken"
if ($LASTEXITCODE -ne 0) { throw "SonarScanner end failed" }

Write-Host "==== SonarQube Analysis Completed Successfully ====" -ForegroundColor Green
