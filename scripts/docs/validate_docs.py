#!/usr/bin/env python3
"""
StudyAI Documentation Health & Integrity Validator

Validates:
1. Presence of required core documents (AGENTS.md, SYSTEM_MAP.md, etc.)
2. Catalog path integrity (every cataloged file exists on disk)
3. Skill frontmatter (valid YAML name/description in .agents/skills/*/SKILL.md)
4. Internal markdown link resolution across core documentation
5. Reports coverage and exits with non-zero code on any failure (CI-friendly).

Cross-platform: Windows, Linux, macOS. Requires Python 3.8+.
"""

import os
import re
import sys
from pathlib import Path
from typing import List, Tuple

REPO_ROOT = Path(__file__).resolve().parent.parent.parent

REQUIRED_DOCUMENTS = [
    "AGENTS.md",
    "SYSTEM_MAP.md",
    "docs/START_HERE.md",
    "docs/INDEX.md",
    "docs/development/CURRENT_STATE.md",
    "docs/codebase/MODULE_INDEX.md",
    "docs/codebase/FEATURE_MAP.md",
    "docs/codebase/FILE_INDEX.md",
    "docs/plans/TASK-TEMPLATE/SPEC.md",
    "docs/plans/TASK-TEMPLATE/PLAN.md",
    "docs/plans/TASK-TEMPLATE/task.md",
    "docs/plans/TASK-TEMPLATE/HANDOFF.md",
    ".agents/rules/mandatory-map-check.md",
]

CORE_DOCS_FOR_LINKS = [
    "AGENTS.md",
    "SYSTEM_MAP.md",
    "docs/START_HERE.md",
    "docs/INDEX.md",
    "docs/development/CURRENT_STATE.md",
    "docs/codebase/MODULE_INDEX.md",
    "docs/codebase/FEATURE_MAP.md",
]


class ValidationReporter:
    def __init__(self):
        self.errors: List[str] = []
        self.warnings: List[str] = []
        self.passes: List[str] = []

    def add_pass(self, msg: str):
        self.passes.append(msg)

    def add_warn(self, msg: str):
        self.warnings.append(msg)

    def add_error(self, msg: str):
        self.errors.append(msg)

    def is_healthy(self) -> bool:
        return len(self.errors) == 0


def check_required_documents(reporter: ValidationReporter):
    """Verify presence of core architecture and navigation files."""
    missing = []
    for doc in REQUIRED_DOCUMENTS:
        p = REPO_ROOT / doc
        if not p.is_file():
            missing.append(doc)

    if missing:
        for m in missing:
            reporter.add_error(f"Missing required document: {m}")
    else:
        reporter.add_pass(f"All {len(REQUIRED_DOCUMENTS)} required core documents exist.")


def check_catalog_integrity(reporter: ValidationReporter):
    """Verify that every cataloged file path exists on disk."""
    catalog_dir = REPO_ROOT / "docs" / "codebase" / "catalog"
    if not catalog_dir.is_dir():
        reporter.add_error(f"Catalog directory not found: {catalog_dir}")
        return

    entry_pattern = re.compile(r"^###\s+`([^`]+)`")
    stale_paths = []
    cataloged_count = 0

    for cat_file in catalog_dir.glob("*.md"):
        content = cat_file.read_text(encoding="utf-8")
        for line in content.splitlines():
            m = entry_pattern.match(line.strip())
            if m:
                cataloged_count += 1
                rel_path = m.group(1).strip()
                target_path = REPO_ROOT / rel_path
                if not target_path.exists():
                    stale_paths.append((cat_file.name, rel_path))

    if stale_paths:
        for cat_name, stale in stale_paths:
            reporter.add_error(f"Stale catalog entry in {cat_name}: '{stale}' does not exist on disk.")
    else:
        reporter.add_pass(f"All {cataloged_count} cataloged paths exist on disk.")


