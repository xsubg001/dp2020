$rgName = "dp2020gaci"
$location = "West Europe"
$serverName = "dp2020gacisqlwe"
$dbName = "dochazkaDB"
$myIPaddress = "185.186.249.40"
$sqlServerAdminUser = "gabika"
$password = Read-Host -Prompt "Enter SQL server admin password"
$sqlServerAdminPassword = ConvertTo-SecureString -String $password -AsPlainText -Force
$sqlAdminCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $sqlServerAdminUser, $sqlServerAdminPassword





# az login
# az account show
# az group create --name dp2020 --location $location
# az sql server create --name dp2020we --resource-group dp2020 --location $location --admin-user gabika --admin-password "tbd"
# az sql server firewall-rule create --resource-group dp2020 --server dp2020we --name AllowAzureIps --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
# az sql server firewall-rule create --name AllowMyLocalDesktop --server dp2020we --resource-group dp2020 --start-ip-address=185.186.249.40 --end-ip-address=185.186.249.40
# az sql db create --resource-group dp2020 --server dp2020we --name diplomka --service-objective Basic
# az sql db show-connection-string --client ado.net --server $serverName --name $dbName

Connect-AzAccount -Tenant 1c23c01a-0a11-4849-a1bd-eddfe415c6d1 -Subscription bdd5ddce-ffd0-49dd-961a-d74ab44a262e
New-AzResourceGroup -Name $rgName -Location $location
New-AzSqlServer -ResourceGroupName $rgName -ServerName $serverName -Location $location -SqlAdministratorCredentials $sqlAdminCredential
New-AzSqlServerFirewallRule -FirewallRuleName AllowAzureIpsOnly -ResourceGroupName $rgName -ServerName $serverName -StartIpAddress 0.0.0.0 -EndIpAddress 0.0.0.0
New-AzSqlServerFirewallRule -FirewallRuleName AllowMyLocalDesktop -ResourceGroupName $rgName -ServerName $serverName -StartIpAddress $myIPaddress -EndIpAddress $myIPaddress
New-AzSqlDatabase -ResourceGroupName $rgName -ServerName $serverName -DatabaseName $dbName -Edition Basic
Get-AzSqlDatabase -ResourceGroupName $rgName -ServerName $serverName -DatabaseName $dbName

$connectionString = "Server=tcp:dp2020gacisqlwe.database.windows.net,1433;Database=dochazkaDB;User ID=$sqlServerAdminUser;Password=$password;Encrypt=true;Connection Timeout=30;"



exit;

rm -r Migrations
Add-Migration initialcreate
$env:ConnectionStrings:DefaultConnection = $connectionString
Update-Database


