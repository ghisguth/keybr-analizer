#!/bin/zsh

echo "🔄 Scanning Git-tracked files for CRLF (Windows) line endings and converting to LF..."
CRLF_FOUND=false

# Detect the correct 'sed -i' syntax (GNU vs. BSD)
if sed --version 2>/dev/null | grep -q GNU; then
    SED_INPLACE_OPTION='-i'
else
    # Assumes BSD/macOS sed which requires an empty string argument to -i
    SED_INPLACE_OPTION='-i ""'
fi

# Use git ls-files -z to safely iterate over all tracked files (removed the problematic -I flag)
# The check-attr command will now be the sole filter for binary files.
git ls-files -z | while IFS= read -r -d $'\0' file; do

    # Check the Git 'text' attribute. This is the official way to skip binary files.
    # Files marked 'binary' in .gitattributes (like *.pdf) will show 'unset' or 'false'.
    # We only want to process files that Git explicitly considers text ('set') or auto-detects ('auto').
    TEXT_ATTR=$(git check-attr text -- "$file" | awk '{print $NF}')
    if [[ "$TEXT_ATTR" != "set" ]] && [[ "$TEXT_ATTR" != "auto" ]]; then
        # Skip files determined to be non-text (like your *.pdf files)
        continue
    fi

    # ----------------------------------------------------------------------
    # If control reaches here, the file is considered a text file.
    # ----------------------------------------------------------------------

    # Check if the file contains the CRLF pattern (\r\n)
    if LC_ALL=C grep -ql $'\r$' "$file"; then
        CRLF_FOUND=true
        echo "\n⚠️  CRLF detected and converting in: ${file}"

        # Build the sed command to replace \r\n with \n globally
        SED_COMMAND="sed ${SED_INPLACE_OPTION} 's/\r$//' \"${file}\""

        # Execute and check the command's exit status
        eval "${SED_COMMAND}"

        if [ $? -eq 0 ]; then
            echo "✅ Successfully converted CRLF to LF."
        else
            echo "❌ ERROR: Failed to convert line endings in ${file}. Manual review needed."
        fi
    fi

done

if ! $CRLF_FOUND; then
    echo "\n🎉 No CRLF line endings were found or converted."
fi
