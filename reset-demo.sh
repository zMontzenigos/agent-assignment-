#!/bin/bash
set -e

SPEC_DIR="AgentAssignment/Specs"

rm -f "$SPEC_DIR/functional-spec.md" "$SPEC_DIR/technical-spec.md"

echo "Specs cleared. Ready to demo."
echo ""
echo "  cd AgentAssignment && dotnet run"
echo ""
echo "Press Enter between each story. Story 3 demos the mismatch detection."
