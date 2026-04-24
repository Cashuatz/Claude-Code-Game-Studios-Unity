# SpeechBubbleDemo — 말풍선 UI 데모

공용 `Runtime/UI/SpeechBubble/*` (9-slice + 타이핑 애니 + 월드 공간 빌보드) 를 실제 캐릭터 오브젝트 위에 붙여 보는 최소 데모 씬.

## 구성

```
SpeechBubbleDemo/
├── Runtime/SpeechBubbleDemo.cs     — 캐릭터 오브젝트에 SpeechBubble 부착 + 대사 이벤트 순차 재생
├── Scenes/SpeechBubbleDemo.unity   — 3D 바닥 + 큐브 캐릭터 + 말풍선 UGUI
└── README.md
```

## 공용 모듈과의 관계

- `Runtime/UI/SpeechBubble/SpeechBubble.cs` (공용) — 말풍선 본체 컴포넌트. Show/Hide/Typing.
- `Runtime/UI/SpeechBubble/SpeechBubbleEnums.cs` (공용) — Position·Tail·Anim enums.
- `Runtime/UI/SpeechBubble/SpeechBubbleSpriteFactory.cs` (공용) — 9-slice sprite 런타임 생성 factory.
- 이 Sample 의 `SpeechBubbleDemo.cs` 는 대사 큐를 시간 순서로 흘려보내며 모듈 API 를 테스트.

## 실행 방법

1. `SpeechBubbleDemo.unity` 오픈 → Play.
2. 자동으로 3가지 대사가 순차 재생된다 (짧은 대사 / 긴 대사 / 자동 타이핑).
3. Scene View 에서 캐릭터를 움직여보며 빌보드 추적 + 화면 밖으로 나갈 때 tail clamping 확인.

## 활용 포인트

- 캐릭터 대사/튜토리얼 힌트 UI 의 API 모양 확인.
- 반응형 크기 (텍스트 길이 기반) / tail 방향 자동 선택 / 말랑 애니메이션 체감.
- 게임 본체에서는 이 데모 스크립트를 버리고 공용 `SpeechBubble.Show(...)` 만 호출하면 충분.
