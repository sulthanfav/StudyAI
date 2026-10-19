#!/usr/bin/env python3
"""
Unit tests for StudyAI documentation automation scripts.
Executes using standard python -m unittest.
"""

import sys
import unittest
from pathlib import Path

# Add scripts/docs to sys.path
SCRIPTS_DIR = Path(__file__).resolve().parent.parent.parent / "scripts" / "docs"
sys.path.insert(0, str(SCRIPTS_DIR))

import sync_codebase_index
import validate_docs


class TestDocsSyncTool(unittest.TestCase):
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
        import tempfile
        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_path = Path(tmpdir)
            cat_file = tmp_path / "sample.md"
            cat_file.write_text(sample_catalog, encoding="utf-8")

            # Temporarily redirect CATALOG_DIR
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
        # Check ordering: file_a comes before file_b in master catalog table
        pos_a = gen1.find("`file_a.cs`")
        pos_b = gen1.find("`file_b.cs`")
        self.assertTrue(pos_a < pos_b, "Master file catalog should be sorted alphabetically")


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


if __name__ == "__main__":
    unittest.main()
