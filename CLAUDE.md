# Claude Code Game Studios -- Unity Game Studio Agent Architecture

Unity game development managed through coordinated Claude Code subagents.
Each agent owns a specific domain, enforcing separation of concerns and quality.

## Technology Stack

- **Engine**: Unity
- **Language**: C#
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Pipeline
- **Asset Pipeline**: Unity Asset Pipeline + Addressables

> **Note**: This is a Unity-specialized fork. Unity engine agents and sub-specialists
> are available for MonoBehaviour, DOTS/ECS, Shaders, Addressables, and UI Toolkit.

## Project Structure

@.claude/docs/directory-structure.md

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coordination Rules

@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction

See `docs/COLLABORATIVE-DESIGN-PRINCIPLE.md` for full protocol and examples.

> **First session?** If the project has no engine configured and no game concept,
> run `/start` to begin the guided onboarding flow.

## 검증 원칙 (VKL)

@.claude/docs/vkl-bridge.md

## Coding Standards

@.claude/docs/coding-standards.md

## 실행 모드

@.claude/docs/execution-modes.md

## Context Management

@.claude/docs/context-management.md
