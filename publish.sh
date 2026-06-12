#!/bin/bash

# FlowDesk v1.0.0 Publish Script

echo "Building FlowDesk Release for macOS (osx-x64)..."
dotnet publish apps/FlowDesk.Desktop/FlowDesk.Desktop.csproj -c Release -r osx-x64 --self-contained

echo "Building FlowDesk Release for Windows (win-x64)..."
dotnet publish apps/FlowDesk.Desktop/FlowDesk.Desktop.csproj -c Release -r win-x64 --self-contained

echo "Building FlowDesk Release for Linux (linux-x64)..."
dotnet publish apps/FlowDesk.Desktop/FlowDesk.Desktop.csproj -c Release -r linux-x64 --self-contained

echo "Publish complete! Check apps/FlowDesk.Desktop/bin/Release/net10.0/<platform>/publish"
