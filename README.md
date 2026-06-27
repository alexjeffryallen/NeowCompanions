# Always Soul Fysh Pip

Slay the Spire 2 mod that makes every run start with the `Byrdpip` event relic, but swaps the combat companion into a tiny harmless Soul Fysh.

Byrdpip still powers the companion behavior, but the card text/art and relic icon are changed to Soul Fysh themed versions.

## Status

This is a first-pass source mod based on the public StS2 mod template. The intent is simple:

- Patch each character model's `StartingRelics` getter.
- Append the base-game Byrdpip relic to the returned starting relic list.
- Patch Byrdpip's combat-start pet summon to create `SoulFyshPipPet`.
- Register a BaseLib config toggle for whether new runs start with Fysh Swoop.

Because Slay the Spire 2 is still early access, class/property names may shift. If the build errors, open `AlwaysSoulFyshPipCode/Patches/StartingRelicsPatch.cs` or `AlwaysSoulFyshPipCode/Models/SoulFyshPipPet.cs` first.

## Files That Matter

- `AlwaysSoulFyshPip.json`: mod manifest. Keep `id` as `AlwaysSoulFyshPip`.
- `AlwaysSoulFyshPip.csproj`: C# project file.
- `AlwaysSoulFyshPipCode/MainFile.cs`: mod initializer.
- `AlwaysSoulFyshPipCode/Patches/StartingRelicsPatch.cs`: adds Byrdpip to starting relics.
- `AlwaysSoulFyshPipCode/Patches/ByrdpipSummonPatch.cs`: swaps the pet summon.
- `AlwaysSoulFyshPipCode/Models/SoulFyshPipPet.cs`: harmless custom Soul Fysh pet.
- `AlwaysSoulFyshPipCode/Patches/ArtPatch.cs`: swaps the Byrd Swoop portrait and Byrdpip relic icon.
- `AlwaysSoulFyshPipCode/Config/AlwaysSoulFyshPipConfig.cs`: BaseLib settings-menu toggle.
- `assets/card_fysh_swoop.png`: replacement card portrait.
- `assets/relic_soul_fysh_pip.png`: replacement relic icon.
- `BUILD_AND_UPLOAD.md`: step-by-step setup, test, and Workshop upload instructions.
