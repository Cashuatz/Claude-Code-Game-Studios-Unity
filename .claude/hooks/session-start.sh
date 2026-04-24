#!/bin/bash
# Claude Code SessionStart hook: Load project context at session start
# Outputs context information that Claude sees when a session begins
#
# Input schema (SessionStart): No stdin input

BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)

# --- Proto 브랜치: 프로토타이핑 경량 모드 환영 메시지만 출력 ---
case "$BRANCH" in
    Proto|Proto/*|claude/*)
        # Proto나 Proto에서 파생된 claude/* 워크트리 브랜치면 경량 모드
        if [ "$BRANCH" = "Proto" ] || [ "${BRANCH#Proto/}" != "$BRANCH" ]; then
            echo "=== Proto 브랜치 — 프로토타이핑 경량 모드 (URP 3D) ==="
            echo ""
            echo "시작: 첫 메시지에 한 문장으로 아이디어를 말씀하세요"
            echo "      예: \"좀비 피해서 3분 버티는 게임\""
            echo "가이드: docs/PROTO-QUICKSTART.ko.md"
            echo ""
            echo "목표: 20시간 / 3분 플레이 빌드"
            echo ""
            echo "VKL: 검증 필요 시 /verify 호출 (원칙은 .claude/docs/vkl-bridge.md 로드됨)"
            echo "==================================="
            exit 0
        fi
        ;;
esac

echo "=== Claude Code Game Studios — Session Context ==="

if [ -n "$BRANCH" ]; then
    echo "Branch: $BRANCH"

    # Recent commits
    echo ""
    echo "Recent commits:"
    git log --oneline -5 2>/dev/null | while read -r line; do
        echo "  $line"
    done
fi

# Current sprint (find most recent sprint file)
LATEST_SPRINT=$(ls -t production/sprints/sprint-*.md 2>/dev/null | head -1)
if [ -n "$LATEST_SPRINT" ]; then
    echo ""
    echo "Active sprint: $(basename "$LATEST_SPRINT" .md)"
fi

# Current milestone
LATEST_MILESTONE=$(ls -t production/milestones/*.md 2>/dev/null | head -1)
if [ -n "$LATEST_MILESTONE" ]; then
    echo "Active milestone: $(basename "$LATEST_MILESTONE" .md)"
fi

# Open bug count
BUG_COUNT=0
for dir in tests/playtest production; do
    if [ -d "$dir" ]; then
        count=$(find "$dir" -name "BUG-*.md" 2>/dev/null | wc -l)
        BUG_COUNT=$((BUG_COUNT + count))
    fi
done
if [ "$BUG_COUNT" -gt 0 ]; then
    echo "Open bugs: $BUG_COUNT"
fi

# Code health quick check
if [ -d "src" ]; then
    TODO_COUNT=$(grep -r "TODO" src/ 2>/dev/null | wc -l)
    FIXME_COUNT=$(grep -r "FIXME" src/ 2>/dev/null | wc -l)
    if [ "$TODO_COUNT" -gt 0 ] || [ "$FIXME_COUNT" -gt 0 ]; then
        echo ""
        echo "Code health: ${TODO_COUNT} TODOs, ${FIXME_COUNT} FIXMEs in src/"
    fi
fi

# --- Active session state recovery ---
STATE_FILE="production/session-state/active.md"
if [ -f "$STATE_FILE" ]; then
    echo ""
    echo "=== ACTIVE SESSION STATE DETECTED ==="
    echo "A previous session left state at: $STATE_FILE"
    echo "Read this file to recover context and continue where you left off."
    echo ""
    echo "Quick summary:"
    head -20 "$STATE_FILE" 2>/dev/null
    TOTAL_LINES=$(wc -l < "$STATE_FILE" 2>/dev/null)
    if [ "$TOTAL_LINES" -gt 20 ]; then
        echo "  ... ($TOTAL_LINES total lines — read the full file to continue)"
    fi
    echo "=== END SESSION STATE PREVIEW ==="
fi

# --- VKL reminder ---
echo ""
echo "VKL: 검증 필요 시 /verify 호출 (원칙은 .claude/docs/vkl-bridge.md 로드됨)"

echo "==================================="
exit 0