def check_skill_frontmatter(reporter: ValidationReporter):
    """Verify skill SKILL.md frontmatter format and mandatory fields."""
    skills_dir = REPO_ROOT / ".agents" / "skills"
    if not skills_dir.is_dir():
        reporter.add_warn(f"Skills directory not found: {skills_dir}")
        return

    skill_files = list(skills_dir.glob("*/SKILL.md"))
    if not skill_files:
        reporter.add_warn("No skill SKILL.md files found.")
        return

    valid_count = 0
    for sf in skill_files:
        rel_sf = sf.relative_to(REPO_ROOT).as_posix()
        content = sf.read_text(encoding="utf-8")
        if not content.startswith("---"):
            reporter.add_error(f"Skill missing YAML frontmatter opening '---': {rel_sf}")
            continue

        parts = content.split("---", 2)
        if len(parts) < 3:
            reporter.add_error(f"Skill has unclosed YAML frontmatter: {rel_sf}")
            continue

        frontmatter = parts[1]
        has_name = bool(re.search(r"^name:\s*.+$", frontmatter, re.MULTILINE))
        has_desc = bool(re.search(r"^description:\s*.+$", frontmatter, re.MULTILINE))

        if not has_name:
            reporter.add_error(f"Skill frontmatter missing 'name': {rel_sf}")
        elif not has_desc:
            reporter.add_error(f"Skill frontmatter missing 'description': {rel_sf}")
        else:
            valid_count += 1

    if valid_count == len(skill_files) and reporter.is_healthy():
        reporter.add_pass(f"All {valid_count} skills have valid YAML frontmatter.")


def validate_links_in_file(file_path: Path, doc_rel: str, reporter: ValidationReporter) -> int:
    """Verifies that all relative markdown links in a file resolve to existing targets."""
    link_pattern = re.compile(r"\[([^\]]+)\]\(([^)]+)\)")
    checked = 0
    if not file_path.is_file():
        return 0

    content = file_path.read_text(encoding="utf-8")
    doc_parent = file_path.parent

    for m in link_pattern.finditer(content):
        url = m.group(2).strip()

        # Skip external or non-file URLs
        if url.startswith(("http://", "https://", "mailto:", "ftp:")):
            continue

        # Strip anchor
        file_part = url.split("#")[0].strip()
        if not file_part:
            continue

        checked += 1
        target = (doc_parent / file_part).resolve()
        if not target.exists():
            reporter.add_error(f"Broken link in '{doc_rel}': '{url}' -> target '{file_part}' does not exist.")

    return checked


def check_markdown_links(reporter: ValidationReporter):
    """Verify relative markdown links resolve across core documentation."""
    total_checked = 0
    initial_errors = len(reporter.errors)

    for doc_rel in CORE_DOCS_FOR_LINKS:
        doc_path = REPO_ROOT / doc_rel
        if doc_path.is_file():
            total_checked += validate_links_in_file(doc_path, doc_rel, reporter)

    if len(reporter.errors) == initial_errors:
        reporter.add_pass(f"All {total_checked} checked relative markdown links resolve successfully.")


def main():
    print("=== StudyAI Documentation Health & Integrity Validator ===")
    reporter = ValidationReporter()

    check_required_documents(reporter)
    check_catalog_integrity(reporter)
    check_skill_frontmatter(reporter)
    check_markdown_links(reporter)

    print("\nPassed Checks:")
    for p in reporter.passes:
        print(f"  [OK] {p}")

    if reporter.warnings:
        print("\nWarnings:")
        for w in reporter.warnings:
            print(f"  [WARN] {w}")

    if reporter.errors:
        print("\nFailures:")
        for e in reporter.errors:
            print(f"  [FAIL] {e}")
        print(f"\n[RESULT] Documentation validation FAILED with {len(reporter.errors)} error(s).")
        sys.exit(1)
    else:
        print(f"\n[RESULT] Documentation validation PASSED cleanly (0 errors).")
        sys.exit(0)


if __name__ == "__main__":
    main()
