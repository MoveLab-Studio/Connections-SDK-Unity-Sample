# Connections SDK Unity Sample

Minimal Unity sample app demonstrating the Connections SDK for rowing machine integration.

## What it shows

- Module initialization with `ConnectionsModule.Create`
- Device scanning via `connections.Devices(...)`, filtered to rowing machines
- Connecting to / forgetting a rowing machine via `connections.Connect` / `connections.Forget`
- Live metrics via the typed `connections.Rowing` streams: distance and drive (power, stroke rate)
- Fake device support for running without hardware (debug builds)

Every callback is delivered on the Unity main thread, so the UI is driven straight from the subscriptions, and each subscription is an `IDisposable` torn down in `OnDestroy`.

## Requirements

- Unity **2022.3.62f3** (see `ProjectSettings/ProjectVersion.txt`)
- Android 8.0+ (API 26) device — the SDK floor; this sample is Android-only
- MoveLab Nexus credentials (contact MoveLab to obtain)

## Getting Started

1. Clone this repo.
2. **UPM registry token** — the package is served from MoveLab's private npm registry on Nexus. Authenticate by adding your token to `~/.upmconfig.toml` (UPM does not read `.npmrc`):
   ```toml
   [npmAuth."https://nexus.mls-cdn.net/repository/movelab-npm/"]
   token = "<base64 of user:password>"
   alwaysAuth = true
   ```
3. **Maven credentials** — the package's native `.aar` and its transitive `mls.*` artifacts resolve from the Nexus Maven repo at Android build time. Add them to `~/.gradle/gradle.properties` (or set `NEXUS_USER` / `NEXUS_PASSWORD`):
   ```
   nexusUser=<your username>
   nexusPassword=<your password>
   ```
4. Open the project in Unity and switch the platform to **Android** (`File ▸ Build Settings ▸ Android`).
5. Open `Assets/Scenes/BasicConnection.unity` and press **Play** in the Editor, or build and run on an Android device (the host prompts for BLE permissions at runtime).

In the Editor and in debug builds a simulated ("Fake") rowing machine appears automatically in the device list — no hardware required. Tap **Connect** and the distance + drive labels update as it rows.

## Authentication

The SDK is private — there are two credential surfaces, both pointing at the same MoveLab Nexus host:

- **UPM (package)** — the `mls.connections.unity` package is an npm package on `movelab-npm`. UPM authenticates via the token in `~/.upmconfig.toml`.
- **Gradle (native artifacts)** — the bundled `.aar` and the transitive `mls.*` dependencies are Maven artifacts on `movelab`. The Android Gradle build authenticates via `nexusUser` / `nexusPassword` (or the `NEXUS_USER` / `NEXUS_PASSWORD` environment variables).

Neither `~/.upmconfig.toml` nor `~/.gradle/gradle.properties` lives in the repo, so credentials are never committed. Contact MoveLab to obtain credentials.

## How the Android dependencies are wired

The native side is pulled in by the [External Dependency Manager for Unity (EDM4U)](https://github.com/googlesamples/unity-jar-resolver), which resolves the SDK `.aar` and its transitive `mls.*` closure through Gradle at build time. The pieces:

- `Packages/manifest.json` — the MoveLab scoped registry (`mls.connections` → `movelab-npm`) and the pinned `mls.connections.unity` version, plus EDM4U from OpenUPM.
- `Assets/Plugins/Android/settingsTemplate.gradle` — the authenticated Nexus Maven repository.
- `Assets/Plugins/Android/gradleTemplate.properties` — `-Xss16m` (D8 dexes the SDK's Kotlin with deep recursion and overflows the default thread stack without it).

## Key Files

| File | Purpose |
|---|---|
| `Assets/Scripts/ConnectionsSample.cs` | Core SDK integration: discovery, connect, rowing streams |
| `Assets/Scenes/BasicConnection.unity` | A camera + the sample component |
| `Assets/Plugins/Android/` | Gradle templates: Nexus repo + `-Xss16m` |
| `Packages/manifest.json` | SDK package + registry declaration |
