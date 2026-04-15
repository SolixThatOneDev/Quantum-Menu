$originalPath = "d:\Downloads\Quantum Sorce Code\Quantum\References\Managed\Assembly-CSharp.dll"
$publicizedPath = "d:\Downloads\Quantum Sorce Code\Quantum\obj\Debug\netstandard2.1\publicized\Assembly-CSharp.dll"

Add-Type -AssemblyName "System.Reflection"

function Check-Visibility {
    param($path, $label)
    try {
        Write-Host "--- Checking $label ($path) ---"
        # Load assembly without dependencies first to see if we can get just the basic type info
        $asmData = [System.IO.File]::ReadAllBytes($path)
        $asm = [System.Reflection.Assembly]::Load($asmData)
        
        $target = $asm.GetTypes() | Where-Object { $_.FullName -eq "GorillaLocomotion.Player" }
        
        if ($target) {
            Write-Host "Found GorillaLocomotion.Player"
            Write-Host "IsPublic: $($target.IsPublic)"
            Write-Host "Attributes: $($target.Attributes)"
        } else {
            Write-Host "GorillaLocomotion.Player NOT FOUND in Metadata!"
        }
    } catch {
        Write-Host "Error in $label: $($_.Exception.Message)"
    }
}

Check-Visibility $originalPath "Original"
Check-Visibility $publicizedPath "Publicized"
