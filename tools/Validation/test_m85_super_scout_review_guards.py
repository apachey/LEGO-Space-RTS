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
    def test_composition_cannot_accept_production(self):
        self.assert_rejected(validator.COMPLETED_COMPOSITION_REVIEW,
                             lambda f: f.update(productionAccepted=True), "composition approval evidence or limited scope")

    def test_composition_requires_exact_approval_evidence(self):
        self.assert_rejected(validator.COMPLETED_COMPOSITION_REVIEW,
                             lambda f: f.pop("approvalEvidence"), "composition approval evidence or limited scope")

    def test_composition_cannot_expand_roster_approval(self):
        self.assert_rejected(validator.COMPLETED_COMPOSITION_REVIEW,
                             lambda f: f["selections"].append(f["selections"][0]), "composition selection scope")

    def test_composition_cannot_substitute_image(self):
        self.assert_rejected(validator.COMPLETED_COMPOSITION_REVIEW,
                             lambda f: f["selections"][0].update(sha256="0"*64), "composition selected status or image")

    def test_mothership_cannot_hide_wrong_operator(self):
        self.assert_rejected(validator.COMPLETED_COMPOSITION_REVIEW,
                             lambda f: f["selections"][1].update(pending=[]), "hid unresolved source corrections")

    def test_native_correction_cannot_accept_production(self):
        self.assert_rejected(validator.SOURCE_LOCKED_CORRECTIONS,
                             lambda f: f.update(productionAccepted=True), "expanded review or production approval")

    def test_native_correction_cannot_substitute_image(self):
        self.assert_rejected(validator.SOURCE_LOCKED_CORRECTIONS,
                             lambda f: f["images"]["MT101"].update(sha256="0"*64), "selected image drifted")

    def test_native_correction_keeps_six_mt_contacts(self):
        def mutate(f):
            next(a for a in f["controls"]["MT101"] if a["kind"]=="main_contact_wheel")["kind"]="missing"
        self.assert_rejected(validator.SOURCE_LOCKED_CORRECTIONS, mutate, "MT six-contact topology")

    def test_native_correction_keeps_permanent_cabin(self):
        def mutate(f):
            next(a for a in f["controls"]["MT101"] if a["name"]=="MT101_PermanentCockpit")["parent"]="MT101_DockedRearSpacecraft"
        self.assert_rejected(validator.SOURCE_LOCKED_CORRECTIONS, mutate, "permanent closed MT cockpit")

    def test_native_correction_keeps_independent_tools(self):
        def mutate(f):
            next(a for a in f["controls"]["MT101"] if a["kind"]=="separate_drill")["parent"]="MT101_UpperBallLauncher"
        self.assert_rejected(validator.SOURCE_LOCKED_CORRECTIONS, mutate, "fused independent MT tools")

    def test_native_correction_keeps_mirrored_emitters(self):
        def mutate(f):
            next(a for a in f["controls"]["MX71"] if a["kind"]=="airframe_emitter")["anchor"][0]+=1
        self.assert_rejected(validator.SOURCE_LOCKED_CORRECTIONS, mutate, "mirrored MX emitter pairs")

    def test_native_correction_keeps_source_payload(self):
        def mutate(f):
            next(a for a in f["controls"]["MX71"] if a["kind"]=="source_payload")["kind"]="missing"
        self.assert_rejected(validator.SOURCE_LOCKED_CORRECTIONS, mutate, "close source payload")

    def test_completed_appearance_requires_director_evidence(self):
        self.assert_rejected(validator.COMPLETED_ALIEN_APPEARANCE,
                             lambda fixture: fixture.pop("approval_evidence"),
                             "appearance approval evidence or scope")

    def test_completed_appearance_cannot_accept_production(self):
        self.assert_rejected(validator.COMPLETED_ALIEN_APPEARANCE,
                             lambda fixture: fixture.update(production_accepted=True),
                             "expanded acceptance beyond images")

    def test_completed_appearance_cannot_expand_approval_scope(self):
        def mutate(fixture):
            fixture["approval_evidence"]["scope"] = "Accept the whole T082 corpus"
        self.assert_rejected(validator.COMPLETED_ALIEN_APPEARANCE, mutate,
                             "appearance approval evidence or scope")

    def test_completed_appearance_must_retain_both_defense_heads(self):
        self.assert_rejected(validator.COMPLETED_ALIEN_APPEARANCE,
                             lambda fixture: fixture["assets"].pop(),
                             "lost a reviewed configuration")

    def test_completed_appearance_cannot_substitute_a_draft(self):
        def mutate(fixture):
            fixture["assets"][0]["output"] = "servitor_finished.png"
        self.assert_rejected(validator.COMPLETED_ALIEN_APPEARANCE, mutate,
                             "selected an internal construction render")

    def test_completed_appearance_selected_hash_is_locked(self):
        def mutate(fixture):
            fixture["assets"][0]["sha256"] = "0" * 64
        self.assert_rejected(validator.COMPLETED_ALIEN_APPEARANCE, mutate,
                             "identity or selected image hash")

    def test_completed_appearance_cannot_hide_new_variants(self):
        def mutate(fixture):
            fixture["assets"][0]["completed_appearance_variants"] = 2
        self.assert_rejected(validator.COMPLETED_ALIEN_APPEARANCE, mutate,
                             "hid an extra selected variant")

    def test_alien_visual_preview_cannot_accept_composition(self):
        def mutate(fixture):
            fixture["composedRevisionProposals"][0]["status"] = "DIRECTOR_ACCEPTED_CORRECTION_CANDIDATE"
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "separate director generation approval")

    def test_air_lance_preview_requires_common_base_edit_lineage(self):
        def mutate(fixture):
            fixture["composedRevisionProposals"][2]["generationInputImages"] = []
        self.assert_rejected(validator.FULL_V2_MANIFEST, mutate,
                             "generation/edit lineage")

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
