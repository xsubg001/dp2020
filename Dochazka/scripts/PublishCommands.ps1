$rgName = "dp2020gaci"
$location = "West Europe"
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

exit;

rm -r Migrations
Add-Migration initialcreate
$env:ConnectionStrings:DefaultConnection = $connectionString
$env:ConnectionStrings:DefaultConnection = ""
Update-Database


