# Embedded item description sources

These two JSON files preserve the exact user-supplied English and Simplified
bytes. Build embeds both under PeakAdminToolkit.Descriptions.*. No JSON file
is generated or read in BepInEx/config. Traditional text is converted at runtime
by Windows; no third duplicate dataset is embedded.

Each dataset has 131 entries. Only items[*].description is consumed. The
original type/color fields remain intact but do not alter toolkit rules.
See ../../THIRD_PARTY_NOTICES.md for hashes and unresolved author/license.
