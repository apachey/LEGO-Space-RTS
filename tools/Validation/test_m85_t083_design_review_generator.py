#!/usr/bin/env python3
"""Regression checks for per-unit T083 document generation."""
import importlib.util
import unittest
from pathlib import Path

SCRIPT = Path(__file__).resolve().parents[1] / 'generate-m85-t083-design-review.py'
SPEC = importlib.util.spec_from_file_location('m85_t083_design_review_generator', SCRIPT)
GENERATOR = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(GENERATOR)


class PerUnitGenerationTests(unittest.TestCase):
    def test_unit_scope_names_only_selected_package_outputs(self):
        markdown, review, registry = GENERATOR.unit_output_paths('sample_unit')

        self.assertEqual(markdown.name, 'sample_unit.md')
        self.assertEqual(review.name, 'review.html')
        self.assertEqual(registry.name, 'registry.json')
        self.assertNotIn('hover_scout.md', {p.name for p in (markdown, review, registry)})

    def test_hover_scout_uses_separate_target_review_artifact(self):
        markdown, review, registry = GENERATOR.unit_output_paths('hover_scout')

        self.assertEqual(markdown.name, 'hover_scout.md')
        self.assertEqual(review.name, 'hover_scout_target_review.html')
        self.assertEqual(registry.name, 'registry.json')

    def test_replacing_review_card_preserves_other_units_byte_for_byte(self):
        before = '<main><article id="sample_one">old</article><article id="sample_two">keep me</article></main>'
        after = GENERATOR.replace_article(before, 'sample_one', '<article id="sample_one">new</article>')

        self.assertEqual(GENERATOR.article(after, 'sample_one'), '<article id="sample_one">new</article>')
        self.assertIn('<article id="sample_two">keep me</article>', after)

    def test_registry_update_preserves_other_unit_record(self):
        other = {'stableId': 'unit.test.preserved', 'status': 'KEEP'}
        document = {'assets': [
            {'stableId': 'unit.test.selected', 'status': 'old'},
            other.copy(),
        ]}
        expected = {'stableId': 'unit.test.selected', 'status': 'UPDATED'}

        GENERATOR.update_registry_entry(document, expected)

        self.assertEqual(document['assets'][0], expected)
        self.assertEqual(document['assets'][1], other)


if __name__ == '__main__':
    unittest.main()
