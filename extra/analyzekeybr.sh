#!/bin/zsh

FILE=$1

# --- CONFIGURATION ---
# Define your focus keys here, separated by spaces
FOCUS_KEYS="& | { } 8 9 @ ! < > ? % *"

# --- STYLING & COLORS ---
BOLD=$(tput bold)
RESET=$(tput sgr0)
RED=$(tput setaf 1)
GREEN=$(tput setaf 2)
YELLOW=$(tput setaf 3)
BLUE=$(tput setaf 4)
MAGENTA=$(tput setaf 5)
CYAN=$(tput setaf 6)
GRAY=$(tput setaf 8)

# Function to print section headers
print_header() {
  echo ""
  echo "${BOLD}${MAGENTA}# ${1} ${RESET}"
}

if [ ! -f "$FILE" ]; then
  echo "${RED}Usage: analyzekeybr <file.json>${RESET}"
  exit 1
fi

if ! command -v jq &>/dev/null; then
  echo "${RED}Error: 'jq' is not installed. Install it with: brew install jq${RESET}"
  exit 1
fi

# Date calculations
MAX_DATE=$(jq -r 'map(.timeStamp) | max' "$FILE")

if date -v-3d >/dev/null 2>&1; then
  # BSD/macOS
  CURRENT_EPOCH=$(date -j -f "%Y-%m-%dT%H:%M:%S" "${MAX_DATE%.*}" +%s)
  ONE_DAY_AGO=$(date -j -r $((CURRENT_EPOCH - 1 * 86400)) +"%Y-%m-%dT%H:%M:%SZ")
  THREE_DAYS_AGO=$(date -j -r $((CURRENT_EPOCH - 3 * 86400)) +"%Y-%m-%dT%H:%M:%SZ")
  SEVEN_DAYS_AGO=$(date -j -r $((CURRENT_EPOCH - 7 * 86400)) +"%Y-%m-%dT%H:%M:%SZ")
  THIRTY_DAYS_AGO=$(date -j -r $((CURRENT_EPOCH - 30 * 86400)) +"%Y-%m-%dT%H:%M:%SZ")
else
  # Linux/GNU
  ONE_DAY_AGO=$(date -d "${MAX_DATE} -1 days" +"%Y-%m-%dT%H:%M:%SZ")
  THREE_DAYS_AGO=$(date -d "${MAX_DATE} -3 days" +"%Y-%m-%dT%H:%M:%SZ")
  SEVEN_DAYS_AGO=$(date -d "${MAX_DATE} -7 days" +"%Y-%m-%dT%H:%M:%SZ")
  THIRTY_DAYS_AGO=$(date -d "${MAX_DATE} -30 days" +"%Y-%m-%dT%H:%M:%SZ")
fi

# --- JQ CONFIGURATION ---
JQ_ARGS=(
  --arg R "$RED"
  --arg G "$GREEN"
  --arg Y "$YELLOW"
  --arg C "$CYAN"
  --arg X "$RESET"
  --arg L1 "$ONE_DAY_AGO"
  --arg L3 "$THREE_DAYS_AGO"
  --arg L7 "$SEVEN_DAYS_AGO"
  --arg FOCUS "$FOCUS_KEYS"
)

# Shared JQ logic for formatting
JQ_PREFIX='
  def sp: "          ";
  def lpad($str; $len): (sp + ($str|tostring))[-$len:];
  def rpad($str; $len): (($str|tostring) + sp)[:$len];

  # Helper to force exactly 2 decimal places
  def to_2dp($val):
    ($val * 100 | round | tostring) as $s |
    if ($s | length) == 1 then "0.0" + $s
    elif ($s | length) == 2 then "0." + $s
    else $s[:-2] + "." + $s[-2:]
    end;

  def color_err($val):
    if $val < 2.0 then $G elif $val < 5.0 then $Y else $R end;

  def color_acc($val):
    if $val >= 98.0 then $G elif $val >= 95.0 then $Y else $R end;

  def fmt_err($val; $w):
    ($val * 100 | round / 100) as $v_num |
    to_2dp($val) as $v_str |
    color_err($v_num) + lpad($v_str + "%"; $w) + $X;

  def fmt_acc($val; $w):
    ($val * 100 | round / 100) as $v_num |
    to_2dp($val) as $v_str |
    color_acc($v_num) + lpad($v_str + "%"; $w) + $X;

  def fmt_wpm($val; $w):
    to_2dp($val) as $v_str |
    $C + lpad("⚡" + $v_str; $w - 1) + $X;
