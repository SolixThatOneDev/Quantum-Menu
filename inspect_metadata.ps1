$managedPath = "d:\Downloads\Quantum Sorce Code\Quantum\obj\Debug\netstandard2.1\publicized\Assembly-CSharp.dll"
$managedDir = Split-Path $managedPath

Add-Type -AssemblyName "System.Reflection"

function Get-TypeInfo {
    try {
        # Load dependencies first
        Get-ChildItem $managedDir -Filter "*.dll" | ForEach-Object {
            try { [System.Reflection.Assembly]::LoadFrom($_.FullName) | Out-Null } catch {}
        }
        
        $asm = [System.Reflection.Assembly]::LoadFrom($managedPath)
        $types = $asm.GetTypes()
        
        Write-Host "Assembly: $($asm.FullName)"
        Write-Host "Total Types: $($types.Count)"
        
        $targetTypes = $types | Where-Object { $_.Name -like "*Player*" }
        
        foreach ($t in $targetTypes) {
            Write-Host "Type: $($t.FullName) (Namespace: $($t.Namespace))"
        }
        
        $targetNamespaces = $types | Select-Object -ExpandProperty Namespace -Unique | Where-Object { $_ -like "*Gorilla*" }
        Write-Host "`nGorilla-related Namespaces:"
        $targetNamespaces | ForEach-Object { Write-Host " - $_" }

    } catch {
        Write-Host "Error: $($_.Exception.Message)"
        if ($_.Exception.LoaderExceptions) {
            Write-Host "Loader Exceptions:"
            $_.Exception.LoaderExceptions | ForEach-Object { Write-Host " - $($_.Message)" }
        }
    }
}

Get-TypeInfo
