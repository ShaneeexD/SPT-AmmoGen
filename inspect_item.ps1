$asm = [System.Reflection.Assembly]::LoadFrom('C:\SPT\SPT\SPTarkov.Server.Core.dll')
$type = $asm.GetTypes() | Where-Object { $_.Name -eq 'Item' -and $_.FullName -like '*Eft*' } | Select-Object -First 1
if ($type -eq $null) { $type = $asm.GetTypes() | Where-Object { $_.Name -eq 'Item' } | Select-Object -First 1 }
$type | Format-List FullName, BaseType
$type.GetProperties() | Select-Object Name, PropertyType | Format-Table -AutoSize
