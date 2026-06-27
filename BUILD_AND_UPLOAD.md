# Build, Test, and Upload

## 1. Install the Required Tools

1. Install the .NET SDK 9.0 or newer.
2. Install Rider or Visual Studio. Rider is easiest for this template style.
3. Download MegaDot, Mega Crit's Godot build, or the matching Godot .NET version.
4. In Steam, subscribe to BaseLib for Slay the Spire 2.

BaseLib's Workshop folder is usually:

```text
C:\Program Files (x86)\Steam\steamapps\workshop\content\2868840\3737335127\BaseLib
```

The game folder is usually:

```text
C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2
```

## 2. Put the Project Somewhere Normal

Copy this whole `AlwaysSoulFyshPip` folder to somewhere like:

```text
C:\Users\count\Desktop\AlwaysSoulFyshPip
```

Avoid OneDrive for this if possible. It can cause permission weirdness with build files.

## 3. Fix Paths if Needed

Open `Directory.Build.props`.

Change this line if your MegaDot exe is not here:

```xml
<GodotPath>C:/megadot/MegaDot_v4.5.1-stable_mono_win64.exe</GodotPath>
```

If Steam or Slay the Spire 2 is not found automatically, open `Sts2PathDiscovery.props` and set:

```xml
<Sts2Path>C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2</Sts2Path>
```

## 4. Build

Open the folder in Rider or Visual Studio.

From a terminal in the project folder:

```powershell
dotnet restore
dotnet build
```

If you get an SDK error for `Godot.NET.Sdk/4.5.1`, run:

```powershell
dotnet nuget add source https://api.nuget.org/v3/index.json
dotnet restore
```

## 5. Test Locally

After a successful build, copy these into a new folder:

```text
C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\AlwaysSoulFyshPip\
```

Required files:

```text
AlwaysSoulFyshPip.dll
AlwaysSoulFyshPip.json
assets\card_fysh_swoop.png
assets\relic_soul_fysh_pip.png
```

This mod loads PNG assets directly from its local `assets` folder. The manifest says `has_pck: false`, so do not include a `.pck`.

Launch Slay the Spire 2 with mods enabled, then check:

1. Settings -> Mod Settings shows `Always Soul Fysh Pip`.
2. Start a new run.
3. You begin with Byrdpip.
4. Byrd Swoop appears from the relic effect.
5. A tiny Soul Fysh-style pet appears in combat.

## 6. If the Code Does Not Compile

The most likely early-access API breakpoints are in:

```text
AlwaysSoulFyshPipCode\Patches\StartingRelicsPatch.cs
```

Check these names against your decompiled `sts2.dll`:

- `CharacterModel`
- `StartingRelics`
- `RelicModel`
- `ModelDb.Relic<Byrdpip>()`
- `PlayerCmd.AddPet<T>()`

The relic id is reported by STS2 Companion as `BYRDPIP`, and Byrdpip is the event relic that grants Byrd Swoop plus the combat companion.

## 7. Workshop Upload

The exact upload button can move while StS2 modding is still changing, but the usual Steam Workshop flow is:

1. Build/publish the mod.
2. Create a clean upload folder containing only:
   - `AlwaysSoulFyshPip.dll`
   - `AlwaysSoulFyshPip.json`
   - a preview image, usually `preview.png`
3. Open Slay the Spire 2's mod upload/workshop tool if it is included with the game/mod launcher.
4. Choose `AlwaysSoulFyshPip.json` or the containing folder.
5. Set title: `Always Soul Fysh Pip`.
6. Set visibility to `Private` first.
7. Upload.
8. Subscribe to your own private item, test it, then change visibility to `Public`.

Suggested Workshop description:

```text
Start every run with Byrdpip, but with a tiny Soul Fysh companion.

This gives you the Byrd Swoop card immediately instead of needing the Byrdonis Nest event. The pet summon is cosmetic-swapped into a harmless small Soul Fysh.

Requires BaseLib.
```
