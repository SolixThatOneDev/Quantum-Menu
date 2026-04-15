$files = Get-ChildItem -Path "d:\Downloads\Quantum Sorce Code\Quantum" -Include *.cs -Recurse

foreach ($f in $files) {
    if ($f.Name -eq "QuantumMenu.IgnoresAccessChecksTo.cs") { continue }
    
    $content = Get-Content $f.FullName
    $hasLocomotion = $content -match "using GorillaLocomotion;"
    $hasRealtime = $content -match "using Photon.Realtime;"
    
    # Remove any existing Player alias to avoid duplicates
    $newContent = $content | Where-Object { $_ -notmatch "^using Player = " }
    
    $processedContent = @()
    $added = $false
    foreach ($line in $newContent) {
        $processedContent += $line
        if ($hasLocomotion -and $hasRealtime -and $line -match "using GorillaLocomotion;" -and -not $added) {
            $processedContent += "using Player = global::GorillaLocomotion.Player;"
            $added = $true
        }
    }
    
    # Special Handling for AssetUtilities (Double Ambiguity)
    if ($f.Name -eq "AssetUtilities.cs") {
        $assetContent = $processedContent
        if ($assetContent -notmatch "using Object = UnityEngine.Object;") {
             $finalAsset = @()
             $objAdded = $false
             foreach ($l in $assetContent) {
                 $finalAsset += $l
                 if ($l -match "using UnityEngine;" -and -not $objAdded) {
                     $finalAsset += "using Object = UnityEngine.Object;"
                     $objAdded = $true
                 }
             }
             $processedContent = $finalAsset
        }
    }

    # Deduplicate all usings (Basic dedup)
    $uniqueContent = @()
    $seenUsings = @{}
    foreach ($l in $processedContent) {
        if ($l -match "^using " -and $l -match ";$") {
            if (-not $seenUsings.ContainsKey($l)) {
                $uniqueContent += $l
                $seenUsings.Add($l, $true)
            }
        } else {
            $uniqueContent += $l
        }
    }
    
    $uniqueContent | Set-Content $f.FullName -Encoding UTF8
}
Write-Host "Project-wide cleanup complete."
