#!/usr/bin/env python3
"""
StudyAI Codebase Catalog Synchronization Tool

Discovers source files, parses module catalogs in docs/codebase/catalog/,
detects uncataloged or stale entries, and deterministically regenerates
docs/codebase/FILE_INDEX.md.

Cross-platform: Windows, Linux, macOS. Requires Python 3.8+.
"""

import os
import re
import subprocess
import sys
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
CATALOG_DIR = REPO_ROOT / "docs" / "codebase" / "catalog"
MASTER_INDEX_PATH = REPO_ROOT / "docs" / "codebase" / "FILE_INDEX.md"

IGNORED_DIR_NAMES = {
    ".git", ".vs", ".vscode", ".idea", "bin", "obj", "out", "dist",
    "node_modules", ".next", ".turbo", "__pycache__", ".venv", "venv",
    ".pytest_cache", "TestResults", "artifacts"
}

IGNORED_EXTENSIONS = {
    ".dll", ".exe", ".pdb", ".cache", ".txt", ".user", ".suo",
    ".log", ".tmp", ".swp"
}

RELEVANT_EXTENSIONS = {
    ".cs", ".csproj", ".json", ".props", ".slnx", ".sql", ".ps1", ".py", ".sh",
    ".yml", ".yaml", ".http", ".editorconfig", ".env.example", ".gitignore"
}


def get_tracked_files() -> List[str]:
    """Get list of relevant files using git or filesystem fallback."""
    try:
        res = subprocess.run(
            ["git", "ls-files", "--cached", "--others", "--exclude-standard"],
            cwd=str(REPO_ROOT),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            check=True
        )
        files = res.stdout.splitlines()
    except Exception:
        files = []
        for root, dirs, filenames in os.walk(REPO_ROOT):
            dirs[:] = [d for d in dirs if d not in IGNORED_DIR_NAMES]
            rel_root = Path(root).relative_to(REPO_ROOT)
            for f in filenames:
                rel_path = (rel_root / f).as_posix()
                files.append(rel_path)

    filtered = []
    for f in files:
        path = Path(f)
        posix_str = path.as_posix()
        if any(part in IGNORED_DIR_NAMES for part in path.parts):
            continue
        if path.suffix.lower() in IGNORED_EXTENSIONS:
            continue
        if posix_str.startswith("docs/") or posix_str.startswith("openspec/") or posix_str.startswith(".agents/"):
            continue
        if posix_str.endswith(".md"):
            continue
        if path.suffix.lower() in RELEVANT_EXTENSIONS or path.name in {".editorconfig", ".gitignore", ".env.example", "global.json"}:
            filtered.append(posix_str)

    return sorted(filtered)


def parse_catalogs() -> Tuple[Dict[str, Dict[str, str]], Dict[str, List[str]]]:
    """
    Parses all markdown files in docs/codebase/catalog/*.md.
    Returns:
      entries: dict mapping relative file path -> {module, category, status, catalog_file}
      by_catalog: dict mapping catalog filename -> list of file paths
    """
    entries = {}
    by_catalog = {}

    if not CATALOG_DIR.is_dir():
        return entries, by_catalog

    entry_pattern = re.compile(r"^###\s+`([^`]+)`")
    prop_pattern = re.compile(r"^-\s+\*\*([^*]+)\*\*:\s*(.+)$")

    for catalog_file in sorted(CATALOG_DIR.glob("*.md")):
        catalog_name = catalog_file.name
        by_catalog[catalog_name] = []
        content = catalog_file.read_text(encoding="utf-8")

        current_file = None
        current_meta = {}

        for line in content.splitlines():
            line_s = line.strip()
            m_entry = entry_pattern.match(line_s)
            if m_entry:
                if current_file:
                    entries[current_file] = current_meta
                    by_catalog[catalog_name].append(current_file)
                current_file = m_entry.group(1).strip()
                current_meta = {
                    "catalog_file": f"catalog/{catalog_name}",
                    "module": "unknown",
                    "category": "Source",
                    "status": "NEEDS_REVIEW"
                }
                continue

            if current_file:
                m_prop = prop_pattern.match(line_s)
                if m_prop:
                    key = m_prop.group(1).strip().lower()
                    val = m_prop.group(2).strip()
                    if "module" in key:
                        current_meta["module"] = val.replace("`", "")
                    elif "category" in key:
                        current_meta["category"] = val.replace("`", "")
                    elif "verification status" in key or "status" in key:
                        if "VERIFIED" in val:
                            current_meta["status"] = "VERIFIED"
                        elif "NEEDS_REVIEW" in val:
                            current_meta["status"] = "NEEDS_REVIEW"

        if current_file:
            entries[current_file] = current_meta
            by_catalog[catalog_name].append(current_file)

    return entries, by_catalog


