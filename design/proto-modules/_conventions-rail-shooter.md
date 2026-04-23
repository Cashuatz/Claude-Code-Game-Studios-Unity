# Proto Rail-Shooter — Conventions & Hard Rules

> 1인칭 건슈팅 (하우스 오브 더 데드류 온-레일 슈터) 프로토 전용 규칙.
> 공통 규칙은 `_conventions.md` (HR-1 ~ HR-11) 를 **그대로 상속**.
> 이 문서는 Rail Shooter 장르에서만 추가되는 Hard Rule (HR-RS-XX) 정의.
> 수강생 답변과 무관하게 **고정**되는 룰 (분기 없음).

## 상속되는 공통 Hard Rules

`_conventions.md` 의 다음 규칙은 Rail Shooter 에도 그대로 적용:

- HR-1. Rigidbody 전면 금지 (히트 판정은 Raycast / OverlapSphere 만)
- HR-2. 단일 씬
- HR-4. 카메라 전환은 이벤트 기반
- HR-5. `Time.timeScale` 직접 조작 금지 (불릿타임은 `Enemy.LocalTimeScale`)
- HR-6. 액션 입력은 CombatScheme 경유 — **Rail Shooter 전용 변형 CombatScheme 제공** (`RailShooterScheme`)
- HR-7. CharacterRenderKit (적 렌더)
- HR-8. 데이터-코드 분리
- HR-9. 이벤트 버스
- HR-10. 에디터-런타임 경계
- HR-11. UI 런타임은 uGUI
- (제안) HR-12. Shader Graph 강제 — 장르 내에서 **하드 적용**.
- (제안) HR-13. PP 는 `PostProcessController` 경유 — 장르 내에서 **하드 적용**.

**적용되지 않는 공통 규칙**:

- HR-3. PCG 레이아웃 — Rail Shooter 는 섹션 기반 사전 제작 레벨 사용.

## Rail Shooter 전용 Hard Rules

### HR-RS-1. 플레이어 이동은 카메라 레일 또는 슬롯 점프만

- 플레이어는 **월드를 자유 이동하지 않는다**. WASD 식 walking 입력 금지.
- Q1 선택에 따라 허용되는 이동:
  - **A (완전 고정)**: 카메라 이동 없음. 플레이어 위치 불변.
  - **B (웨이포인트 자동 진행)**: 카메라가 `RailPath` 위를 자동 슬라이드. 속도/정지는 사전 정의.
  - **C (슬롯 이동)**: 지정된 `Slot` 간 점프 전환 (트리거 기반). 슬롯 간 이동 중에는 조준 불가.
- 금지:
  - `CharacterController.Move(...)` 금지.
  - `NavMeshAgent` 금지.
  - `Rigidbody.velocity` 금지 (HR-1 연장).
  - **`Camera.transform` 직접 이동 금지** — 반드시 `RailCameraDriver` 경유.

### HR-RS-2. 엄폐 상태는 플래그 + 이벤트 모델

- 플레이어의 엄폐 상태는 `PlayerCoverState` (단일 ScriptableObject 인스턴스) 가 소유.
- 상태 전이: `Uncovered` ↔ `Covering`.
- 전이 트리거는 **입력 핸들러만** 호출 가능 (`RailCoverInput`).
- 엄폐 중 피격 확률 = 0 (기본). 머리 내밀기(엿보기) 구현 시 별도 `Peeking` 서브 상태.
- 엄폐 중 적 AI 는 플레이어를 "놓침" — 계속 쏘지 않고 다른 적으로 주의 분산.
- 엄폐 입력 금지 타이밍: 슬롯 전환 중 (HR-RS-1 C), 레벨 전환 컷신 중.

### HR-RS-3. 불릿타임은 LocalTimeScale + PP 프로파일 블렌드

- 불릿타임 활성화 시:
  - 모든 적의 `LocalTimeScale = bulletTimeSlowFactor` (기본 0.3).
  - 플레이어 입력 타임스케일은 1.0 유지.
  - `PostProcessController.SetWeight("pp-bullettime", 1.0, blendIn)` 호출.
