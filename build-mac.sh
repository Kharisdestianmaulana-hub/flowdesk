#!/bin/bash
set -e
VERSION="1.5.0"
APP_NAME="FlowDesk"
PUBLISH_DIR="release/mac-tmp"
APP_DIR="release/${APP_NAME}.app"
DMG_PATH="release/${APP_NAME}-v${VERSION}-macOS.dmg"

echo "Publishing..."
dotnet publish apps/FlowDesk.Desktop -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o $PUBLISH_DIR

echo "Creating App Bundle..."
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

cp $PUBLISH_DIR/FlowDesk.Desktop "$APP_DIR/Contents/MacOS/"
cp $PUBLISH_DIR/*.dylib "$APP_DIR/Contents/MacOS/" 2>/dev/null || true
cp apps/FlowDesk.Desktop/Assets/FlowDesk.icns "$APP_DIR/Contents/Resources/"

cat << PLIST > "$APP_DIR/Contents/Info.plist"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>${APP_NAME}</string>
    <key>CFBundleExecutable</key>
    <string>FlowDesk.Desktop</string>
    <key>CFBundleIdentifier</key>
    <string>com.flowdesk.app</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundleIconFile</key>
    <string>FlowDesk.icns</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
PLIST

echo "Creating DMG..."
rm -f "$DMG_PATH"
hdiutil create -volname "${APP_NAME}" -srcfolder "$APP_DIR" -ov -format UDZO "$DMG_PATH"

echo "Cleaning up..."
rm -rf "$APP_DIR"
rm -rf "$PUBLISH_DIR"
rm -rf release/FlowDesk-v1.5.0-macOS-arm64
