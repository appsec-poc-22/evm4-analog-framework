#!/usr/bin/env bash
# STAGE 3 setup: pack DemoFramework as a .nupkg on a local feed, so DemoService
# consumes it as a binary package rather than source. This is the configuration
# that matches EVM4 (Crossbones.* from Nexus).
set -euo pipefail
cd "$(dirname "$0")"
mkdir -p local-feed
dotnet pack DemoFramework/DemoFramework.csproj -c Release -o local-feed
cat > nuget.config <<'XML'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local-feed" value="./local-feed" />
  </packageSources>
</configuration>
XML
echo
echo "Packed. Now edit DemoService/DemoService.csproj:"
echo "  - comment out the ProjectReference"
echo "  - uncomment the PackageReference"
echo "  - then: dotnet restore && git commit"
