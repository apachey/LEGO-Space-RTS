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
    def test_registry_acceptance_owns_package_even_if_proposal_input_is_unaccepted(self):
        accepted = 'unit.rock_raiders.crew'
        registry = {'assets': [{'stableId': accepted, 'directorAccepted': True}]}
        records = [{'stableId': accepted}, {'stableId': 'unit.rock_raiders.loader_dozer'}]

        self.assertEqual(GENERATOR.accepted_unit_ids(registry), {accepted})
        generated = {
            GENERATOR.BATCH / 'crew.md': 'proposal output',
            GENERATOR.BATCH / 'loader_dozer.md': 'unaccepted proposal output',
            GENERATOR.BATCH / 'review.html': '<article id="crew">proposal</article>',
        }
        owned = GENERATOR.proposal_outputs_for_check(generated, records, {accepted})

        self.assertNotIn(GENERATOR.BATCH / 'crew.md', owned)
        self.assertIn(GENERATOR.BATCH / 'loader_dozer.md', owned)
        self.assertIn(GENERATOR.BATCH / 'review.html', owned)

    def test_accepted_review_card_is_preserved_and_unaccepted_card_remains_generated(self):
        records = [
            {'stableId': 'unit.rock_raiders.crew'},
            {'stableId': 'unit.rock_raiders.loader_dozer'},
        ]
        generated = '<article id="crew">unaccepted template</article><article id="loader_dozer">new proposal</article>'
        current = '<article id="crew">director accepted card</article><article id="loader_dozer">old proposal</article>'

        result = GENERATOR.preserve_accepted_review_cards(
            generated, current, records, {'unit.rock_raiders.crew'}
        )

        self.assertEqual(GENERATOR.article(result, 'crew'), '<article id="crew">director accepted card</article>')
        self.assertEqual(GENERATOR.article(result, 'loader_dozer'), '<article id="loader_dozer">new proposal</article>')

    def test_unaccepted_proposal_remains_reproducibility_owned(self):
        record = {'stableId': 'unit.rock_raiders.loader_dozer'}
        outputs = {GENERATOR.BATCH / 'loader_dozer.md': 'proposal'}

        self.assertEqual(
            GENERATOR.proposal_outputs_for_check(outputs, [record], set()),
            outputs,
        )

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
