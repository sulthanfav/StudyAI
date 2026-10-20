#!/usr/bin/env python3
"""
Unit tests for StudyAI documentation automation scripts.
Executes using standard python -m unittest.
"""

import os
import sys
import tempfile
import unittest
from pathlib import Path

# Add scripts/docs to sys.path
SCRIPTS_DIR = Path(__file__).resolve().parent.parent.parent / "scripts" / "docs"
sys.path.insert(0, str(SCRIPTS_DIR))

import sync_codebase_index
import validate_docs


class TestDocsSyncTool(unittest.TestCase):
    def test_csproj_in_relevant_extensions(self):
        """Verify .csproj is recognized as a relevant source/project file."""
        self.assertIn(".csproj", sync_codebase_index.RELEVANT_EXTENSIONS, ".csproj must be in RELEVANT_EXTENSIONS")

    def test_parse_catalog_structure(self):
        """Verify catalog parsing logic handles entry headings and metadata."""
        sample_catalog = """# Sample Catalog
### `apps/api/Sample.cs`
- **Module**: `apps/api`
- **Category**: Source
- **Verification Status**: `VERIFIED`

### `apps/api/Unverified.cs`
- **Module**: `apps/api`
- **Category**: Source
- **Verification Status**: `NEEDS_REVIEW`
"""
        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_path = Path(tmpdir)
            cat_file = tmp_path / "sample.md"
            cat_file.write_text(sample_catalog, encoding="utf-8")

            orig_dir = sync_codebase_index.CATALOG_DIR
            try:
                sync_codebase_index.CATALOG_DIR = tmp_path
                entries, by_catalog = sync_codebase_index.parse_catalogs()
                self.assertIn("apps/api/Sample.cs", entries)
                self.assertEqual(entries["apps/api/Sample.cs"]["status"], "VERIFIED")
                self.assertIn("apps/api/Unverified.cs", entries)
                self.assertEqual(entries["apps/api/Unverified.cs"]["status"], "NEEDS_REVIEW")
                self.assertEqual(len(by_catalog["sample.md"]), 2)
            finally:
                sync_codebase_index.CATALOG_DIR = orig_dir

    def test_master_index_deterministic_output(self):
        """Verify that master index generation is deterministic given identical inputs."""
        entries = {
            "file_b.cs": {"module": "b", "category": "Source", "status": "VERIFIED", "catalog_file": "catalog/b.md"},
            "file_a.cs": {"module": "a", "category": "Source", "status": "VERIFIED", "catalog_file": "catalog/a.md"},
        }
        by_catalog = {
            "a.md": ["file_a.cs"],
            "b.md": ["file_b.cs"],
        }
        gen1 = sync_codebase_index.generate_master_index(entries, by_catalog)
        gen2 = sync_codebase_index.generate_master_index(entries, by_catalog)
        self.assertEqual(gen1, gen2)
        pos_a = gen1.find("`file_a.cs`")
        pos_b = gen1.find("`file_b.cs`")
        self.assertTrue(pos_a < pos_b, "Master file catalog should be sorted alphabetically")

    def test_sync_detects_uncataloged_files_and_fails(self):
        """Verify sync detects uncataloged source/.csproj files and reports failure."""
        tracked = ["apps/api/NewProject.csproj", "apps/api/Existing.cs"]
        cataloged = {"apps/api/Existing.cs": {"status": "VERIFIED"}}
        missing = [f for f in tracked if f not in cataloged]
        self.assertEqual(missing, ["apps/api/NewProject.csproj"])

        # Test sync execution helper
        code, msg = sync_codebase_index.run_sync(
            tracked_files=tracked,
            entries=cataloged,
            by_catalog={"api-host.md": ["apps/api/Existing.cs"]},
            check_only=False,
            allow_repair=False,
            dry_run=True
        )
        self.assertNotEqual(code, 0, "Sync must fail when uncataloged files exist")
        self.assertIn("NewProject.csproj", msg)

    def test_sync_detects_stale_catalog_entries_and_fails(self):
        """Verify sync detects catalog entries for non-existent files and fails."""
        tracked = ["apps/api/Existing.cs"]
        cataloged = {
            "apps/api/Existing.cs": {"status": "VERIFIED"},
            "apps/api/Deleted.cs": {"status": "VERIFIED"}
        }
        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_root = Path(tmpdir)
            (tmp_root / "apps" / "api").mkdir(parents=True)
            (tmp_root / "apps" / "api" / "Existing.cs").write_text("", encoding="utf-8")

            orig_root = sync_codebase_index.REPO_ROOT
            try:
                sync_codebase_index.REPO_ROOT = tmp_root
                code, msg = sync_codebase_index.run_sync(
                    tracked_files=tracked,
                    entries=cataloged,
                    by_catalog={"api-host.md": list(cataloged.keys())},
                    check_only=False,
                    allow_repair=False,
                    dry_run=True
                )
                self.assertNotEqual(code, 0, "Sync must fail when stale entries exist")
                self.assertIn("Deleted.cs", msg)
            finally:
                sync_codebase_index.REPO_ROOT = orig_root

    def test_sync_does_not_rewrite_file_index_on_failure_unless_repair(self):
        """Verify FILE_INDEX.md is untouched when validation fails unless repair mode is requested."""
        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_root = Path(tmpdir)
            idx_file = tmp_root / "docs" / "codebase" / "FILE_INDEX.md"
            idx_file.parent.mkdir(parents=True)
            original_content = "# Original Index"
            idx_file.write_text(original_content, encoding="utf-8")

            orig_root = sync_codebase_index.REPO_ROOT
            orig_idx = sync_codebase_index.MASTER_INDEX_PATH
            try:
                sync_codebase_index.REPO_ROOT = tmp_root
                sync_codebase_index.MASTER_INDEX_PATH = idx_file
                # Missing uncataloged file exists
                code, _ = sync_codebase_index.run_sync(
                    tracked_files=["uncataloged.cs"],
                    entries={},
                    by_catalog={},
                    check_only=False,
                    allow_repair=False,
                    dry_run=False
                )
                self.assertNotEqual(code, 0)
                # Content must NOT be rewritten
                self.assertEqual(idx_file.read_text(encoding="utf-8"), original_content)

                # With allow_repair=True, it updates
                code_repair, _ = sync_codebase_index.run_sync(
                    tracked_files=["uncataloged.cs"],
                    entries={},
                    by_catalog={},
                    check_only=False,
                    allow_repair=True,
                    dry_run=False
                )
                self.assertEqual(code_repair, 0)
                self.assertNotEqual(idx_file.read_text(encoding="utf-8"), original_content)
            finally:
                sync_codebase_index.REPO_ROOT = orig_root
                sync_codebase_index.MASTER_INDEX_PATH = orig_idx


