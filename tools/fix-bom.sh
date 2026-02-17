#!/bin/zsh

echo "🔍 Scanning Git-tracked files for UTF-8 BOM (EF BB BF)..."
BOM_FOUND=false
BOM_PATTERN=$'\xef\xbb\xbf'

# Detect the correct 'sed -i' syntax (GNU vs. BSD)
if sed --version 2>/dev/null | grep -q GNU; then
    SED_INPLACE_OPTION='-i'
else
    # Assumes BSD/macOS sed which requires an empty string argument to -i
    SED_INPLACE_OPTION='-i ""'
fi

# Use git ls-files -z for null-separated, safe filename handling
git ls-files -z | while IFS= read -r -d $'\0' file; do
    # Check if the file is large enough to contain the 3-byte BOM
    if [[ $(stat -c %s "$file" 2>/dev/null || stat -f %z "$file" 2>/dev/null) -lt 3 ]]; then
        continue # Skip files smaller than 3 bytes
    fi

    # Check for the BOM at the very beginning of the file
    if head -c 3 -- "$file" | LC_ALL=C grep -q "${BOM_PATTERN}"; then
        BOM_FOUND=true
        echo "\n⚠️  BOM detected in: ${file}"

        # Build the sed command
        # The 'eval' is used to correctly handle the SED_INPLACE_OPTION with quotes
        SED_COMMAND="sed ${SED_INPLACE_OPTION} '1s/^${BOM_PATTERN}//' \"${file}\""

        # Execute and check the command's exit status
        eval "${SED_COMMAND}"

        if [ $? -eq 0 ]; then
            echo "✅ Successfully removed BOM."
        else
            echo "❌ ERROR: Failed to remove BOM from ${file}. Reverting changes if possible."
            # A basic error handling step: if sed fails, notify the user.
            # Git can usually revert the file if sed caused a corruption.
        fi
    fi

done

if ! $BOM_FOUND; then
    echo "\n🎉 No UTF-8 BOM found in any tracked files."
fi
