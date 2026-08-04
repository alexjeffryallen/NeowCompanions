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
    assassin_ruby_raider = 'assassin_ruby_raider'
    architect = 'architect'
    bonebinder = 'necrobinder'
    bygone_effigy = 'bygone_effigy'
    axe_ruby_raider = 'axe_ruby_raider'
    brute_ruby_raider = 'brute_ruby_raider'
    byrdonis = 'byrdonis'
    ceremonial_beast = 'ceremonial_beast'
    calcified_cultist = 'calcified_cultist'
    corpse_slug = 'corpse_slug'
    chomper = 'chomper'
    crossbow_ruby_raider = 'crossbow_ruby_raider'
    cubex_construct = 'cubex_construct'
    damp_cultist = 'damp_cultist'
    fossil_stalker = 'fossil_stalker'
    decimillipede = 'decimillipede'
    ember_pip = 'byrdpip'
    entomancer = 'entomancer'
    eye_with_teeth = 'eye_with_teeth'
    flyconid = 'flyconid'
    fogmog = 'fogmog'
    frost_pip = 'byrdpip'
    fysh_swoop = 'soul_fysh'
    gilded_page = 'regent'
    glitchling = 'defect'
    gremlin_merc = 'gremlin_merc'
    fuzzy_wurm_crawler = 'fuzzy_wurm_crawler'
    haunted_ship = 'haunted_ship'
    hunter_killer = 'hunter_killer'
    infested_prism = 'infested_prism'
    inklet = 'inklet'
    living_fog = 'living_fog'
    louse_progenitor = 'louse_progenitor'
    kaiser_crab = 'kaiser_crab'
    kin_follower = 'kin_follower'
    knowledge_demon = 'knowledge_demon'
    knight_gang = 'spectral_knight'
    lagavulin_matriarch = 'lagavulin_matriarch'
    leaf_slime_m = 'leaf_slime_m'
    leaf_slime_s = 'leaf_slime_s'
    mawler = 'mawler'
    operosis = 'osty'
    mecha_knight = 'mecha_knight'
    phantasmal_gardener = 'phantasmal_gardener'
    phrog_parasite = 'phrog_parasite'
    parafright = 'parafright'
    punch_construct = 'punch_construct'
    queen = 'queen'
    rustclad = 'ironclad'
    seapunk = 'seapunk'
    shadeleaf = 'silent'
    shrinker_beetle = 'shrinker_beetle'
    slithering_strangler = 'slithering_strangler'
    snapping_jaxfruit = 'snapping_jaxfruit'
    skulking_colony = 'skulking_colony'
    sludge_spinner = 'sludge_spinner'
    slumbering_beetle = 'slumbering_beetle'
    spiny_toad = 'spiny_toad'
    stabbot = 'stabbot'
    soul_fysh_pip = 'soul_fysh'
    storm_pip = 'byrdpip'
    soul_nexus = 'soul_nexus'
    test_subject = 'test_subject'
    terror_eel = 'terror_eel'
    torch_head_amalgam = 'torch_head_amalgam'
    tunneler = 'tunneler'
    two_tailed_rat = 'two_tailed_rat'
    the_insatiable = 'the_insatiable'
    the_kin = 'kin_priest'
    thieving_hopper = 'thieving_hopper'
    thorn_pip = 'byrdpip'
    twig_slime_m = 'twig_slime_m'
    twig_slime_s = 'twig_slime_s'
    vantom = 'vantom'
    waterfall_giant = 'waterfall_giant'
    vine_shambler = 'vine_shambler'
    bowlbug_egg = 'bowlbug_egg'
    bowlbug_nectar = 'bowlbug_nectar'
    bowlbug_rock = 'bowlbug_rock'
    bowlbug_silk = 'bowlbug_silk'
    sewer_clam = 'sewer_clam'
    wriggler = 'wriggler'
    axebot = 'axebot'
    battle_friend_v1 = 'battle_friend_v1'
    battle_friend_v2 = 'battle_friend_v2'
    battle_friend_v3 = 'battle_friend_v3'
    crusher = 'crusher'
    devoted_sculptor = 'devoted_sculptor'
    exoskeleton = 'exoskeleton'
    fabricator = 'fabricator'
    flail_knight = 'flail_knight'
    frog_knight = 'frog_knight'
    gas_bomb = 'gas_bomb'
    globe_head = 'globe_head'
    guardbot = 'guardbot'
    living_shield = 'living_shield'
    magi_knight = 'magi_knight'
    mysterious_knight = 'mysterious_knight'
    myte = 'myte'
    nibbit = 'nibbit'
    noisebot = 'noisebot'
    ovicopter = 'ovicopter'
    owl_magistrate = 'owl_magistrate'
    paels_legion = 'paels_legion'
    rocket = 'rocket'
    scroll_of_biting = 'scroll_of_biting'
    slimed_berserker = 'slimed_berserker'
    the_forgotten = 'the_forgotten'
    the_lost = 'the_lost'
    the_obscura = 'the_obscura'
    toadpole = 'toadpole'
    tough_egg = 'tough_egg'
    tracker_ruby_raider = 'tracker_ruby_raider'
    turret_operator = 'turret_operator'
    zapbot = 'zapbot'
}