class TestDocsValidatorTool(unittest.TestCase):
    def test_reporter_health(self):
        """Verify ValidationReporter transitions between healthy and unhealthy states."""
        reporter = validate_docs.ValidationReporter()
        self.assertTrue(reporter.is_healthy())

        reporter.add_pass("Feature X valid")
        self.assertTrue(reporter.is_healthy())

        reporter.add_warn("Minor warning")
        self.assertTrue(reporter.is_healthy())

        reporter.add_error("Critical missing file")
        self.assertFalse(reporter.is_healthy())

    def test_validator_detects_malformed_skill_frontmatter(self):
        """Verify validator flags missing name, missing description, or unclosed frontmatter."""
        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_root = Path(tmpdir)
            skills_dir = tmp_root / ".agents" / "skills"
            
            # Case 1: unclosed frontmatter
            s1 = skills_dir / "s1"
            s1.mkdir(parents=True)
            (s1 / "SKILL.md").write_text("---\nname: s1\nno closing", encoding="utf-8")

            # Case 2: missing description
            s2 = skills_dir / "s2"
            s2.mkdir(parents=True)
            (s2 / "SKILL.md").write_text("---\nname: s2\n---\nBody", encoding="utf-8")

            # Case 3: valid
            s3 = skills_dir / "s3"
            s3.mkdir(parents=True)
            (s3 / "SKILL.md").write_text("---\nname: s3\ndescription: A valid skill\n---\nBody", encoding="utf-8")

            orig_root = validate_docs.REPO_ROOT
            try:
                validate_docs.REPO_ROOT = tmp_root
                reporter = validate_docs.ValidationReporter()
                validate_docs.check_skill_frontmatter(reporter)
                self.assertFalse(reporter.is_healthy())
                self.assertTrue(any("unclosed" in e.lower() for e in reporter.errors))
                self.assertTrue(any("missing 'description'" in e.lower() for e in reporter.errors))
            finally:
                validate_docs.REPO_ROOT = orig_root

    def test_validator_detects_broken_internal_links(self):
        """Verify validator flags broken relative markdown links."""
        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_root = Path(tmpdir)
            doc_file = tmp_root / "test_doc.md"
            doc_file.write_text("[Good Link](existing.md) and [Bad Link](nonexistent.md)", encoding="utf-8")
            (tmp_root / "existing.md").write_text("Hello", encoding="utf-8")

            reporter = validate_docs.ValidationReporter()
            validate_docs.validate_links_in_file(doc_file, "test_doc.md", reporter)
            self.assertFalse(reporter.is_healthy())
            self.assertTrue(any("nonexistent.md" in e for e in reporter.errors))


if __name__ == "__main__":
    unittest.main()
