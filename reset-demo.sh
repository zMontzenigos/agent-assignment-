#!/bin/bash
set -e

SPEC_DIR="AgentAssignment/Specs"

rm -f "$SPEC_DIR/functional-spec.md" "$SPEC_DIR/technical-spec.md"

echo "Specs cleared."
echo "(CLAUDE.md is preserved — it accumulates project context across runs.)"
echo ""
echo "Run story 1 (password reset):"
echo "  cd AgentAssignment && DIFF_FILE=Specs/sample.diff STORY_FILE=Specs/stories/story-1-password-reset.md dotnet run"
echo ""
echo "Then run story 2 (account security) to see incremental update + CLAUDE.md evolution:"
echo "  DIFF_FILE=Specs/sample.diff STORY_FILE=Specs/stories/story-2-account-security.md dotnet run"
