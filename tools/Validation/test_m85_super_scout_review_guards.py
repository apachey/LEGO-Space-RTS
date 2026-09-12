#!/usr/bin/env python3
"""Exercise T082 recording/approval guards without mutating repository fixtures."""

from contextlib import redirect_stderr, redirect_stdout
import io
import json
from pathlib import Path
import unittest
from unittest.mock import patch

import validate_m85_super_scout as validator


class ReviewGuardTests(unittest.TestCase):
    def assert_rejected(self, fixture_path, mutate, expected_reason):
        original_read = Path.read_text
        fixture = json.loads(original_read(fixture_path, encoding="utf-8"))
        mutate(fixture)

        def read_fixture(path, *args, **kwargs):
            if path == fixture_path:
                return json.dumps(fixture, ensure_ascii=False)
            return original_read(path, *args, **kwargs)

        errors = io.StringIO()
        with patch.object(Path, "read_text", read_fixture), redirect_stdout(io.StringIO()), redirect_stderr(errors):
            with self.assertRaises(SystemExit) as raised:
                validator.main()
        self.assertEqual(raised.exception.code, 1)
        self.assertIn(expected_reason, errors.getvalue())

    def test_stale_sixty_response_record_is_rejected(self):
        def mutate(fixture):
            fixture["attempts"][2]["reviewed"] = 60
            fixture["attempts"][2]["missingCodes"] = [f"V{i}" for i in range(51, 57)]
        self.assert_rejected(validator.BLIND_REVIEW_RESULTS, mutate, "complete 66-code")

    def test_missing_review_code_is_rejected(self):
        self.assert_rejected(validator.BLIND_REVIEW_RESULTS,
                             lambda fixture: fixture["attempts"][2]["reviewedCodes"].pop(),
                             "complete 66-code")

    def test_missing_recovered_response_is_rejected(self):
        self.assert_rejected(validator.BLIND_REVIEW_RESULTS,
                             lambda fixture: fixture["attempts"][2]["recoveredResponses"].pop("V51"),
                             "six recovered original director responses")

    def test_missing_accepted_correction_is_rejected(self):
        self.assert_rejected(validator.BLIND_REVIEW_RESULTS,
                             lambda fixture: fixture["attempts"][2]["acceptedIndividualCorrections"].pop("V55"),
                             "accepted correction records disagree")

    def test_unreviewed_mothership_cannot_be_silently_accepted(self):
        def mutate(fixture):
            next(record for record in fixture["revisionCandidates"]
                 if record["stableId"] == "unit.aliens.alien_mothership")["status"] = "DIRECTOR_ACCEPTED_CORRECTION_CANDIDATE"
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate, "active revision candidate set or state")

    def test_donor_cannot_be_promoted_to_final_building(self):
        def mutate(fixture):
            fixture["sourceStudies"][0]["status"] = "DIRECTOR_ACCEPTED_FINAL_BUILDING"
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate, "donor study was silently promoted")

    def test_donor_pdf_must_match_its_audited_source(self):
        def mutate(fixture):
            fixture["sourceStudies"][1]["sourcePdf"] = "https://www.lego.com/cdn/product-assets/product.bi.core.pdf/incorrect.pdf"
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate, "unverified official PDF lineage")

    def test_composed_proposal_cannot_be_silently_accepted(self):
        def mutate(fixture):
            fixture["proposals"][0]["directorDecision"] = "APPROVED"
        self.assert_rejected(validator.COMPOSED_DESIGN_PROPOSALS, mutate, "composed proposal was silently accepted")


if __name__ == "__main__":
    unittest.main()
