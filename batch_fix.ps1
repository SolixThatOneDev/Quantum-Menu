$files = @(
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Experimental.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Fun.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Overpowered.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Projectiles.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Safety.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Settings.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Sound.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Mods\Visuals.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Managers\GunLib.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Managers\FriendManager.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Managers\NotificationManager.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Managers\SoundManager.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Menu\Main.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Menu\Buttons.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Patches\Menu\EffectDataPatch.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Patches\Menu\ReleasePatch.cs",
    "d:\Downloads\Quantum Sorce Code\Quantum\Patches\Safety\URLBlocker.cs"
)

foreach ($f in $files) {
    if (Test-Path $f) {
        $content = Get-Content $f
        if ($content -notmatch "using Player = global::GorillaLocomotion.Player;") {
            $newContent = @()
            $added = $false
            foreach ($line in $content) {
                $newContent += $line
                if ($line -match "using GorillaLocomotion;" -and -not $added) {
                    $newContent += "using Player = global::GorillaLocomotion.Player;"
                    $added = $true
                }
            }
            $newContent | Set-Content $f -Encoding UTF8
            Write-Host "Updated $f"
        } else {
            Write-Host "Skipping $f (already alias exists)"
        }
    } else {
        Write-Host "File not found: $f"
    }
}
