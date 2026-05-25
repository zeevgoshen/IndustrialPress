import fs from "fs";
import path from "path";
import { execSync } from "child_process";
import { fileURLToPath } from "url";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(scriptDir, "../..");
const srcArch = path.join(root, "docs", "architecture.md");
const destArch = path.join(root, "IndustrialPress", "docs", "architecture.md");
const zipPath = path.join(root, "IndustrialPress-Phase0.zip");
const donePath = path.join(root, "IndustrialPress", "ZIP-DONE.txt");

if (!fs.existsSync(srcArch)) {
  console.error("Missing:", srcArch);
  process.exit(1);
}

fs.mkdirSync(path.dirname(destArch), { recursive: true });
fs.copyFileSync(srcArch, destArch);

const archText = fs.readFileSync(destArch, "utf8");
const archLines = archText.split(/\n/).length;
if (archLines < 600) {
  console.error("architecture.md too short:", archLines, "lines");
  process.exit(1);
}

if (fs.existsSync(zipPath)) fs.unlinkSync(zipPath);

const folder = path.join(root, "IndustrialPress");
if (process.platform === "win32") {
  const cmd = `Compress-Archive -LiteralPath '${folder.replace(/'/g, "''")}' -DestinationPath '${zipPath.replace(/'/g, "''")}' -Force`;
  execSync(`powershell -NoProfile -Command "${cmd}"`, { stdio: "inherit" });
} else {
  execSync(`zip -r "${zipPath}" IndustrialPress`, { cwd: root, stdio: "inherit" });
}

const zipBytes = fs.statSync(zipPath).size;
const desktop = path.join(process.env.USERPROFILE || "", "Desktop", "IndustrialPress-Phase0.zip");
let desktopCopied = false;
try {
  if (process.env.USERPROFILE && fs.existsSync(path.dirname(desktop))) {
    fs.copyFileSync(zipPath, desktop);
    desktopCopied = true;
  }
} catch {
  /* ignore */
}

const report = {
  zip_absolute_path: path.resolve(zipPath),
  zip_size_bytes: zipBytes,
  architecture_md_size_bytes: fs.statSync(destArch).size,
  architecture_md_line_count: archLines,
  desktop_copy: desktopCopied ? path.resolve(desktop) : null,
  timestamp: new Date().toISOString(),
};

fs.writeFileSync(donePath, JSON.stringify(report, null, 2), "utf8");
console.log(JSON.stringify(report, null, 2));
