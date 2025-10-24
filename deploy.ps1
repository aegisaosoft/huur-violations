# Azure Deployment Script for HuurViolations.Api
# Make sure you're logged in with: az login

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [string]$AppServicePlan = "DefaultAppServicePlan"
)

Write-Host "🚀 Starting deployment of HuurViolations.Api to Azure..." -ForegroundColor Green

# Step 1: Build the project
Write-Host "📦 Building the project..." -ForegroundColor Yellow
dotnet build HuurViolations.Api -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Step 2: Publish the project
Write-Host "📤 Publishing the project..." -ForegroundColor Yellow
dotnet publish HuurViolations.Api -c Release -o ./publish --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Publish failed!" -ForegroundColor Red
    exit 1
}

# Step 3: Create deployment package
Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow
if (Test-Path "./deployment.zip") {
    Remove-Item "./deployment.zip"
}
Compress-Archive -Path "./publish/*" -DestinationPath "./deployment.zip"

# Step 4: Deploy to Azure
Write-Host "☁️ Deploying to Azure App Service..." -ForegroundColor Yellow
az webapp deployment source config-zip --resource-group $ResourceGroupName --name $AppServiceName --src ./deployment.zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Deployment successful!" -ForegroundColor Green
    Write-Host "🌐 Your app should be available at: https://$AppServiceName.azurewebsites.net" -ForegroundColor Cyan
} else {
    Write-Host "❌ Deployment failed!" -ForegroundColor Red
}

# Cleanup
Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
Remove-Item -Recurse -Force "./publish" -ErrorAction SilentlyContinue
Remove-Item "./deployment.zip" -ErrorAction SilentlyContinue