'

echo "${BOLD}${BLUE}========================================${RESET}"
echo "${BOLD}    ⌨️    TYPING ANALYSIS REPORT    📊${RESET}"
echo "${BOLD}${BLUE}========================================${RESET}"

# 1. General Stats
get_general_stats() {
  local filter=$1
  local label=$2
  stats=$(jq -r "$filter | if length == 0 then \"none\" else
        (length as \$len | {
           sessions: \$len,
           avg_speed: (map(.speed) | add / \$len),
           avg_acc: (map(1 - (.errors / .length)) | add / \$len * 100)
        }) end" "$FILE")

  if [ "$stats" != "none" ]; then
    print_header "$label"

    sessions=$(echo "$stats" | jq -r '.sessions')
    avg_speed=$(echo "$stats" | jq -r '.avg_speed')
    avg_acc=$(echo "$stats" | jq -r '.avg_acc')
    wpm=$(echo "$avg_speed / 5" | bc -l)

    ACC_COLOR=$RED
    if (($(echo "$avg_acc >= 98" | bc -l))); then
      ACC_COLOR=$GREEN
    elif (($(echo "$avg_acc >= 95" | bc -l))); then
      ACC_COLOR=$YELLOW
    fi

    printf "  %s%-12s%s %s%-16s%s %s%-16s%s\n" \
      "${GRAY}" "SESSIONS" "${RESET}" \
      "${GRAY}" "AVG SPEED" "${RESET}" \
      "${GRAY}" "ACCURACY" "${RESET}"

    printf "  ${BOLD}%-12s${RESET} ${CYAN}⚡%-13.2f${RESET} ${ACC_COLOR}🎯 %-13.2f%%${RESET}\n" \
      "$sessions" "$wpm" "$avg_acc"
  fi
}

training_state() {
  print_header "TRAINING PROGRESS SUMMARY"
  echo "${BOLD}Unlocked keys${RESET} ${GREEN}✓${RESET}:"
  echo "  ${GREEN}abcdefghijklmnopqrstuvwxyz${RESET}"
  echo "  ${GREEN}ABCDEFGHIJKLMNOPQRSTUVWXYZ${RESET}"
  echo "  ${GREEN}; : . ,${RESET}"
  echo "  ${GREEN}_ ' \" ( ) [ ] { }${RESET}"
  echo "  ${GREEN}/ + - = * < >${RESET}"
  echo "  ${GREEN}! @ % & |${RESET}"
  echo "  ${GREEN}1 2 3 4 5 7 8 9 0${RESET}"

  echo "${BOLD}Focus keys (Structural)${RESET} ${YELLOW}→${RESET}:"
  echo "  ${YELLOW}${FOCUS_KEYS}${RESET}"

  echo "${BOLD}Locked: Tier 3 (Advanced C#)${RESET} ${RED}🔒${RESET}:"
  echo "  ${RED}@ # ^ \ ~${RESET}"
  echo "${BOLD}Locked: Tier 4 (Data/Constants)${RESET} ${RED}🔒${RESET}:"
  echo "  ${RED}6${RESET}"
}

training_state
get_general_stats "." "ALL-TIME STATS"
get_general_stats "map(select(.timeStamp >= \"$THIRTY_DAYS_AGO\"))" "LAST 30 DAYS"
get_general_stats "map(select(.timeStamp >= \"$ONE_DAY_AGO\"))" "LAST 24 HOURS"

# 1.5. Focus Keys Stats (NEW SECTION)
print_header "FOCUS KEYS PERFORMANCE ($FOCUS_KEYS)"
printf "${BOLD}%-5s %-10s %-10s %-9s %-10s %-9s %-10s %-9s %-10s %-9s${RESET}\n" "Key" "Latency" "All(H/M)" "All_Err" "L7(H/M)" "L7_Err" "L3(H/M)" "L3_Err" "L1(H/M)" "L1_Err"

jq -r "${JQ_ARGS[@]}" "$JQ_PREFIX"'
  [ .[] as $session | $session.histogram[] | . + {ts: $session.timeStamp} ]
  | group_by(.codePoint) | map({
    key: ([.[0].codePoint] | implode | if . == " " then "SPC" else . end),
    all_h: (map(.hitCount) | add),
    all_m: (map(.missCount) | add),
    total_time: (map(.timeToType * .hitCount) | add),
    l7_h: (map(select(.ts >= $L7) | .hitCount) | add // 0),
    l7_m: (map(select(.ts >= $L7) | .missCount) | add // 0),
    l3_h: (map(select(.ts >= $L3) | .hitCount) | add // 0),
    l3_m: (map(select(.ts >= $L3) | .missCount) | add // 0),
    l1_h: (map(select(.ts >= $L1) | .hitCount) | add // 0),
    l1_m: (map(select(.ts >= $L1) | .missCount) | add // 0)
  })
  | map(select(.key as $k | ($FOCUS | split(" ") | index($k))))
  | map(. + {
    latency: (if .all_h > 0 then (.total_time / .all_h) else 0 end),
    all_err: ((.all_m / (.all_h + .all_m + 0.000001)) * 100),
    l7_err: ((.l7_m / (.l7_h + .l7_m + 0.000001)) * 100),
    l3_err: ((.l3_m / (.l3_h + .l3_m + 0.000001)) * 100),
    l1_err: ((.l1_m / (.l1_h + .l1_m + 0.000001)) * 100)
  })
  | sort_by(.l1_err) | reverse | .[]
  | rpad(.key; 5) + " " +
    lpad((.latency | floor | tostring) + "ms"; 9) + " " +
    lpad("\(.all_h)/\(.all_m)"; 10) + " " +
    fmt_err(.all_err; 9) + " " +
    lpad("\(.l7_h)/\(.l7_m)"; 10) + " " +
    fmt_err(.l7_err; 9) + " " +
    lpad("\(.l3_h)/\(.l3_m)"; 10) + " " +
    fmt_err(.l3_err; 9) + " " +
    lpad("\(.l1_h)/\(.l1_m)"; 10) + " " +
    fmt_err(.l1_err; 9)
' "$FILE"

# 2. Key-specific Hits/Misses Table
print_header "TOP 50 KEY PERFORMANCE (Sorted by 1-day Error Rate)"
printf "${BOLD}%-5s %-10s %-10s %-9s %-10s %-9s %-10s %-9s %-10s %-9s${RESET}\n" "Key" "Latency" "All(H/M)" "All_Err" "L7(H/M)" "L7_Err" "L3(H/M)" "L3_Err" "L1(H/M)" "L1_Err"

jq -r "${JQ_ARGS[@]}" "$JQ_PREFIX"'
  [ .[] as $session | $session.histogram[] | . + {ts: $session.timeStamp} ]
  | group_by(.codePoint) | map({
    key: ([.[0].codePoint] | implode | if . == " " then "SPC" else . end),
    all_h: (map(.hitCount) | add),
    all_m: (map(.missCount) | add),
    total_time: (map(.timeToType * .hitCount) | add),
    l7_h: (map(select(.ts >= $L7) | .hitCount) | add // 0),
    l7_m: (map(select(.ts >= $L7) | .missCount) | add // 0),
    l3_h: (map(select(.ts >= $L3) | .hitCount) | add // 0),
    l3_m: (map(select(.ts >= $L3) | .missCount) | add // 0),
    l1_h: (map(select(.ts >= $L1) | .hitCount) | add // 0),
    l1_m: (map(select(.ts >= $L1) | .missCount) | add // 0)
  }) | map(. + {
    latency: (if .all_h > 0 then (.total_time / .all_h) else 0 end),
    all_err: ((.all_m / (.all_h + .all_m + 0.000001)) * 100),
    l7_err: ((.l7_m / (.l7_h + .l7_m + 0.000001)) * 100),
    l3_err: ((.l3_m / (.l3_h + .l3_m + 0.000001)) * 100),
    l1_err: ((.l1_m / (.l1_h + .l1_m + 0.000001)) * 100)
  })
  | sort_by(.l1_err) | reverse | .[:50] | .[]
  | rpad(.key; 5) + " " +
    lpad((.latency | floor | tostring) + "ms"; 9) + " " +
    lpad("\(.all_h)/\(.all_m)"; 10) + " " +
    fmt_err(.all_err; 9) + " " +
    lpad("\(.l7_h)/\(.l7_m)"; 10) + " " +
    fmt_err(.l7_err; 9) + " " +
    lpad("\(.l3_h)/\(.l3_m)"; 10) + " " +
    fmt_err(.l3_err; 9) + " " +
    lpad("\(.l1_h)/\(.l1_m)"; 10) + " " +
    fmt_err(.l1_err; 9)
' "$FILE"

# 3. Slowest Keys
print_header "TOP 50 SLOWEST KEYS (Sorted by Latency)"
printf "${BOLD}%-5s %-10s %-10s %-9s %-10s %-9s %-10s %-9s %-10s %-9s${RESET}\n" "Key" "Latency" "All(H/M)" "All_Err" "L7(H/M)" "L7_Err" "L3(H/M)" "L3_Err" "L1(H/M)" "L1_Err"

jq -r "${JQ_ARGS[@]}" "$JQ_PREFIX"'
  [ .[] as $session | $session.histogram[] | . + {ts: $session.timeStamp} ]
  | group_by(.codePoint) | map({
    key: ([.[0].codePoint] | implode | if . == " " then "SPC" else . end),
    all_h: (map(.hitCount) | add),
    all_m: (map(.missCount) | add),
    total_time: (map(.timeToType * .hitCount) | add),
    l7_h: (map(select(.ts >= $L7) | .hitCount) | add // 0),
    l7_m: (map(select(.ts >= $L7) | .missCount) | add // 0),
    l3_h: (map(select(.ts >= $L3) | .hitCount) | add // 0),
    l3_m: (map(select(.ts >= $L3) | .missCount) | add // 0),
    l1_h: (map(select(.ts >= $L1) | .hitCount) | add // 0),
    l1_m: (map(select(.ts >= $L1) | .missCount) | add // 0)
  }) | map(. + {
    latency: (if .all_h > 0 then (.total_time / .all_h) else 0 end),
    all_err: ((.all_m / (.all_h + .all_m + 0.000001)) * 100),
    l7_err: ((.l7_m / (.l7_h + .l7_m + 0.000001)) * 100),
    l3_err: ((.l3_m / (.l3_h + .l3_m + 0.000001)) * 100),
    l1_err: ((.l1_m / (.l1_h + .l1_m + 0.000001)) * 100)
  })
  | sort_by(.latency) | reverse | .[:50] | .[]
  | rpad(.key; 5) + " " +
    lpad((.latency | floor | tostring) + "ms"; 9) + " " +
    lpad("\(.all_h)/\(.all_m)"; 10) + " " +
    fmt_err(.all_err; 9) + " " +
    lpad("\(.l7_h)/\(.l7_m)"; 10) + " " +
    fmt_err(.l7_err; 9) + " " +
    lpad("\(.l3_h)/\(.l3_m)"; 10) + " " +
    fmt_err(.l3_err; 9) + " " +
    lpad("\(.l1_h)/\(.l1_m)"; 10) + " " +
    fmt_err(.l1_err; 9)
' "$FILE"

# 4. Detailed Daily Table
print_header "DAILY PROGRESS TABLE"
# Adjusted header widths to match JQ output exactly
printf "${BOLD}%-12s %-10s %-7s %-10s %-10s %-10s %-10s %-10s${RESET}\n" "DATE" "TIME" "SESS" "AVG_WPM" "MAX_WPM" "AVG_ACC" "AVG_ERR" "TOP_ACC"

jq -r "${JQ_ARGS[@]}" "$JQ_PREFIX"'group_by(.timeStamp[:10]) | .[] | {
    date: .[0].timeStamp[:10],
    time: (map(.time) | add / 1000 | floor),
    lessons: length,
    avg_wpm: (map(.speed) | add / length / 5),
    max_wpm: (map(.speed) | max / 5),
    avg_acc: (map(1 - (.errors / .length)) | add / length * 100),
    avg_err: (map(.errors / .length) | add / length * 100),
    max_acc: (map(1 - (.errors / .length)) | max * 100)
  } | rpad(.date; 12) + " " +
      rpad(
        (.time / 3600 | floor | if . < 10 then "0" + (.|tostring) else (. | tostring) end) + ":" +
        (.time % 3600 / 60 | floor | if . < 10 then "0" + (.|tostring) else (. | tostring) end) + ":" +
        (.time % 60 | if . < 10 then "0" + (.|tostring) else (. | tostring) end)
      ; 10) + " " + # Added explicit space
      lpad(.lessons; 6) + "  " + # Adjusted sess width
      fmt_wpm(.avg_wpm; 9) + " " +
      fmt_wpm(.max_wpm; 9) + " " +
      fmt_acc(.avg_acc; 9) + " " +
      fmt_err(.avg_err; 9) + " " +
      fmt_acc(.max_acc; 9)
  ' "$FILE" | tail -n 30

# 5. All Keys Stats
print_header "ALL KEYS PERFORMANCE (Sorted by ASCII)"
printf "${BOLD}%-5s %-10s %-9s %-9s %-10s %-9s %-9s %-10s %-9s %-9s %-10s %-9s %-9s${RESET}\n" "Key" "All(H/M)" "All_Err" "All_WPM" "L7(H/M)" "L7_Err" "L7_WPM" "L3(H/M)" "L3_Err" "L3_WPM" "L1(H/M)" "L1_Err" "L1_WPM"

jq -r "${JQ_ARGS[@]}" "$JQ_PREFIX"'
  [ .[] as $session | $session.histogram[] | . + {ts: $session.timeStamp} ]
  | group_by(.codePoint) | map({
    code: .[0].codePoint,
    key: ([.[0].codePoint] | implode | if . == " " then "SPC" else . end),
    all_h: (map(.hitCount) | add),
    all_m: (map(.missCount) | add),
    all_t: (map(.timeToType * .hitCount) | add),
    l7_h: (map(select(.ts >= $L7) | .hitCount) | add // 0),
    l7_m: (map(select(.ts >= $L7) | .missCount) | add // 0),
    l7_t: (map(select(.ts >= $L7) | .timeToType * .hitCount) | add // 0),
    l3_h: (map(select(.ts >= $L3) | .hitCount) | add // 0),
    l3_m: (map(select(.ts >= $L3) | .missCount) | add // 0),
    l3_t: (map(select(.ts >= $L3) | .timeToType * .hitCount) | add // 0),
    l1_h: (map(select(.ts >= $L1) | .hitCount) | add // 0),
    l1_m: (map(select(.ts >= $L1) | .missCount) | add // 0),
    l1_t: (map(select(.ts >= $L1) | .timeToType * .hitCount) | add // 0)
  })
  | sort_by(.code)
  | .[]
  | {
      k: .key,
      ahm: "\(.all_h)/\(.all_m)",
      ae: ((.all_m / (.all_h + .all_m + 0.000001)) * 100),
      aw: (if .all_h > 0 then (12000 / ((.all_t / .all_h) + 0.000001)) | if . > 100 then 100 else . end else 0 end),

      l7hm: "\(.l7_h)/\(.l7_m)",
      l7e: ((.l7_m / (.l7_h + .l7_m + 0.000001)) * 100),
      l7w: (if .l7_h > 0 then (12000 / ((.l7_t / .l7_h) + 0.000001)) | if . > 100 then 100 else . end else 0 end),

      l3hm: "\(.l3_h)/\(.l3_m)",
      l3e: ((.l3_m / (.l3_h + .l3_m + 0.000001)) * 100),
      l3w: (if .l3_h > 0 then (12000 / ((.l3_t / .l3_h) + 0.000001)) | if . > 100 then 100 else . end else 0 end),

      l1hm: "\(.l1_h)/\(.l1_m)",
      l1e: ((.l1_m / (.l1_h + .l1_m + 0.000001)) * 100),
      l1w: (if .l1_h > 0 then (12000 / ((.l1_t / .l1_h) + 0.000001)) | if . > 100 then 100 else . end else 0 end)
    }
  | rpad(.k; 5) + " " +
    lpad(.ahm; 10) + " " +
    fmt_err(.ae; 9) + " " +
    fmt_wpm(.aw; 9) + " " +
    lpad(.l7hm; 10) + " " +
    fmt_err(.l7e; 9) + " " +
    fmt_wpm(.l7w; 9) + " " +
    lpad(.l3hm; 10) + " " +
    fmt_err(.l3e; 9) + " " +
    fmt_wpm(.l3w; 9) + " " +
    lpad(.l1hm; 10) + " " +
    fmt_err(.l1e; 9) + " " +
    fmt_wpm(.l1w; 9)
' "$FILE"

echo ""
