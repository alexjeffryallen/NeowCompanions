param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$chrome = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
if (-not (Test-Path -LiteralPath $chrome)) {
    throw "Chrome is required for the two animated WebP captures: $chrome"
}

$sourceByAsset = @{
    aeonglass = 'aeonglass'
    architect = 'architect'
    bonebinder = 'necrobinder'
    bygone_effigy = 'bygone_effigy'
    byrdonis = 'byrdonis'
    ceremonial_beast = 'ceremonial_beast'
    decimillipede = 'decimillipede'
    ember_pip = 'byrdpip'
    entomancer = 'entomancer'
    eye_with_teeth = 'eye_with_teeth'
    frost_pip = 'byrdpip'
    fysh_swoop = 'soul_fysh'
    gilded_page = 'regent'
    glitchling = 'defect'
    gremlin_merc = 'gremlin_merc'
    infested_prism = 'infested_prism'
    kaiser_crab = 'kaiser_crab'
    kin_follower = 'kin_follower'
    knowledge_demon = 'knowledge_demon'
    knight_gang = 'spectral_knight'
    lagavulin_matriarch = 'lagavulin_matriarch'
    operosis = 'osty'
    mecha_knight = 'mecha_knight'
    phantasmal_gardener = 'phantasmal_gardener'
    phrog_parasite = 'phrog_parasite'
    queen = 'queen'
    rustclad = 'ironclad'
    seapunk = 'seapunk'
    shadeleaf = 'silent'
    shrinker_beetle = 'shrinker_beetle'
    skulking_colony = 'skulking_colony'
    soul_fysh_pip = 'soul_fysh'
    storm_pip = 'byrdpip'
    soul_nexus = 'soul_nexus'
    test_subject = 'test_subject'
    terror_eel = 'terror_eel'
    the_insatiable = 'the_insatiable'
    the_kin = 'kin_priest'
    thieving_hopper = 'thieving_hopper'
    thorn_pip = 'byrdpip'
    vantom = 'vantom'
    waterfall_giant = 'waterfall_giant'
    wriggler = 'wriggler'
}

# These companions intentionally use original generated artwork and must not be
# replaced by the authentic-game-art refresh workflow.
$customGeneratedAssets = @()

$characterSources = @('defect', 'ironclad', 'necrobinder', 'regent', 'silent')
$webpOnlySources = @('aeonglass', 'decimillipede', 'soul_nexus', 'thieving_hopper')
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) 'neow-authentic-game-art'
$sourceRoot = Join-Path $tempRoot 'sources'
New-Item -ItemType Directory -Force -Path $sourceRoot | Out-Null

function Get-AuthenticSource([string]$sourceName) {
    $pngPath = Join-Path $sourceRoot "$sourceName.png"
    if (Test-Path -LiteralPath $pngPath) {
        return $pngPath
    }

    if ($webpOnlySources -contains $sourceName) {
        $webpPath = Join-Path $sourceRoot "$sourceName.webp"
        Invoke-WebRequest "https://spire-codex.com/static/images/monsters/$sourceName.webp" -OutFile $webpPath
        $htmlPath = Join-Path $tempRoot "$sourceName.html"
        $profilePath = Join-Path $tempRoot "chrome-$sourceName"
        $uri = ([Uri]$webpPath).AbsoluteUri
        [IO.File]::WriteAllText($htmlPath, "<style>*{margin:0}html,body{width:512px;height:512px;background:transparent;overflow:hidden}img{width:512px;height:512px;object-fit:contain}</style><img src='$uri'>")
        & $chrome --headless --disable-gpu --hide-scrollbars --default-background-color=00000000 --window-size=512,512 "--user-data-dir=$profilePath" "--screenshot=$pngPath" $htmlPath | Out-Null
        if (-not (Test-Path -LiteralPath $pngPath)) {
            throw "Failed to capture authentic WebP for $sourceName"
        }
        return $pngPath
    }

    $kind = if ($characterSources -contains $sourceName) { 'characters' } else { 'monsters' }
    $url = "https://spire-codex.com/static/images/renders/$kind/$sourceName/$sourceName.png"
    Invoke-WebRequest $url -OutFile $pngPath
    return $pngPath
}

function Write-FittedPng([string]$sourcePath, [string]$destinationPath, [int]$width, [int]$height, [double]$fill, [Drawing.Color]$backgroundColor) {
    $source = [Drawing.Bitmap]::FromFile($sourcePath)
    try {
        $output = New-Object Drawing.Bitmap($width, $height, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $graphics = [Drawing.Graphics]::FromImage($output)
            try {
                $graphics.Clear($backgroundColor)
                $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceOver
                $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
                $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $scale = [Math]::Min(($width * $fill) / $source.Width, ($height * $fill) / $source.Height)
                $drawWidth = [int][Math]::Round($source.Width * $scale)
                $drawHeight = [int][Math]::Round($source.Height * $scale)
                $x = [int](($width - $drawWidth) / 2)
                $y = [int](($height - $drawHeight) / 2)
                $graphics.DrawImage($source, $x, $y, $drawWidth, $drawHeight)
            }
            finally {
                $graphics.Dispose()
            }
            $output.Save($destinationPath, [Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $output.Dispose()
        }
    }
    finally {
        $source.Dispose()
    }
}

$assetDirectories = @(
    (Join-Path $RepositoryRoot 'assets'),
    (Join-Path (Split-Path -Parent $RepositoryRoot) 'NeowCompanions\assets')
) | Where-Object { Test-Path -LiteralPath $_ }

$changed = 0
foreach ($assetDirectory in $assetDirectories) {
    foreach ($asset in Get-ChildItem -LiteralPath $assetDirectory -Filter '*.png') {
        if ($asset.BaseName -notmatch '^(card|relic)_(.+)$') {
            continue
        }
        $type = $Matches[1]
        $key = $Matches[2]
        if ($customGeneratedAssets -contains $key) {
            continue
        }
        if (-not $sourceByAsset.ContainsKey($key)) {
            throw "No authentic source mapping for $($asset.FullName)"
        }
        $sourcePath = Get-AuthenticSource $sourceByAsset[$key]
        if ($type -eq 'card') {
            Write-FittedPng $sourcePath $asset.FullName 1536 1024 0.96 ([Drawing.Color]::Black)
        }
        else {
            Write-FittedPng $sourcePath $asset.FullName 256 256 0.92 ([Drawing.Color]::Transparent)
        }
        $changed++
    }
}

Write-Output "Replaced $changed card/relic PNGs with authentic game renders."
& (Join-Path $PSScriptRoot 'tint_byrdpip_relics.ps1') -RepositoryRoot $RepositoryRoot
