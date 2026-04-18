# Technical Preferences — Proto 브랜치 (프로토타이핑 경량 모드)

<!-- Proto 전용: URP 3D 프로토타이핑 기본값. main 브랜치의 정식 기술 선호도와 다름. -->

## Engine & Language

- **Engine**: Unity 6.x LTS
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline)
- **Project Type**: 3D
- **Physics**: Unity built-in (3D)
- **Input**: Unity Input System (신규)
- **Build System**: Unity Build Pipeline

## Input & Platform

- **Target Platforms**: PC (Windows standalone + WebGL 옵션)
- **Input Methods**: Keyboard/Mouse
- **Primary Input**: Keyboard (WASD + Space + Mouse)
- **Gamepad Support**: None (Proto 범위 밖)
- **Touch Support**: None (Proto 범위 밖)
- **Platform Notes**: 프로토타이핑 전용. 플랫폼 최적화는 정식 개발에서.

## Naming Conventions

- **Classes/Methods**: PascalCase
- **Local Variables**: camelCase
- **Private Fields**: _camelCase
- **Constants**: UPPER_SNAKE_CASE
- **Namespaces**: `Proto.<Feature>` (예: `Proto.Player`, `Proto.Enemy`, `Proto.UI`)
- **Files**: 클래스명과 일치 (예: `PlayerController.cs`)

## Performance Budgets (Proto 느슨)

- **Target Framerate**: 60fps (가능하면)
- **Frame Budget**: 16.6ms (엄격 강제 안 함)
- **Draw Calls**: 측정만, 제한 없음
- **Memory Ceiling**: 측정만, 제한 없음

> Proto는 빠른 이터레이션이 우선. 성능 튜닝은 정식 개발 단계에서.

## Testing

- **Framework**: Unity Test Framework (NUnit) — 선택
- **Minimum Coverage**: 없음 (Proto에서는 테스트 선택)
- **Required Tests**: 없음 — 수동 Play 테스트로 충분

## Forbidden Patterns (Proto 완화)

- ❌ `main` 브랜치 직접 수정 (Proto 작업은 반드시 Proto 브랜치에)
- ❌ `Assets/` 밖에 Unity 관련 파일 생성
- ❌ 네임스페이스 없는 클래스

## Allowed Libraries

- URP 17.3+ (이미 포함)
- Unity Input System 1.19+ (이미 포함)
- AI Navigation 2.0+ (이미 포함)
- Unity MCP (`com.coplaydev.unity-mcp` — 이미 포함)

## Proto 전용 Unity 경로

| 용도 | 경로 |
|------|------|
| 게임 스크립트 | `Assets/Scripts/Proto/<Feature>/` |
| 프리팹 | `Assets/Prefabs/Proto/` |
| ScriptableObject | `Assets/ScriptableObjects/Proto/` |
| 씬 | `Assets/Scenes/` (SampleScene 재사용) |
| 리소스 | `Assets/Resources/` (필요 시) |

## Unity Specialist Routing (Proto)

Proto에서는 단순한 스크립트 작성이 주 작업. 특수 영역은 대부분 범위 밖.

- **Primary**: unity-specialist (일반 MonoBehaviour)
- **Gameplay**: gameplay-programmer (플레이어, 적, 아이템 스크립트)
- **UI**: ui-programmer (HUD, 메뉴)
- **Art 상담**: art-director

### 비활성 Specialists (Proto 범위 밖 — `_hidden/`)

- `unity-dots-specialist` — DOTS/ECS는 프로토타이핑에 과함
- `unity-shader-specialist` — 셰이더는 URP 기본 머티리얼 사용
- `unity-addressables-specialist` — 리소스는 `Resources/` 또는 직접 참조
- `unity-ui-specialist` — 기본 uGUI 또는 UI Toolkit 기본만

### File Extension Routing

| 파일 종류 | 담당 에이전트 |
|---|---|
| `Assets/Scripts/Proto/**/*.cs` | gameplay-programmer 또는 unity-specialist |
| `Assets/Scripts/Proto/UI/**/*.cs` | ui-programmer |
| `Assets/Prefabs/Proto/**` | unity-specialist (또는 Unity MCP 직접) |
| 런타임 에러·로그 | omc-debugger |
| 코드 탐색 | omc-explore |
