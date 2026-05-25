from pathlib import Path

root = Path(__file__).resolve().parents[2]
src = root / "docs" / "architecture.md"
dst = root / "IndustrialPress" / "docs" / "architecture.md"
dst.parent.mkdir(parents=True, exist_ok=True)
if not src.is_file():
    raise SystemExit(f"Missing source: {src}")
dst.write_text(src.read_text(encoding="utf-8"), encoding="utf-8")
print(f"Copied {src} -> {dst} ({dst.stat().st_size} bytes)")
