$files = Get-ChildItem -Path "d:\Downloads\Quantum Sorce Code\Quantum" -Include *.cs -Recurse

foreach ($f in $files) {
    if ($f.Name -eq "QuantumMenu.IgnoresAccessChecksTo.cs") { continue }
    
    $content = Get-Content $f.FullName -Raw
    
    # Update Player.<Something> to GTPlayer.<Something>
    # We only target common nested types used in GT mods to avoid hitting Photon.Realtime.Player.ActorNumber etc.
    # Common ones: MaterialData, HandState, locomotionEnabledLayers (Wait, that's an instance member usually)
    
    $keywords = "MaterialData", "HandState"
    $newContent = $content
    foreach ($k in $keywords) {
        $newContent = $newContent -replace "\bPlayer\.$k\b", "GorillaLocomotion.GTPlayer.$k"
        $newContent = $newContent -replace "\bGTPlayer\.$k\b", "GorillaLocomotion.GTPlayer.$k"
    }
    
    # Also handle some specific instances that might have been missed by the word boundary regex
    $newContent = $newContent -replace "(?<!GorillaLocomotion\.)\bPlayer\.materialData\b", "GorillaLocomotion.GTPlayer.Instance.materialData"

    if ($newContent -ne $content) {
        $newContent | Set-Content $f.FullName -Encoding UTF8
        Write-Host "Updated nested types in $f"
    }
}
Write-Host "Nested Type Migration complete."
