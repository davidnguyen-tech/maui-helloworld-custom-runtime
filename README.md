# MAUI Hello World with Custom Runtime

A `dotnet new maui` app wired up to use a locally-built .NET runtime from
[dotnet/runtime](https://github.com/dotnet/runtime).

## Prerequisites

- MAUI repo built locally (provides the SDK)
- Runtime repo built locally with shipping packages

### Building the runtime shipping packages

From the runtime repo root:

```bash
# For maccatalyst
./build.sh -s clr+libs+packs+host -c Release /p:TargetOS=maccatalyst /p:TargetArchitecture=arm64

# For iOS
./build.sh -s clr+libs+packs+host -c Release /p:TargetOS=ios /p:TargetArchitecture=arm64
```

Packages land in `artifacts/packages/Release/Shipping/`.

## Setup

Update the paths in these files to match your local setup:

- `NuGet.config` — `local-runtime` source pointing to runtime's shipping packages
- `Directory.Build.props` — `Crossgen2Path` pointing to the local crossgen2 binary

For example, if your repos are at `/Users/user/repos/`:

```
NuGet.config:           /Users/user/repos/runtime/artifacts/packages/Release/Shipping
Directory.Build.props:  /Users/user/repos/runtime/artifacts/bin/coreclr/maccatalyst.arm64.Release/arm64/crossgen2/crossgen2
```

## Building the app

```bash
DOTNET=/Users/user/repos/maui/.dotnet/dotnet

# maccatalyst (Composite R2R with CoreCLR)
$DOTNET build maui-helloworld-custom-runtime.csproj \
  -f net11.0-maccatalyst -r maccatalyst-arm64 \
  -p:UseMonoRuntime=false \
  -p:PublishReadyToRun=true \
  -p:PublishReadyToRunComposite=true

# iOS (Composite R2R with CoreCLR)
$DOTNET build maui-helloworld-custom-runtime.csproj \
  -f net11.0-ios -r ios-arm64 \
  -p:UseMonoRuntime=false \
  -p:PublishReadyToRun=true \
  -p:PublishReadyToRunComposite=true
```

## How it works

- `NuGet.config` points to the runtime's local shipping packages and dev feeds
- `Directory.Build.props` overrides the framework reference to `11.0.0-dev`
  and sets the crossgen2 path to the local build
- `.nuget-cache/` is a local NuGet cache — delete it when rebuilding the runtime

## Running on maccatalyst

```bash
open bin/Debug/net11.0-maccatalyst/maccatalyst-arm64/maui-helloworld-custom-runtime.app
```

## Running on iOS device

```bash
# Install
xcrun devicectl device install app \
  --device <DEVICE-UUID> \
  bin/Debug/net11.0-ios/ios-arm64/maui-helloworld-custom-runtime.app

# Launch with console output
xcrun devicectl device process launch \
  --device <DEVICE-UUID> \
  --console --terminate-existing \
  com.companyname.mauihelloworldcustomruntime
```

## Clearing NuGet cache after runtime rebuild

```bash
rm -rf .nuget-cache
```
