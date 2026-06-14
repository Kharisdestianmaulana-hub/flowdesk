#!/bin/bash
set -e

APP_NAME="FlowDesk"
PUBLISH_DIR="apps/FlowDesk.Desktop/bin/Release/net10.0/osx-x64/publish"
DMG_NAME="${APP_NAME}-v1.0.0-macOS.dmg"

echo "1. Memastikan rilis osx-x64 sudah di-build..."
dotnet publish apps/FlowDesk.Desktop/FlowDesk.Desktop.csproj -c Release -r osx-x64 --self-contained

echo "2. Membuat struktur aplikasi macOS (.app)..."
APP_DIR="${APP_NAME}.app"
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

echo "3. Menyalin binary ke dalam bundle..."
cp -R "$PUBLISH_DIR/"* "$APP_DIR/Contents/MacOS/"
cp apps/FlowDesk.Desktop/Assets/FlowDesk.icns "$APP_DIR/Contents/Resources/"

# Pastikan file utamanya dapat dieksekusi (executable)
chmod +x "$APP_DIR/Contents/MacOS/FlowDesk.Desktop"

echo "4. Membuat Info.plist..."
cat > "$APP_DIR/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>FlowDesk</string>
    <key>CFBundleDisplayName</key>
    <string>FlowDesk</string>
    <key>CFBundleIdentifier</key>
    <string>com.flowdesk.app</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundleExecutable</key>
    <string>FlowDesk.Desktop</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>CFBundleIconFile</key>
    <string>FlowDesk.icns</string>
</dict>
</plist>
EOF

echo "5. Membungkus menjadi .dmg dengan create-dmg..."
# Jika DMG lama ada, hapus dulu
if [ -f "$DMG_NAME" ]; then
    rm "$DMG_NAME"
fi

# Fix PATH issue where LWP's head overrides system head
export PATH="/usr/bin:$PATH"

create-dmg \
  --volname "$APP_NAME Installer" \
  --window-pos 200 120 \
  --window-size 600 400 \
  --icon-size 100 \
  --icon "$APP_NAME.app" 150 190 \
  --hide-extension "$APP_NAME.app" \
  --app-drop-link 450 190 \
  "$DMG_NAME" \
  "$APP_DIR"

echo "✅ Berhasil! File $DMG_NAME telah siap di folder saat ini."
