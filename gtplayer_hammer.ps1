$files = Get-ChildItem -Path "d:\Downloads\Quantum Sorce Code\Quantum" -Include *.cs -Recurse

foreach ($f in $files) {
    if ($f.Name -eq "QuantumMenu.IgnoresAccessChecksTo.cs") { continue }
    
    $content = Get-Content $f.FullName -Raw
    
    # 1. Update fully-qualified names from previous repair attempts
    $newContent = $content -replace "GorillaLocomotion\.Player", "GorillaLocomotion.GTPlayer"
    
    # 2. Match any remaining naked Player.Instance calls that were intended for movement
    # Using negative lookahead to avoid already qualified names
    $newContent = $newContent -replace "(?<!GorillaLocomotion\.)\bPlayer\.Instance\b", "GorillaLocomotion.GTPlayer.Instance"
    $newContent = $newContent -replace "(?<!GorillaLocomotion\.)\bPlayer\.StaticInstance\b", "GorillaLocomotion.GTPlayer.StaticInstance"
    
    # 3. Handle explicit type casts or variable declarations if any
    # (Simplified for now, as .Instance is the most common pattern)
    
    if ($newContent -ne $content) {
        $newContent | Set-Content $f.FullName -Encoding UTF8
        Write-Host "Migrated to GTPlayer: $f"
    }
}
Write-Host "GTPlayer Migration complete."
