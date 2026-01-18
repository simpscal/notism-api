#!/bin/bash

# Script to format C# code using dotnet format
# Usage: ./format-code.sh [options]
# Options:
#   --verify      Only check if code needs formatting (doesn't modify files)
#   --whitespace  Fix whitespace issues only
#   --style       Fix code style issues only
#   --analyzers   Fix analyzer issues only
#   (no args)     Fix all issues (whitespace, style, and analyzers)

set -e

# Add .NET tools to PATH if not already there
export PATH="$PATH:$HOME/.dotnet/tools"

# Default: fix all issues
if [ $# -eq 0 ]; then
    echo "Running dotnet format: whitespace, style, and analyzers"
    echo ""
    dotnet format Notism.sln whitespace
    dotnet format Notism.sln style
    dotnet format Notism.sln analyzers
    echo ""
    echo "Formatting complete!"
    exit 0
fi

# Parse arguments
case "$1" in
    --verify)
        echo "Verifying code formatting (no changes will be made)..."
        echo ""
        dotnet format Notism.sln whitespace --verify-no-changes
        dotnet format Notism.sln style --verify-no-changes
        dotnet format Notism.sln analyzers --verify-no-changes
        echo ""
        echo "Verification complete!"
        ;;
    --whitespace)
        echo "Running dotnet format: whitespace only"
        echo ""
        dotnet format Notism.sln whitespace
        echo ""
        echo "Whitespace formatting complete!"
        ;;
    --style)
        echo "Running dotnet format: code style only"
        echo ""
        dotnet format Notism.sln style
        echo ""
        echo "Code style formatting complete!"
        ;;
    --analyzers)
        echo "Running dotnet format: analyzers only"
        echo ""
        dotnet format Notism.sln analyzers
        echo ""
        echo "Analyzer fixes complete!"
        ;;
    *)
        echo "Usage: ./format-code.sh [--verify|--whitespace|--style|--analyzers]"
        echo ""
        echo "Options:"
        echo "  --verify      Only check if code needs formatting (doesn't modify files)"
        echo "  --whitespace  Fix whitespace issues only"
        echo "  --style       Fix code style issues only"
        echo "  --analyzers   Fix analyzer issues only"
        echo "  (no args)     Fix all issues (default)"
        exit 1
        ;;
esac