def generate_master_index(entries: Dict[str, Dict[str, str]], by_catalog: Dict[str, List[str]]) -> str:
    """Generates deterministic markdown content for docs/codebase/FILE_INDEX.md."""
    lines = [
        "# StudyAI Master Codebase File Index",
        "",
        "Last synchronized: 2026-10-19  ",
        "Generator: `scripts/docs/sync_codebase_index.py`  ",
        "Navigation: [`SYSTEM_MAP.md`](../../SYSTEM_MAP.md) | [`MODULE_INDEX.md`](MODULE_INDEX.md) | [`FEATURE_MAP.md`](FEATURE_MAP.md)",
        "",
        "---",
        "",
        "## 1. Summary of Documentation Coverage",
        "",
        "| Module | Catalog File | Cataloged Files | Verified | Needs Review | Coverage |",
        "|---|---|---|---|---|---|"
    ]

    total_files = len(entries)
    total_verified = sum(1 for e in entries.values() if e.get("status") == "VERIFIED")
    total_review = total_files - total_verified

    catalog_display_names = {
        "api-host.md": "API Host",
        "contracts.md": "Contracts",
        "domain.md": "Domain",
        "application.md": "Application",
        "infrastructure.md": "Infrastructure",
        "tests.md": "Test Suites",
        "infra.md": "Infra & Tooling"
    }

    for cat_name, file_list in sorted(by_catalog.items()):
        display = catalog_display_names.get(cat_name, cat_name)
        count = len(file_list)
        verified = sum(1 for f in file_list if entries.get(f, {}).get("status") == "VERIFIED")
        review = count - verified
        cov_pct = f"{(verified / count * 100):.0f}%" if count > 0 else "0%"
        lines.append(f"| **{display}** | [`catalog/{cat_name}`](catalog/{cat_name}) | {count} | {verified} | {review} | {cov_pct} |")

    total_cov = f"{(total_verified / total_files * 100):.0f}%" if total_files > 0 else "0%"
    lines.append(f"| **Total Baseline** | — | **{total_files}** | **{total_verified}** | **{total_review}** | **{total_cov}** |")
    lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("## 2. Master File Catalog")
    lines.append("")
    lines.append("| File Path | Module | Category | Status | Catalog Reference |")
    lines.append("|---|---|---|---|---|")

    for f_path in sorted(entries.keys()):
        meta = entries[f_path]
        mod = meta.get("module", "root")
        cat = meta.get("category", "Source")
        stat = meta.get("status", "NEEDS_REVIEW")
        cat_file = meta.get("catalog_file", "catalog/infra.md")
        lines.append(f"| `{f_path}` | `{mod}` | {cat} | `{stat}` | [`{cat_file}`]({cat_file}) |")

    lines.append("")
    return "\n".join(lines)


def run_sync(
    tracked_files: Optional[List[str]] = None,
    entries: Optional[Dict[str, Dict[str, str]]] = None,
    by_catalog: Optional[Dict[str, List[str]]] = None,
    check_only: bool = False,
    allow_repair: bool = False,
    dry_run: bool = False
) -> Tuple[int, str]:
    """
    Executes synchronization and validation of catalogs.
    Returns (exit_code, message).
    """
    if tracked_files is None:
        tracked_files = get_tracked_files()

    if entries is None or by_catalog is None:
        parsed_entries, parsed_by_catalog = parse_catalogs()
        if entries is None:
            entries = parsed_entries
        if by_catalog is None:
            by_catalog = parsed_by_catalog

    cataloged_paths = set(entries.keys())
    missing_from_catalog = sorted([f for f in tracked_files if f not in cataloged_paths])
    stale_paths = sorted([f for f in cataloged_paths if not (REPO_ROOT / f).exists()])

    has_errors = bool(missing_from_catalog or stale_paths)
    generated_content = generate_master_index(entries, by_catalog)

    output_lines = [
        "=== StudyAI Codebase Index Synchronization ===",
        f"Discovered relevant source files: {len(tracked_files)}",
        f"Found cataloged entries: {len(cataloged_paths)}"
    ]

    if missing_from_catalog:
        output_lines.append(f"\n[FAIL] Found {len(missing_from_catalog)} source file(s) not in any catalog:")
        for m in missing_from_catalog[:10]:
            output_lines.append(f"  + {m}")
        if len(missing_from_catalog) > 10:
            output_lines.append(f"  ... and {len(missing_from_catalog) - 10} more.")

    if stale_paths:
        output_lines.append(f"\n[FAIL] Found {len(stale_paths)} catalog entry(ies) referencing non-existent files:")
        for s in stale_paths:
            output_lines.append(f"  - {s}")

    if check_only:
        if has_errors:
            output_lines.append("\n[FAIL] Codebase file index validation failed (uncataloged or stale files exist).")
            return 1, "\n".join(output_lines)

        if not MASTER_INDEX_PATH.exists():
            output_lines.append(f"\n[FAIL] Master index does not exist at {MASTER_INDEX_PATH}")
            return 1, "\n".join(output_lines)

        existing = MASTER_INDEX_PATH.read_text(encoding="utf-8")
        if existing != generated_content:
            output_lines.append("\n[FAIL] Master index FILE_INDEX.md is out of date.")
            return 1, "\n".join(output_lines)

        output_lines.append("\n[SUCCESS] Codebase file index is perfectly synchronized.")
        return 0, "\n".join(output_lines)

    # Sync mode (not check_only)
    if has_errors and not allow_repair:
        output_lines.append("\n[FAIL] Synchronization halted: uncataloged files or stale entries detected.")
        output_lines.append("Add missing files to catalog or use --repair to force rewriting FILE_INDEX.md.")
        return 1, "\n".join(output_lines)

    if not dry_run:
        MASTER_INDEX_PATH.parent.mkdir(parents=True, exist_ok=True)
        MASTER_INDEX_PATH.write_text(generated_content, encoding="utf-8")

    output_lines.append(f"\n[SUCCESS] Master index updated at: {MASTER_INDEX_PATH}")
    return 0, "\n".join(output_lines)


def main():
    check_only = "--check" in sys.argv
    allow_repair = "--repair" in sys.argv

    code, msg = run_sync(check_only=check_only, allow_repair=allow_repair)
    print(msg)
    sys.exit(code)


if __name__ == "__main__":
    main()
