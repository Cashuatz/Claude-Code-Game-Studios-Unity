# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity
- **Version**: 6.x LTS (프로젝트 시작 시 /setup-engine으로 확정)
- **Language**: C#
- **Rendering**: [TO BE CONFIGURED — URP/HDRP/Built-in]
- **Physics**: [TO BE CONFIGURED — Unity Physics/Havok]
- **Build System**: Unity Build Pipeline

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: [TO BE CONFIGURED — e.g., PC, Console, Mobile, Web]
- **Input Methods**: [TO BE CONFIGURED — e.g., Keyboard/Mouse, Gamepad, Touch, Mixed]
- **Primary Input**: [TO BE CONFIGURED — the dominant input for this game]
- **Gamepad Support**: [TO BE CONFIGURED — Full / Partial / None]
- **Touch Support**: [TO BE CONFIGURED — Full / Partial / None]
- **Platform Notes**: [TO BE CONFIGURED — any platform-specific UX constraints]

## Naming Conventions

- **Classes/Methods**: PascalCase
- **Local Variables**: camelCase
- **Private Fields**: _camelCase
- **Constants**: UPPER_SNAKE_CASE
- **Namespaces**: CompanyName.ProjectName.Module
- **Files**: Match class name (MyClass.cs)
- **Scenes/Prefabs**: PascalCase with category prefix (UI_MainMenu, Level_Tutorial)

## Performance Budgets

- **Target Framerate**: [TO BE CONFIGURED]
- **Frame Budget**: [TO BE CONFIGURED]
- **Draw Calls**: [TO BE CONFIGURED]
- **Memory Ceiling**: [TO BE CONFIGURED]

## Testing

- **Framework**: Unity Test Framework (NUnit)
- **Minimum Coverage**: [TO BE CONFIGURED]
- **Required Tests**: Balance formulas, gameplay systems, networking (if applicable)

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- [None configured yet — add as architectural decisions are made]

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one]

## Unity Specialist Routing

<!-- Engine specialists are automatically routed based on file patterns. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (MonoBehaviour, ScriptableObject)
- **DOTS/ECS Specialist**: unity-dots-specialist (Jobs, Burst, ECS)
- **Shader Specialist**: unity-shader-specialist (Shader Graph, VFX Graph, URP/HDRP)
- **UI Specialist**: unity-ui-specialist (UI Toolkit, UGUI, UXML/USS)
- **Asset Specialist**: unity-addressables-specialist (Addressable Groups, async loading)

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| `*.cs` (general C#) | unity-specialist |
| `*.shader`, `*.hlsl`, `*.shadergraph` | unity-shader-specialist |
| `*DOTS*`, `*ECS*`, `*ISystem*`, `*IJobEntity*` | unity-dots-specialist |
| `*Addressable*`, `*AssetBundle*`, `*AssetReference*` | unity-addressables-specialist |
| `**/UI/**`, `*.uxml`, `*.uss`, `*Canvas*`, `*UIDocument*` | unity-ui-specialist |
| `*.prefab`, `*.unity` (scenes) | unity-specialist |
| Architecture review | technical-director |
