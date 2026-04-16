# Agent Coordination Rules

1. **Vertical Delegation**: Leadership agents delegate to department leads, who
   delegate to specialists. Never skip a tier for complex decisions.
2. **Horizontal Consultation**: Agents at the same tier may consult each other
   but must not make binding decisions outside their domain.
3. **Conflict Resolution**: When two agents disagree, escalate to the shared
   parent. If no shared parent, escalate to `creative-director` for design
   conflicts or `technical-director` for technical conflicts.
4. **Change Propagation**: When a design change affects multiple domains, the
   `producer` agent coordinates the propagation.
5. **No Unilateral Cross-Domain Changes**: An agent must never modify files
   outside its designated directories without explicit delegation.

## Model Tier Assignment

Skills and agents are assigned to model tiers based on task complexity:

| Tier | Model | When to use |
|------|-------|-------------|
| **Haiku** | `claude-haiku-4-5-20251001` | Read-only status checks, formatting, simple lookups — no creative judgment needed |
| **Sonnet** | `claude-sonnet-4-6` | Implementation, design authoring, analysis of individual systems — default for most work |
| **Opus** | `claude-opus-4-6` | Multi-document synthesis, high-stakes phase gate verdicts, cross-system holistic review |

Skills with `model: haiku`: `/help`, `/sprint-status`, `/story-readiness`, `/scope-check`,
`/project-stage-detect`, `/changelog`, `/patch-notes`, `/onboard`

Skills with `model: opus`: `/review-all-gdds`, `/architecture-review`, `/gate-check`

All other skills default to Sonnet. When creating new skills, assign Haiku if the
skill only reads and formats; assign Opus if it must synthesize 5+ documents with
high-stakes output; otherwise leave unset (Sonnet).

## Subagents vs Agent Teams

This project uses two distinct multi-agent patterns:

### Subagents (current, always active)
Spawned via `Task` within a single Claude Code session. Used by all `team-*` skills
and orchestration skills. Subagents share the session's permission context, run
sequentially or in parallel within the session, and return results to the parent.

**When to spawn in parallel**: If two subagents' inputs are independent (neither
needs the other's output to begin), spawn both Task calls simultaneously rather
than waiting. Example: `/review-all-gdds` Phase 1 (consistency) and Phase 2
(design theory) are independent — spawn both at the same time.

### Agent Teams (experimental — opt-in)
Multiple independent Claude Code *sessions* running simultaneously, coordinated
via a shared task list. Each session has its own context window and token budget.
Requires `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` environment variable.

**Use agent teams when**:
- Work spans multiple subsystems that will not touch the same files
- Each workstream would take >30 minutes and benefits from true parallelism
- A senior agent (technical-director, producer) needs to coordinate 3+ specialist
  sessions working on different epics simultaneously

**Do not use agent teams when**:
- One session's output is required as input for another (use sequential subagents)
- The task fits in a single session's context (use subagents instead)
- Cost is a concern — each team member burns tokens independently

**Current status**: Not yet used in this project. Document usage here when first adopted.

## Parallel Task Protocol

When an orchestration skill spawns multiple independent agents:

1. Issue all independent Task calls before waiting for any result
2. Collect all results before proceeding to dependent phases
3. If any agent is BLOCKED, surface it immediately — do not silently skip
4. Always produce a partial report if some agents complete and others block

## OMC 보조 에이전트 라우팅

| 작업 유형 | 에이전트 | 모델 | 사용 시점 |
|-----------|---------|------|----------|
| 빠른 코드 탐색 | omc-explore | haiku | 파일/패턴 검색, 코드베이스 정찰 |
| 런타임 디버깅 | omc-debugger | sonnet | 버그 추적, 스택트레이스 분석, 빌드 에러 해결 |
| 구현 검증 | omc-verifier | sonnet | 수락 기준 체크, 증거 기반 완료 검증 |
| 교차 검증 | /codex | external | 중요 결정 2차 검증 (Codex CLI) |

## MCP 도구 라우팅

Unity MCP 도구는 다음 CCGS 에이전트가 사용할 수 있습니다:

| MCP 도구 카테고리 | 사용 가능한 CCGS 에이전트 |
|------------------|-------------------------|
| manage_gameobject, manage_components | unity-specialist, gameplay-programmer |
| manage_scene | unity-specialist, level-designer, qa-tester |
| manage_script, validate_script | 모든 programmer 에이전트 |
| manage_material, manage_shader | unity-shader-specialist, technical-artist |
| manage_animation | gameplay-programmer, technical-artist |
| manage_physics | gameplay-programmer, engine-programmer |
| manage_ui | unity-ui-specialist, ui-programmer |
| manage_build, manage_packages | engine-programmer, devops-engineer |
| manage_camera | gameplay-programmer, level-designer |
| manage_vfx | technical-artist |
| run_tests, read_console | qa-tester, performance-analyst |
| execute_code | 모든 에이전트 (범용) |
