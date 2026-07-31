param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$assetDirectory = Join-Path $RepositoryRoot 'assets'
$sourcePath = Join-Path $assetDirectory 'relic_ember_pip.png'
$tints = [ordered]@{
    'relic_ember_pip.png' = [Drawing.Color]::FromArgb(255, 245, 108, 42)
    'relic_frost_pip.png' = [Drawing.Color]::FromArgb(255, 72, 205, 255)
    'relic_storm_pip.png' = [Drawing.Color]::FromArgb(255, 166, 108, 255)
    'relic_thorn_pip.png' = [Drawing.Color]::FromArgb(255, 92, 220, 78)
}

$loadedSource = [Drawing.Bitmap]::FromFile($sourcePath)
try {
    $source = New-Object Drawing.Bitmap($loadedSource)
}
finally {
    $loadedSource.Dispose()
}
try {
    foreach ($entry in $tints.GetEnumerator()) {
        $output = New-Object Drawing.Bitmap($source.Width, $source.Height, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            for ($y = 0; $y -lt $source.Height; $y++) {
                for ($x = 0; $x -lt $source.Width; $x++) {
                    $pixel = $source.GetPixel($x, $y)
                    if ($pixel.A -eq 0) {
                        continue
                    }

                    $luminance = (0.2126 * $pixel.R + 0.7152 * $pixel.G + 0.0722 * $pixel.B) / 255.0
                    $highlight = [Math]::Min(1.0, 0.18 + (0.96 * $luminance))
                    $red = [int][Math]::Round($entry.Value.R * $highlight)
                    $green = [int][Math]::Round($entry.Value.G * $highlight)
                    $blue = [int][Math]::Round($entry.Value.B * $highlight)
                    $output.SetPixel($x, $y, [Drawing.Color]::FromArgb($pixel.A, $red, $green, $blue))
                }
            }

            $destination = Join-Path $assetDirectory $entry.Key
            $output.Save($destination, [Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $output.Dispose()
        }
    }
}
finally {
    $source.Dispose()
}

Write-Output 'Tinted the four authentic Byrdpip relic icons.'
