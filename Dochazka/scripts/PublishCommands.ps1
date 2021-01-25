
# 0. General variables
$rgName = "dp2020gaci"
$location = "West Europe"

# 1. SQL server and DB
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

$connectionString = "Server=tcp:dp2020gacisqlwe.database.windows.net,1433;Database=dochazkaDB;User ID=$sqlServerAdminUser;Password=$password;Encrypt=true;Connection Timeout=30;"


# 2. WebApp
$webAppServicePlan = "dp2020wasp"
$webAppName = "dp2020wa"
New-AzAppServicePlan -ResourceGroupName $rgName -Location $location -Name $webAppServicePlan -Tier Free
New-AzWebApp -ResourceGroupName $rgName -AppServicePlan $webAppServicePlan -Name $webAppName -Location $location 


$gitRepo = "https://github.com/xsubg001/dp2020.git"
$gitToken = Read-Host -Prompt "Enter GitHub token"

# SET GitHub
$PropertiesObject = @{    
    token = "$gitToken";
}
Set-AzResource -PropertyObject $PropertiesObject -ResourceId "/providers/Microsoft.Web/sourcecontrols/GitHub" -ApiVersion 2015-08-01  -Force

# Configure GitHub deployment from your GitHub repo and deploy once.
$PropertiesObject = @{
    repoUrl = "$gitRepo";
    branch = "develop";
    isManualIntegration = $false
}

Set-AzResource -PropertyObject $PropertiesObject -ResourceGroupName $rgName -ResourceType Microsoft.Web/sites/sourcecontrols `
    -ResourceName $webAppName/web -ApiVersion 2018-02-01 -Force



$webAppConnectionStrings = @{DefaultConnection = @{Type = "SQLAzure"; Value = $connectionString}}
Set-AzWebApp -ResourceGroupName $rgName -Name $webAppName -ConnectionStrings $webAppConnectionStrings


exit;

# package manager
rm -r Migrations
Add-Migration initialcreate
$env:ConnectionStrings:DefaultConnection = $connectionString
$env:ConnectionStrings:DefaultConnection = ""
Update-Database


