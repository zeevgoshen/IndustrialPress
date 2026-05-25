IndustrialPress Phase 0 — download / zip
=======================================

YOUR REPO ROOT (example):
  C:\Users\zeev\IndustrialPress

BUILD ZIP (run in PowerShell from that folder):
  powershell -ExecutionPolicy Bypass -File .\scripts\Create-Phase0Zip.ps1

OUTPUT:
  C:\Users\zeev\IndustrialPress\IndustrialPress-Phase0.zip
  (also copied to Desktop if available)

OPEN IN VISUAL STUDIO 2022:
  C:\Users\zeev\IndustrialPress\IndustrialPress.sln

WHY CURSOR AGENT SHELL FAILED
  The agent workspace was not the same folder as C:\Users\zeev\IndustrialPress.
  Use YOUR terminal in this directory for copy/zip/build commands.

VERIFY:
  Test-Path .\docs\architecture.md
  Test-Path .\IndustrialPress.sln
  dotnet build .\IndustrialPress.sln
