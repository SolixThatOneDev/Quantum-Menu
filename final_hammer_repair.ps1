$files = Get-ChildItem -Path "d:\Downloads\Quantum Sorce Code\Quantum" -Include *.cs -Recurse

foreach ($f in $files) {
    if ($f.Name -eq "QuantumMenu.IgnoresAccessChecksTo.cs") { continue }
    
    $content = Get-Content $f.FullName -Raw
    
    # 1. Remove ANY Player/GorillaPlayer alias attempts
    $newContent = $content -replace "(?m)^using Player\s*=\s*.*?;`r?`n", ""
    $newContent = $newContent -replace "(?m)^using GorillaPlayer\s*=\s*.*?;`r?`n", ""
    
    # 2. Replace 'Player.Instance' or 'GorillaPlayer.Instance' with fully qualified version
    # ONLY if it's not already qualified
    $newContent = $newContent -replace "(?<!GorillaLocomotion\.)\bPlayer\.Instance\b", "GorillaLocomotion.Player.Instance"
    $newContent = $newContent -replace "(?<!GorillaLocomotion\.)\bPlayer\.StaticInstance\b", "GorillaLocomotion.Player.StaticInstance"
    $newContent = $newContent -replace "\bGorillaPlayer\.Instance\b", "GorillaLocomotion.Player.Instance"
    $newContent = $newContent -replace "\bGorillaPlayer\.StaticInstance\b", "GorillaLocomotion.Player.StaticInstance"
    
    # 3. Handle explicit 'Player' type usage (e.g. in casts or local variables)
    # We must be careful not to hit Photon.Realtime.Player in method signatures
    # In this project, movement Player is almost always used via .Instance
    
    if ($newContent -ne $content) {
        $newContent | Set-Content $f.FullName -Encoding UTF8
        Write-Host "Repaired $f"
    }
}
Write-Host "Project-wide Repair complete."
