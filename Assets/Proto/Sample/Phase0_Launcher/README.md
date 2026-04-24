# Phase0_Launcher — 3장르 공통 런처 실사례

Proto 브랜치가 쓰리 장르 (턴제 3D / 타워 디펜스 / 레일슈팅) 를 한 프로젝트 안에서 동시에 시연할 때 사용했던 **공통 진입 씬 + 장르 전환 싱글톤** 샘플.

## 구성

```
Phase0_Launcher/
├── Runtime/Launcher/
│   ├── LauncherUI.cs         — UGUI Canvas 3 버튼. 선택 시 SceneFlow.LoadGenre.
│   └── SceneFlow.cs          — DontDestroyOnLoad 싱글톤. 장르 씬 Additive 로드 + 이전 씬 Unload.
├── Scenes/
│   ├── Proto_Launcher.unity  — 3 버튼 런처 씬 (첫 진입)
│   ├── Proto_Turn3d.unity    — 턴제 3D 껍데기 씬
│   └── Proto_RailShooter.unity — 1인칭 레일슈팅 껍데기 씬
└── README.md
```

> Proto_TD.unity (타워 디펜스 껍데기) 는 `Sample/TD_CityWfc/Scenes/Proto_TD.unity` 로 분리되어 있다. TD_CityWfc Sample 을 먼저 import 해 두면 런처에서 TD 버튼도 정상 동작한다.

## 핵심 기법

- **UGUI + Input System UI Module** — `EventSystem` 에 Input System UI Module 이 붙어 있어야 UGUI `Button` 이 신규 입력 파이프라인에서 눌린다. Proto_Launcher 씬이 이미 그렇게 구성됨.
- **DontDestroyOnLoad 싱글톤** — `SceneFlow` 가 런처 진입 시 자기 자신을 DontDestroyOnLoad 로 표기. 장르 씬 로드/언로드 반복 시에도 유일 인스턴스 유지.
- **Additive 씬 로딩** — `LoadSceneMode.Additive` 로 장르 씬을 더한 뒤, 런처 씬을 언로드. 반대로 장르에서 "돌아가기" 요청 시 런처 씬을 Additive 로드 → 장르 씬 언로드.

## 사용 시 주의

- 장르 씬은 "껍데기" 수준으로 비어 있다 (Camera + Light 정도). 실제 게임플레이는 수강생이 덮어써서 구현.
- `SceneFlow` 가 장르별 씬 경로를 **문자열 상수**로 들고 있다. 씬을 옮기거나 이름을 바꾸면 `SceneFlow.cs` 상수를 같이 고친다.
- Build Settings 에 이 씬들이 들어있지 않으면 런처가 `SceneManager.LoadScene` 호출 시 실패한다. 샘플을 쓰려면 3개 씬을 Build Settings 에 직접 추가.
