#!/bin/bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
PACKAGE_DIR="$ROOT_DIR/src/ShiziMac"
VERSION="${1:-0.2.0}"
OUTPUT_DIR="${2:-$ROOT_DIR/artifacts/release}"

mkdir -p "$OUTPUT_DIR"
OUTPUT_DIR="$(cd "$OUTPUT_DIR" && pwd)"
case "$OUTPUT_DIR" in
  "$ROOT_DIR"/artifacts/*) ;;
  *) echo "Output directory must be inside $ROOT_DIR/artifacts" >&2; exit 1 ;;
esac

swift test --package-path "$PACKAGE_DIR"
swift build --package-path "$PACKAGE_DIR" -c release --arch arm64 --arch x86_64
BIN_DIR="$(swift build --package-path "$PACKAGE_DIR" -c release --arch arm64 --arch x86_64 --show-bin-path)"

APP_PATH="$OUTPUT_DIR/Shizi.app"
ICONSET_PATH="$OUTPUT_DIR/Shizi.iconset"
DMG_STAGE="$OUTPUT_DIR/dmg-stage"
ZIP_PATH="$OUTPUT_DIR/Shizi-v$VERSION-macOS-universal.zip"
DMG_PATH="$OUTPUT_DIR/Shizi-v$VERSION-macOS-universal.dmg"

rm -rf "$APP_PATH" "$ICONSET_PATH" "$DMG_STAGE"
rm -f "$ZIP_PATH" "$DMG_PATH"
mkdir -p "$APP_PATH/Contents/MacOS" "$APP_PATH/Contents/Resources" "$ICONSET_PATH" "$DMG_STAGE"

cp "$BIN_DIR/ShiziMac" "$APP_PATH/Contents/MacOS/Shizi"
cp "$PACKAGE_DIR/Info.plist" "$APP_PATH/Contents/Info.plist"
/usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString $VERSION" "$APP_PATH/Contents/Info.plist"

SOURCE_ICON="$ROOT_DIR/src/Shizi/Assets/Shizi.png"
sips -z 16 16 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_16x16.png" >/dev/null
sips -z 32 32 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_16x16@2x.png" >/dev/null
sips -z 32 32 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_32x32.png" >/dev/null
sips -z 64 64 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_32x32@2x.png" >/dev/null
sips -z 128 128 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_128x128.png" >/dev/null
sips -z 256 256 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_128x128@2x.png" >/dev/null
sips -z 256 256 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_256x256.png" >/dev/null
sips -z 512 512 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_256x256@2x.png" >/dev/null
sips -z 512 512 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_512x512.png" >/dev/null
sips -z 1024 1024 "$SOURCE_ICON" --out "$ICONSET_PATH/icon_512x512@2x.png" >/dev/null
iconutil -c icns "$ICONSET_PATH" -o "$APP_PATH/Contents/Resources/Shizi.icns"

codesign --force --deep --sign - "$APP_PATH"
ditto -c -k --sequesterRsrc --keepParent "$APP_PATH" "$ZIP_PATH"

cp -R "$APP_PATH" "$DMG_STAGE/"
ln -s /Applications "$DMG_STAGE/Applications"
hdiutil create -volname "拾字 Shizi" -srcfolder "$DMG_STAGE" -ov -format UDZO "$DMG_PATH"

rm -rf "$ICONSET_PATH" "$DMG_STAGE"
echo "$ZIP_PATH"
echo "$DMG_PATH"
