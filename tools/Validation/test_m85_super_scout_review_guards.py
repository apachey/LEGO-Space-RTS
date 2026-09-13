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
    def test_solar_correction_generation_cannot_accept_image(self):
        def mutate(fixture):
            fixture["composedRevisionCandidates"][0]["status"] = "DIRECTOR_ACCEPTED_CORRECTION_CANDIDATE"
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "generation-only review/attempt boundary")

    def test_solar_correction_cannot_hide_third_attempt(self):
        def mutate(fixture):
            fixture["composedRevisionCandidates"][0]["attemptNumber"] = 3
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "generation-only review/attempt boundary")

    def test_solar_revision_requires_its_exact_image_hash(self):
        def mutate(fixture):
            fixture["composedRevisionCandidates"][0]["outputSha256"] = "0" * 64
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "solar revision PNG or hash drifted")

    def test_original_approval_cannot_authorize_revised_alien_anatomy(self):
        def mutate(fixture):
            fixture["composedRevisionProposals"][0].update(
                status="DIRECTOR_APPROVED_CONCEPT_GENERATION_ONLY",
                approvalEvidence="Historical original composition approval")
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "separate director generation approval")

    def test_biomechanical_canon_cannot_authorize_gameplay(self):
        self.assert_rejected(validator.ALIEN_BIOMECHANICAL_POLICY,
                             lambda fixture: fixture.update(gameplayImpact="SWARM_PRODUCTION"),
                             "gameplay or acceptance boundary")

    def test_biomechanical_canon_cannot_accept_images(self):
        self.assert_rejected(validator.ALIEN_BIOMECHANICAL_POLICY,
                             lambda fixture: fixture.update(assetAcceptance="ACCEPTED"),
                             "gameplay or acceptance boundary")

    def test_anatomical_inference_cannot_become_literal_source_fact(self):
        def mutate(fixture):
            fixture["sources"][0]["evidenceKind"] = "OFFICIAL_PROMO_LITERAL_LABEL"
        self.assert_rejected(validator.ALIEN_BIOMECHANICAL_POLICY, mutate,
                             "provenance or inference distinction")

    def test_current_alien_contract_cannot_restore_biology_ban(self):
        def mutate(fixture):
            fixture["sharedMaterialPlan"]["globalRules"].append("Aliens are never biological")
        self.assert_rejected(validator.ALIENS_CONTRACTS, mutate,
                             "absolute biology ban")

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

    def test_composed_generation_requires_director_evidence(self):
        self.assert_rejected(validator.COMPOSED_DESIGN_PROPOSALS,
                             lambda fixture: fixture.pop("approvalEvidence"),
                             "approval evidence or generation-only scope")

    def test_composed_generation_approval_cannot_expand_to_production(self):
        def mutate(fixture):
            fixture["approvalEvidence"]["scope"] = "All generated images and production contracts approved."
        self.assert_rejected(validator.COMPOSED_DESIGN_PROPOSALS, mutate,
                             "approval evidence or generation-only scope")

    def test_composed_candidate_requires_separate_image_acceptance(self):
        def mutate(fixture):
            fixture["composedDesignCandidates"][0]["status"] = "DIRECTOR_ACCEPTED_CORRECTION_CANDIDATE"
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "composed candidate was silently accepted")

    def test_composed_candidate_donor_set_is_protected(self):
        def mutate(fixture):
            fixture["composedDesignCandidates"][0]["sourceReferences"][0]["setId"] = "7691"
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "composed candidate changed its approved donor set")


if __name__ == "__main__":
    unittest.main()
