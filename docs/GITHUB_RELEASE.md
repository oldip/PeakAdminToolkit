# GitHub upload and release checklist

## Upload the current source

Extract PeakAdminToolkit-0.8.1-source.zip and upload the CONTENTS of its single root folder
as the repository root. Do not upload the surrounding local version archive, Managed,
old, refs, out, tests/out, game/dependency DLLs or personal logs.
The source ZIP contains LICENSE, README, translations, tests, notices and .github templates.
Keep your local archived releases unchanged. Future published versions can use Git tags.

## Preview release

Use 0.8.1 as a preview, with known limitations linked to TESTING.md.
A runtime release can contain PeakAdminToolkit.dll plus LICENSE and
THIRD_PARTY_NOTICES.md and docs/PEAK_ITEM_TOOLTIP_LICENSE.txt.
No extra dependency DLLs are needed in the runtime release.
This preparation does not create a repository or publish a release.

## Before 1.0.0

Complete the live regression matrix in RELEASE_READINESS.md and record both players'
observations, game build, toolkit version and other mods. Freeze the feature set at
0.9.x, fix reported regressions, rerun offline checks/build/source-package inspection.
Offline checks do not confirm Unity physics or remote delivery.