- 비활성화 시:
  - `LocalTimeScale = 1.0`, `SetWeight("pp-bullettime", 0.0, blendOut)`.
- 게이지/쿨다운은 `BulletTimeGauge` 가 단일 진입점. 다른 스크립트가 직접 값 조작 금지.
- `Time.timeScale` 수정 절대 금지 (공통 HR-5).

### HR-RS-4. 에임과 카메라 회전 분리

- **크로스헤어는 스크린 공간에서 이동**, 카메라 transform 은 회전하지 않는다 (기본).
- `RailAimController` 가 입력 → 크로스헤어 UI 좌표 → 월드 Ray 변환.
- 사격 시 `Physics.Raycast` 로 대상 결정. 카메라 흔들림은 애니메이션/셰이크 효과만 (이동/회전 불변식 유지).
- 예외: Q1 B 에서 **웨이포인트 전환 시 카메라 yaw 변경** 허용 (경로 곡선 추종). 입력과 무관.

### HR-RS-5. 적은 파동(wave) 단위 스폰 + 사전 스크립트

- 적 등장은 레벨 데이터에 정의된 `SpawnEvent` 시퀀스를 따른다.
- `SpawnEvent` 필드: `triggerTime` (섹션 진입 후 초 단위), `enemyType`, `spawnPointId`, `behaviorProfile`.
- 동적 스폰(플레이어 행동 기반 난이도 조절) 은 프로토 범위 밖.
- 적 AI 는 **상태 머신 3상태**로 제한: `Approach` / `Attack` / `Dead`. 복잡한 플래닝 금지.

### HR-RS-6. 섹션은 선형 체인 (분기 금지)

- 레벨 = 섹션의 선형 배열 (`Section[0] → Section[1] → ... → Section[N]`).
- 분기 / 선택 경로 / 백트래킹 금지 (프로토 범위).
- 섹션 전환 트리거: 모든 스폰 완료 + 모든 적 사망 + 5초 대기 (조정 가능).
- 전환 중에는 플레이어 입력 비활성, `timeline-cue` 연출 재생, `shader-fx-kit` 디졸브 트랜지션.

### HR-RS-7. 조준점 UI 는 uGUI 절대 좌표

- 크로스헤어는 `Canvas` (Screen Space - Overlay) 의 `RectTransform` 으로 구현.
- 감도/데드존 등은 `AimConfig.asset` SO.
- 크로스헤어 월드 Ray 는 `Camera.ScreenPointToRay` 로 매 프레임 계산.
- 게임패드/마우스/터치 추상화는 `InputSystem` 사용 (공통 — 프로젝트 전체).

### HR-RS-8. HP 는 섹션 단위 + 체크포인트 리로드

- 플레이어 HP 는 `PlayerHP.asset` 의 `maxHP` (기본 100).
- HP 0 → **섹션 재시작** (체크포인트 저장되었으면 해당 섹션부터, 없으면 레벨 처음).
- 레벨 전체 실패 = 레벨 재시작.
- 무한 컨티뉴 허용 (점수 없는 경우). Q6 C (점수 기반) 에서는 컨티뉴 시 점수 리셋.

### HR-RS-9. 불릿타임·엄폐·재장전 입력 전용 핸들러

- 세 입력은 **각각 전용 Input Action** 에 바인딩:
  - `RailShoot` (기본: 마우스 좌클릭 / 트리거)
  - `RailReload` (R / X)
  - `RailCover` (Space / A)
  - `RailBulletTime` (Shift / LB)
  - `RailWeaponSwap` (Q 또는 휠 / RB) — Q4 B 선택 시만 바인딩
- 바인딩 변경은 `InputOverridesConfig.asset` 로만. 하드코드 금지.

### HR-RS-10. 결정론은 권장, 필수 아님