# These companions intentionally use original generated artwork and must not be
# replaced by the authentic-game-art refresh workflow.
$customGeneratedAssets = @()

$characterSources = @('defect', 'ironclad', 'necrobinder', 'regent', 'silent')
$webpOnlySources = @(
    'aeonglass', 'decimillipede', 'soul_nexus', 'thieving_hopper',
    'assassin_ruby_raider', 'axe_ruby_raider', 'brute_ruby_raider', 'crossbow_ruby_raider',
    'flyconid', 'fogmog', 'mawler', 'fuzzy_wurm_crawler', 'inklet', 'snapping_jaxfruit',
    'slithering_strangler', 'leaf_slime_s', 'leaf_slime_m', 'twig_slime_s', 'twig_slime_m',
    'vine_shambler', 'chomper', 'cubex_construct', 'damp_cultist', 'calcified_cultist',
    'corpse_slug', 'two_tailed_rat', 'sewer_clam', 'haunted_ship', 'sludge_spinner',
    'punch_construct', 'fossil_stalker', 'living_fog', 'parafright', 'tunneler',
    'spiny_toad', 'stabbot', 'hunter_killer', 'torch_head_amalgam', 'bowlbug_egg',
    'bowlbug_nectar', 'bowlbug_rock', 'bowlbug_silk', 'louse_progenitor', 'slumbering_beetle',
    'axebot', 'battle_friend_v1', 'battle_friend_v2', 'battle_friend_v3', 'crusher',
    'devoted_sculptor', 'exoskeleton', 'fabricator', 'flail_knight', 'frog_knight',
    'gas_bomb', 'globe_head', 'guardbot', 'living_shield', 'magi_knight',
    'mysterious_knight', 'myte', 'nibbit', 'noisebot', 'ovicopter', 'owl_magistrate',
    'paels_legion', 'rocket', 'scroll_of_biting', 'slimed_berserker', 'the_forgotten',
    'the_lost', 'the_obscura', 'toadpole', 'tough_egg', 'tracker_ruby_raider',
    'turret_operator', 'zapbot'
)
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
        Invoke-WebRequest "https://cdn.spire-codex.com/monsters/$sourceName.webp" -OutFile $webpPath
        $htmlPath = Join-Path $tempRoot "$sourceName.html"
        $profilePath = Join-Path $tempRoot "chrome-$sourceName"
        $uri = ([Uri]$webpPath).AbsoluteUri
        [IO.File]::WriteAllText($htmlPath, "<style>*{margin:0}html,body{width:512px;height:512px;background:transparent;overflow:hidden}img{width:512px;height:512px;object-fit:contain}</style><img src='$uri'>")
        & $chrome --headless --disable-gpu --hide-scrollbars --default-background-color=00000000 --window-size=512,512 "--user-data-dir=$profilePath" "--screenshot=$pngPath" $htmlPath | Out-Null
        for ($attempt = 0; $attempt -lt 40 -and -not (Test-Path -LiteralPath $pngPath); $attempt++) {
            Start-Sleep -Milliseconds 250
        }
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
