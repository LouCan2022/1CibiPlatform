#!/usr/bin/env bash
# Repoints hardcoded colour literals in a stylesheet at the shared --c-* tokens
# declared in UI/FrontendWebassembly/wwwroot/css/theme.css.
#
# Used for the dark-mode rollout (see docs/ui-dark-mode-and-responsiveness.md). Kept
# in the repo so the same mapping can be replayed on any sheet that is added later or
# missed - re-running it on an already-converted file is a no-op.
#
#   Tools/tokenize-css.sh <file>...
#
# Only maps literals that appear in the CIBI palette. Anything left over after a run
# is either genuinely one-off or needs a human decision (e.g. white text on a dark
# gradient, which must become --c-on-dark-fg rather than --c-surface); list them with
#   grep -noE '#[0-9a-fA-F]{3,8}\b' <file>
set -euo pipefail

for f in "$@"; do
    [ -f "$f" ] || { echo "skip (missing): $f" >&2; continue; }

    # theme.css is where the literals are SUPPOSED to live. Rewriting them there
    # makes every token define itself (--c-surface: var(--c-surface)), which
    # silently blanks the entire palette.
    case "$(basename "$f")" in
        theme.css) echo "skip (token source): $f"; continue ;;
    esac

    sed -i -E \
        `# --- foreground --- ` \
        -e 's/#(16233f|1e2a3c)\b/var(--c-fg-strong)/gI' \
        -e 's/#(5b6b8c|6b7b92|4b5468|41536e|51627d|6f819b|43556f|3b4a61|355273|2c3f57|233a5a)\b/var(--c-fg-muted)/gI' \
        -e 's/#(8a97b5|8992a6|7d91ab|7d8aa8|7d899a|6e86a6|9aa8ba|97a8bc)\b/var(--c-fg-subtle)/gI' \
        `# --- borders --- ` \
        -e 's/#(d9e3f5|dce7f4|dce6f7|d9e2ee)\b/var(--c-border)/gI' \
        -e 's/#(e4eaf6|e3e9f1|e2e6f0|eef1f7|e6eaf2)\b/var(--c-border-soft)/gI' \
        -e 's/#(c2d2f0|c9daef|c9d9ec|c6d5f5|d7e3f3|d6e2ef|d5e5f4|d4e0ed|d3e2f2|d2e3fc|cfe0f4|c4dcf9|c6cedd)\b/var(--c-border-strong)/gI' \
        `# --- brand ramp --- ` \
        -e 's/#(0b1b3d|0b1f3a|102247|0a1c3f|263852)\b/var(--c-navy-900)/gI' \
        -e 's/#(132a54|0f294b|04224d|123a63|234c80|0f2a5c)\b/var(--c-navy-800)/gI' \
        -e 's/#(1c3a70)\b/var(--c-navy-700)/gI' \
        -e 's/#(1d5fd1|1769d2)\b/var(--c-blue-600)/gI' \
        -e 's/#(2e7ce0|2c7fb8|2a77ae|2f6fed|4f93ea|62a8ff|7fb2e6|4d8bff)\b/var(--c-blue-500)/gI' \
        -e 's/#(edf3fc|eef7ff|edf5fd|e8f2ff|e6edf5|e8f0fe|eaf4ff|eef4ff)\b/var(--c-blue-tint)/gI' \
        -e 's/#(e3ecfb)\b/var(--c-blue-tint-2)/gI' \
        `# --- surfaces --- ` \
        -e 's/#(f4f7fb|f4f6fb)\b/var(--c-page-bg)/gI' \
        -e 's/#(fcfdff|fbfcff|fafbff|f9fcff|f8fafc|f8f9fc|f7f9fc|f4f7fc|f3f9fd|f3f5fa|f1f4f8|f0f5fb|f0f4fa|f7fafe|eaf1f8|eef3f9|eff3f8|fbfdff|f8fbff|f7faff|f7fafd|f1f7ff|f1f4fa)\b/var(--c-surface-sunken)/gI' \
        `# --- status --- ` \
        -e 's/#(1e9e64|1f9d57|1e8e5a|34c77e|2e7d32|0e7a4c|16a34a)\b/var(--c-success)/gI' \
        -e 's/#(e4f7ed|e7f5ec|e8f5e9|e4f3ed|e9f9ef)\b/var(--c-success-bg)/gI' \
        -e 's/#(b5790f|b7791b|a15c07|8a6212|7a5209|8a6a1d|7a5a0d)\b/var(--c-warn)/gI' \
        -e 's/#(fcf1dd|fcf3de|fdf1dd|fdf7ec|f0dca8|e6d3a8|efd9a0|fff8e1)\b/var(--c-warn-bg)/gI' \
        -e 's/#(d64545|d8503a|e75a3c)\b/var(--c-danger)/gI' \
        -e 's/#(c0453e)\b/var(--c-danger-strong)/gI' \
        -e 's/#(fdecec|fdecea|fcece7|fbe9e9|fdeceb)\b/var(--c-danger-bg)/gI' \
        -e 's/#(eef0f5)\b/var(--c-neutral-bg)/gI' \
        -e 's/#(6b7690)\b/var(--c-neutral-fg)/gI' \
        `# --- white: text vs surface are different tokens, so split by property --- ` \
        -e 's/(\bcolor: *)#(fff|ffffff)\b/\1var(--c-on-dark-fg)/gI' \
        -e 's/(background[a-z-]*: *)#(fff|ffffff)\b/\1var(--c-surface)/gI' \
        "$f"

    remaining=$(grep -coE '#[0-9a-fA-F]{3,8}\b' "$f" || true)
    echo "$f -> ${remaining:-0} literal(s) left"
done
