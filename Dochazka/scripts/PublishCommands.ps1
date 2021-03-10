
# 0. General variables
$rgName = "dp2020gaci"
$location = "West Europe"

# 1. User Story 22: Vytvoření služby Azure SQL Server Database
$serverName = "dp2020gacisqlwe"
$dbName = "dochazkaDB"
$myIPaddress = "185.186.249.40"
$sqlServerAdminUser = Read-Host -Prompt "Enter SQL server admin userName"
$password = Read-Host -Prompt "Enter SQL server admin password"
$sqlServerAdminPassword = ConvertTo-SecureString -String $password -AsPlainText -Force
$sqlAdminCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $sqlServerAdminUser, $sqlServerAdminPassword

Connect-AzAccount -Tenant 1c23c01a-0a11-4849-a1bd-eddfe415c6d1 -Subscription bdd5ddce-ffd0-49dd-961a-d74ab44a262e
New-AzResourceGroup -Name $rgName -Location $location
New-AzSqlServer -ResourceGroupName $rgName -ServerName $serverName -Location $location -SqlAdministratorCredentials $sqlAdminCredential
New-AzSqlServerFirewallRule -FirewallRuleName AllowAzureIpsOnly -ResourceGroupName $rgName -ServerName $serverName -StartIpAddress 0.0.0.0 -EndIpAddress 0.0.0.0
New-AzSqlServerFirewallRule -FirewallRuleName AllowMyLocalDesktop -ResourceGroupName $rgName -ServerName $serverName -StartIpAddress $myIPaddress -EndIpAddress $myIPaddress
New-AzSqlDatabase -ResourceGroupName $rgName -ServerName $serverName -DatabaseName $dbName -Edition Basic
Get-AzSqlDatabase -ResourceGroupName $rgName -ServerName $serverName -DatabaseName $dbName

$connectionString = "Server=tcp:$serverName.database.windows.net,1433;Database=$dbName;User ID=$sqlServerAdminUser;Password=$password;Encrypt=true;Connection Timeout=30;"
$connectionString


# 2. User Story 23: Vytvoření služby Azure App Service
$webAppServicePlan = "dp2020wasp"
$webAppName = "dp2020wa"
New-AzAppServicePlan -ResourceGroupName $rgName -Location $location -Name $webAppServicePlan -Tier Free
New-AzWebApp -ResourceGroupName $rgName -AppServicePlan $webAppServicePlan -Name $webAppName -Location $location 

# 3. User Story 44: Integrace Azure Web App se službou GitHub pro přístup ke zdrojovým kódům aplikace a jejich nasazení do App Service
$gitToken = Read-Host -Prompt "Enter GitHub token"
$PropertiesObject = @{    
    token = "$gitToken";
}

Set-AzResource -PropertyObject $PropertiesObject -ResourceId "/providers/Microsoft.Web/sourcecontrols/GitHub" -ApiVersion 2018-02-01 -Force

# Configure GitHub deployment from your GitHub repo and deploy once.
$gitRepoURL = "https://github.com/xsubg001/dp2020.git"
$PropertiesObject = @{
    repoUrl = "$gitRepoURL";
    branch = "master";
    isManualIntegration = $false
}

Set-AzResource -PropertyObject $PropertiesObject -ResourceGroupName $rgName -ResourceType Microsoft.Web/sites/sourcecontrols `
    -ResourceName $webAppName/web -ApiVersion 2018-02-01 -Force

# 4. User Story 45: Integrace služeb Azure App Service a Azure SQL Database 
$webAppConnectionStrings = @{
    DefaultConnection = @{
        Type = "SQLAzure";
        Value = $connectionString
    }
}

Set-AzWebApp -ResourceGroupName $rgName -Name $webAppName -ConnectionStrings $webAppConnectionStrings

# zde končí část nastavení Azure
exit;

# 5. User Story 46: Migrace modelu databáze do instance služby Azure SQL Database
# nutno provést v lokálním package manageru Visual Studia
rm -r Migrations
Add-Migration initialcreate
$env:ConnectionStrings:DefaultConnection = $connectionString
Update-Database
# $env:ConnectionStrings:DefaultConnection = "" # volat pouze podle potřeby k resetování Connection Stringu



