<#
This is used to modify Last Update timestamp for Excel and Word documents. It is using Set-Date function to adjust temporarily computer system date.
Then, it reopens and saves the selected office documents.
By default, C:\Temp folder is scanned for *.doc, *.docx, *.xls, *.xlsx and those are opened and saved with actual system date.
If option "-antidateEnabled" is on, it must be run in Powershell Admin console, because Set-Date function requires Admin permissions. It may also require:
"Set-ExecutionPolicy -ExecutionPolicy Unrestricted" to be able to run this script

Run as following: 
    1. cd <folder-containing-this-script>
    2a. Either in normal Powershell console/ISE window:
    .\Set-OfficeDocumentProperty.ps1 -path C:\MyDocuments

    2b. Or in admin/elevated Powershell console/ISE window with "antidateEnabled" parameter set:
    .\Set-OfficeDocumentProperty.ps1 -antidateEnabled -newOldDate 2018-06-18 
    .\Set-OfficeDocumentProperty.ps1 -antidateEnabled -newOldDate 2018-06-18 -path C:\MyDocuments
#>

Param(
    $path = "C:\temp",
    [array]$include = @("*.doc", "*.docx", "*.xls", "*.xlsx"),

    [Parameter(ParameterSetName = 'Antidate')]
    [DateTime]$newOldDate = "2016-06-18", # YYYY-MM-DD

    [Parameter(ParameterSetName = 'Antidate')]
    [switch]$antidateEnabled
)

if ($antidateEnabled) {        
    $originalDate = Get-Date
    $maxNewOldDate = [DateTime]"$($newOldDate.Year)-$($newOldDate.Month)-$($newOldDate.Day) 23:59:59"
    $randomTicks = Get-Random -Minimum $newOldDate.Ticks -Maximum $maxNewOldDate.Ticks
    $newRandomDate = New-Object DateTime($randomTicks)
    write-warning "New document Last Update timestamp to be applied: $newRandomDate"
    Read-Host "Press Enter to continue ... "
    $current = Get-Date
    set-date $newRandomDate | Out-Null
}

$docs = Get-childitem -path $Path -Recurse -Include $include 

#Create Word application
$wordApplication = New-Object -ComObject Word.application
$excelApplication = New-Object -ComObject Excel.application

Foreach ($doc in $docs) {
    write-host "`r`nUpdating file: $($doc.FullName)"
    
    if ($doc.Name -match ".xls") {
        $application = $excelApplication
        #Get reference to Excel doc
    $document = $application.WorkBooks.open($doc.FullName);
    }
    else {
        $application = $wordApplication
        #Get reference to word doc
        $document = $application.documents.open($doc.FullName);
    }    

    #set up binding flags for custom properties
    $binding = "System.Reflection.BindingFlags" -as [type];        
    $customProperties = $document.BuiltInDocumentProperties

    [Array]$propertyName = "Comments"
    # [Array]$propertyName = "Creation Date"
    # [Array]$propertyName = "Last Save Time"
    [Array]$propertyValue = (Get-Date -Date $newOldDate).Ticks
    
    #Get property value
    $myProperty = [System.__ComObject].InvokeMember("Item", $binding::GetProperty, $null, $customProperties, $propertyName)
    $myPropertyValue = [System.__ComObject].InvokeMember("value", $binding::GetProperty, $null, $myProperty, $null);
    Write-Host "Original value: $propertyName, $myPropertyValue"

    #Set property value
    [System.__ComObject].InvokeMember("Value", $binding::SetProperty, $null, $myProperty, $propertyValue)

    #Get property value
    $myPropertyValue = [System.__ComObject].InvokeMember("value", $binding::GetProperty, $null, $myProperty, $null);
    Write-Host "New value: $propertyName, $myPropertyValue"
    
    # Close the workbook with save forced
    $document.Saved = $false;
    $document.Close($true)  
}

$wordApplication.Quit()
$excelApplication.Quit()
Start-Sleep -Seconds 1

if ($antidateEnabled) {
    set-date $originalDate | Out-Null
    $current = Get-date
    write-warning "Original system datetime restored: $current"
}