- TD 장르와 달리 결정론 bit-exact 는 필수 아님.
- 그러나 **적 스폰 패턴 재현성** 을 위해 seeded RNG 는 권장:
  - `SpawnEvent` 내 랜덤 타이밍/위치가 있으면 `Proto.Shared.Rng` (seeded) 사용.
  - 스폰 난수는 씬 로드 시 결정 — 플레이어 입력과 무관하게 재현 가능.
- `System.Random` 대신 공통 시드 RNG 권장 (강제 아님).

## 파일·폴더 규칙 (Rail Shooter 전용 추가)

```
Assets/Proto/
├── Runtime/
│   └── RailShooter/
│       ├── Camera/          ← RailCameraDriver, WaypointSlider, SlotManager
│       ├── Aim/             ← RailAimController, Crosshair, AimConfig
│       ├── Weapon/          ← Weapon, WeaponSlot, Ammo, Reload
│       ├── Cover/           ← PlayerCoverState, RailCoverInput
│       ├── BulletTime/      ← BulletTimeGauge, BulletTimeController
│       ├── Enemy/           ← EnemyCore, EnemyFSM, EnemyPack.*
│       ├── Spawn/           ← SpawnEvent, SpawnScheduler
│       ├── Section/         ← Section, SectionChain, SectionTransition
│       ├── Rule/            ← RuleSection, RuleBoss, RuleScore, BossCore
│       ├── HP/              ← PlayerHP, HPEvents
│       └── HUD/             ← HUDCrosshair, HUDAmmo, HUDBT, HUDScore, HUDBossHP, HUDProgress
├── Data/
│   └── RailShooter/
│       ├── Levels/          ← Level_<name>.asset
│       ├── Sections/        ← Section_<name>.asset
│       ├── Weapons/         ← Weapon_<name>.asset
│       ├── EnemyPacks/      ← EnemyPack_<theme>.asset
│       ├── Spawns/          ← Spawn_<section>.asset
│       ├── Rails/           ← RailPath_<name>.asset (Q1-B)
│       └── Slots/           ← SlotSet_<name>.asset (Q1-C / Q2-C)
└── VFX/
    └── RailShooter/         ← pp-bullettime, pp-horror, pp-cyber, pp-weird 프로파일
```

## 네이밍 (Rail Shooter 전용)

- 네임스페이스: `Proto.RailShooter.<Kind>` (예: `Proto.RailShooter.Camera.RailCameraDriver`)
- SO 파일: `RS_<Kind>_<Name>.asset` (예: `RS_Weapon_Pistol.asset`)
- 프리팹: `RS_<Kind>_<Name>.prefab` (예: `RS_Enemy_ZombieBasic.prefab`)
- 섹션 에셋: `RS_Section_<level>_<index>.asset`

## 검증 공통 (Rail Shooter 추가)

공통 검증(컴파일/씬/프리팹/런타임/스크린샷) 에 더해:

1. **입력 바인딩 무결성**: `RailShoot` / `RailCover` / `RailBulletTime` / `RailReload` 모두 Input Action Asset 에 존재.
2. **레일/슬롯 데이터**: Q1 선택에 해당하는 `RailPath` 또는 `SlotSet` 에셋이 `Data/RailShooter/Rails|Slots/` 에 존재.
3. **섹션 체인 무결성**: `Level.sections[i].next == Level.sections[i+1].id` 체크, 마지막 섹션은 `next == null`.
4. **PP 프로파일**: 불릿타임 ON 이면 `pp-bullettime.asset` 존재, 테마별 PP 프로파일(`pp-horror` 등) 도 선택 테마에 따라 존재.
5. **카메라 불변식**: Play 후 5초간 카메라 회전 벡터 변화량 < 임계값 (HR-RS-4). Q1 B 경로 곡선 추종은 예외 허용.

실패 시 롤백 + 사유 보고 (1문장).

## 메타 규칙

- 본 문서는 수강생 답변과 무관하게 고정.
- 수정 시 `.vkl/proposals/` 에 proposal 추가 후 인간 검토.
- 각 HR-RS 에 VKL oracle ID 부여 필요 (TBD).
