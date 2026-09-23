#!/usr/bin/env python3
"""Check shell profile dispatch without launching any build, engine or export."""
from pathlib import Path
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
VERIFY = ROOT / 'tools/verify.sh'


def plan(*args):
    result = subprocess.run(['bash', str(VERIFY), *args, '--list-stages'],
                            cwd=ROOT, text=True, capture_output=True, check=True)
    lines = result.stdout.splitlines()
    stages = [line.split(' | ', 1)[0] for line in lines]
    if len(stages) != len(set(stages)):
        raise AssertionError(f'Duplicate stages: {stages}')
    return set(stages), result.stdout


class VerifyProfiles(unittest.TestCase):
    def test_bounded_document_and_design_work(self):
        self.assertEqual(plan('--targeted', 'workflow')[0],
                         {'static', 'shell-syntax', 'profile-routing'})
        for scope in ('design-package', 'references'):
            self.assertEqual(plan('--targeted', scope)[0],
                             {'static', 'm85-super-scout', 'm85-review-guards'})

    def test_default_core_does_not_sweep_historical_fixtures(self):
        expected = {'static', 'restore', 'build', 'godot-build', 'tests',
                    'content', 'headless', 'godot'}
        self.assertEqual(plan()[0], expected)
        self.assertEqual(plan('--targeted', 'core')[0], expected)

    def test_runtime_scopes_are_isolated(self):
        groups = {
            'network': {'m6-transport', 'm6-command', 'm6-snapshot',
                        'm6-reconnect', 'm6-replay'},
            'assets': {'m85-asset-static', 'm85-asset-godot'},
            'material': {'m7-material'}, 'style': {'m7-style'},
            'palette': {'m7-palette'}, 'look': {'m7-look'},
            'hud': {'m7-hud'}, 'visual': {'m7-acceptance'},
        }
        base = {'static', 'restore', 'build', 'godot-build'}
        for scope, expected in groups.items():
            with self.subTest(scope=scope):
                self.assertEqual(plan('--targeted', scope)[0], base | expected)

    def test_integration_and_full_preserve_gates(self):
        integration, _ = plan('--integration')
        self.assertEqual(integration, plan()[0] | {
            'm6-transport', 'm6-command', 'm6-snapshot', 'm6-reconnect',
            'm6-replay', 'm85-asset-static', 'm85-asset-godot',
            'm85-super-scout', 'm85-review-guards', 'm7-hud', 'm7-production'})
        full, full_text = plan('--full')
        self.assertEqual(full, (integration - {'m7-production'}) | {
            'm7-material', 'm7-style', 'm7-palette', 'm7-look', 'm7-acceptance',
            'golden100', 'replay', 'snapshot', 'stress60', 'content-regenerate',
            'm85-asset-regenerate', 'macos-export', 'shell-syntax', 'profile-routing'})
        self.assertEqual(plan('--milestone')[0], full)
        m9, m9_text = plan('--m9-acceptance')
        self.assertEqual(m9, full)
        self.assertIn('DIAGNOSTIC: [BLOCKING_LATER M9', full_text)
        self.assertNotIn('DIAGNOSTIC:', m9_text)
        self.assertIn('[BLOCKING_NOW M9', m9_text)
        self.assertNotIn('m2-movement', full)

    def test_invalid_arguments_fail_before_execution(self):
        for args in (['--targeted'], ['--targeted', 'unknown'], ['--unknown'],
                     ['--integration', '--full']):
            with self.subTest(args=args):
                result = subprocess.run(['bash', str(VERIFY), *args, '--list-stages'],
                                        cwd=ROOT, capture_output=True)
                self.assertEqual(result.returncode, 2)


if __name__ == '__main__':
    unittest.main()
