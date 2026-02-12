#!/bin/bash
set -e

# Build native library as a static library for iOS arm64
# The .a gets linked into the MAUI app bundle

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
OUT_DIR="$SCRIPT_DIR/bin"
mkdir -p "$OUT_DIR"

SDK=$(xcrun --sdk iphoneos --show-sdk-path)

echo "Building for ios-arm64..."
xcrun clang -arch arm64 -isysroot "$SDK" -miphoneos-version-min=15.0 \
    -c "$SCRIPT_DIR/nativelib.c" -o "$OUT_DIR/nativelib-ios.o"
ar rcs "$OUT_DIR/libnativelib-ios.a" "$OUT_DIR/nativelib-ios.o"
echo "  -> $OUT_DIR/libnativelib-ios.a"

echo "Building for maccatalyst-arm64..."
xcrun clang -arch arm64 -target arm64-apple-ios17.0-macabi \
    -c "$SCRIPT_DIR/nativelib.c" -o "$OUT_DIR/nativelib-maccatalyst.o"
ar rcs "$OUT_DIR/libnativelib-maccatalyst.a" "$OUT_DIR/nativelib-maccatalyst.o"
echo "  -> $OUT_DIR/libnativelib-maccatalyst.a"

echo "Done."
