$files = Get-ChildItem -Path "d:\Downloads\Quantum Sorce Code\Quantum" -Include *.cs -Recurse

foreach ($f in $files) {
    if ($f.Name -eq "QuantumMenu.IgnoresAccessChecksTo.cs") { continue }
    
    $content = Get-Content $f.FullName -Raw
    
    # Check if this file has both conflicting namespaces
    if ($content -match "using GorillaLocomotion;" -and $content -match "using Photon.Realtime;") {
        # 1. Remove ANY existing Player alias attempt
        $newContent = $content -replace "(?m)^using Player\s*=\s*.*?;`r?`n", ""
        
        # 2. Add the unique GorillaPlayer alias
        if ($newContent -notmatch "using GorillaPlayer = global::GorillaLocomotion.Player;") {
            $newContent = $newContent -replace "using GorillaLocomotion;", "using GorillaLocomotion;`r`nusing GorillaPlayer = global::GorillaLocomotion.Player;"
        }
        
        # 3. Replace 'Player.Instance' with 'GorillaPlayer.Instance' in the code
        # We must be careful not to replace 'Photon.Realtime.Player'
        # Usually it's used as 'Player.Instance' or 'Player.StaticInstance' in this project
        $newContent = $newContent -replace "\bPlayer\.Instance\b", "GorillaPlayer.Instance"
        $newContent = $newContent -replace "\bPlayer\.StaticInstance\b", "GorillaPlayer.StaticInstance"
        
        if ($newContent -ne $content) {
            $newContent | Set-Content $f.FullName -Encoding UTF8
            Write-Host "Migrated $f"
        }
    }
}
Write-Host "Migration to GorillaPlayer complete."
