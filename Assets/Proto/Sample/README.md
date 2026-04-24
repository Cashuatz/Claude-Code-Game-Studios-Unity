# Assets/Proto/Sample/

이 폴더의 각 하위 디렉토리는 **이 저장소가 실제로 만든 프로토타입 실사례**다.
Proto 패키지 수강생은 자유롭게 **열어보기 / 학습하기 / 덮어쓰기 / 삭제하기** 해도 된다.

기본 Proto 패키지 (`Assets/Proto/Runtime/`, `Assets/Proto/Editor/`, `Assets/Proto/Prefabs/`, `Assets/Proto/Materials/`, `Assets/Proto/VFX/`) 는 **장르 무관 재사용 가능한 모듈** 만 포함한다. 여기 Sample 들은 그 모듈들이 실제 게임에서 어떻게 엮이는지 보여주는 참고 예시다.

## 샘플 목록

| 샘플 | 주제 | 핵심 포인트 |
|---|---|---|
| `Phase0_Launcher/` | 3장르(턴제·TD·레일슈팅) 공통 런처 + 씬 전환 싱글톤 | UGUI `Button` → `SceneFlow.LoadGenre` → 각 장르 씬으로 전환. Additive 로딩 / DontDestroyOnLoad 패턴 실사례. |
| `TD_CityWfc/` | 타워 디펜스 맵 — 시가지 PCG + WFC 타일 솔버 + A\* 경로 + JSON 레벨 직렬화 + EditMode 테스트 11건 | 공용 `Runtime/Stage/PCG/Cityscape*` 와 `Runtime/Stage/Wfc/*` 모듈을 TD 규약에 맞게 엮은 실사례. `Proto.Tests.Editor` asmdef 가 여기 포함. |
| `BushDemo/` | 풀밭 빌보드 렌더 — 공용 `Runtime/Environment/Bush/BushBillboard.cs` + `VFX/Environment/Shaders/ProtoEnv_BushBillboard.shader` 를 쓰는 최소 데모 씬 | `BushDemo.cs` 가 샘플 로직. 셰이더·컴포넌트 자체는 공용이라 다른 게임에서도 재사용 가능. |
| `SpeechBubbleDemo/` | 캐릭터 말풍선 — 공용 `Runtime/UI/SpeechBubble/*` 를 쓰는 최소 데모 | 9-slice + 타이핑 + 빌보드 옵션 확인용 씬. |
| `ShaderFxDemo/` | 셰이더 FX 레시피 허브 — 공용 `Runtime/Shared/ShaderFx/{ShaderFxHub, ShaderFxRecipe, Easings}` + `VFX/Shared/Shaders/*` 6종 (Dissolve/HitFlash/Hologram/Outline/RimLight/Scanline) 데모 | 레시피 조합 실험장. |

## 사용 방법

1. 관심 있는 샘플 디렉토리를 연다.
2. 해당 `Scenes/<씬>.unity` 를 Unity 에서 열고 Play 버튼.
3. 코드 (`Runtime/<샘플>/*.cs`) 를 읽거나 덮어써서 내 게임에 맞게 고친다.
4. 필요 없으면 샘플 디렉토리 통째로 삭제해도 패키지 기본 동작엔 영향 없음.

## Build Settings

이 패키지의 **기본 Build Scenes 리스트는 비어 있다**. 각 샘플 씬은 Build 에 포함하고 싶으면 `File → Build Profiles → Scene List` 에서 직접 추가한다.

## 과거 작업 참고

이 샘플들이 만들어지기까지의 검증 기록 (VKL case logs · observations · oracle runs · hypotheses · proposals) 은 `.vkl/archive/2026-04-24-proto-package-finalization/` 아래에 append-only 로 보존돼 있다. 특정 기법이 왜 이렇게 구현됐는지 배경이 궁금하면 그 아카이브의 `README.md` 와 `INDEX.md` 를 먼저 읽는다.
