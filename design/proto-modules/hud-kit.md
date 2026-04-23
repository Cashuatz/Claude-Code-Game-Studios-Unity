# Module: hud-kit

> 참고: [`_genres-index.md`](./_genres-index.md) / [`_conventions.md`](./_conventions.md)
>
> **장르 적용**: turn3d ✅ / td ✅ / rail-shooter ✅ (전 장르 공용)

## 1. Purpose

런타임 HUD 의 **공통 프레임워크**. 각 장르가 개별 위젯(HP바·타이머·크로스헤어 등)을
만들 때 이 모듈의 Canvas 셋업·위젯 등록·레이아웃 슬롯·이벤트 바인딩 규약을 따른다.

**왜 공유 모듈인가**: 장르마다 위젯은 다르지만 **Canvas 구조, 안전 영역, 레이아웃 슬롯, 이벤트 기반 값 바인딩** 은 동일. 중복 구현 금지.

## 2. Hard Rules

- 공통 **HR-11** 준수: 런타임 UI 는 반드시 uGUI (Canvas + RectTransform). `UIDocument` 금지.
- `EventSystem` 1개만 유지. 설치 시 없으면 자동 생성.
- Canvas 는 기본 **Screen Space - Overlay**. 월드 공간 UI (체력 바 등) 는 별도 하위 Canvas 로 분리.
- 위젯 → 데이터 바인딩은 **이벤트 버스 경유** (HR-9). 위젯이 게임 상태 오브젝트를 직접 참조하지 않는다.
- 모든 텍스트는 `TextMeshProUGUI`. `Text` (legacy) 금지.
- 해상도 독립: `CanvasScaler` = `Scale With Screen Size`, ref=1920x1080, match=0.5.

## 3. Public API

```csharp
namespace Proto.Shared.HUD
{
    // 위젯 루트 추상
    public interface IHudWidget
    {
        string WidgetId { get; }
        HudSlot TargetSlot { get; }
        void Bind(HudContext ctx);
        void Unbind();
    }

    // 슬롯: 화면 안전 영역에 사전 정의된 위치
    public enum HudSlot
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight,
        FullScreen
    }

    // 컨텍스트: 장르마다 다른 이벤트 채널 묶음
    public sealed class HudContext
    {
        public GameEventChannel Events { get; }
        public IReadOnlyDictionary<string, object> State { get; }
    }

    // 허브: 위젯 등록/해제
    public static class HudHub
    {
        public static void Register(IHudWidget widget);
        public static void Unregister(string widgetId);
        public static T Get<T>(string widgetId) where T : class, IHudWidget;
        public static event Action<IHudWidget> OnWidgetRegistered;
    }

    // 이벤트: 값 바인딩
    public static class HudEvents
    {
        public static readonly GameEventChannel<float> OnFloatChanged;   // HP%, 게이지 등
        public static readonly GameEventChannel<int> OnIntChanged;       // 탄수, 점수 등
        public static readonly GameEventChannel<string> OnTextChanged;   // 상태 텍스트
        public static readonly GameEventChannel<bool> OnToggleChanged;   // 엄폐/불릿타임 on-off
    }
}
```

## 4. Dependencies

- **Required**: 공통 `Proto.Shared.Events` (GameEventChannel SO 기반).
- **Optional**: `sound-kit` (버튼 클릭 사운드), `shader-fx-kit` (위젯 등장/퇴장 디졸브).

## 5. Default Prefabs / Assets

- `HUD_Root.prefab` — Canvas + CanvasScaler + EventSystem + 9 슬롯 빈 컨테이너 포함.
- `HUD_SafeArea.prefab` — 노치/디스플레이 컷아웃 대응 RectTransform 스크립트 포함.
- `HUD_Widget_Template.prefab` — 신규 위젯 제작 시 복제용 기본.
- `HudStyle_Default.asset` — 공통 폰트/색상/여백 설정 SO.

## 6. Skill Hook

`/proto-hud-kit` (인자 없음):

1. `Assets/Proto/Runtime/Shared/HUD/` 에 스크립트 복사.
2. `Assets/Proto/Prefabs/Shared/HUD_Root.prefab` 생성 + 9 슬롯 기본 레이아웃.
3. 씬 `ProtoMain.unity` 에 `HUD_Root` 인스턴스 배치, `EventSystem` 없으면 생성.
4. `HudEvents` SO 파일 생성 (`Data/Shared/HudEvents_*.asset`).
5. `HudStyle_Default.asset` 생성.
6. 검증: Play 진입 후 에러 0, Canvas + EventSystem 존재.

## 7. Verification

- **컴파일**: `read_console(types=["error"])` → 0 에러.
- **씬 구조**: `ProtoMain` 에 `HUD_Root` 1개, `EventSystem` 1개.
- **스크린샷**: `screenshot(include_image=True)` — 빈 씬에서 9 슬롯 gizmo 또는 투명 패널 확인.
- **이벤트 더미**: `HudEvents.OnFloatChanged.Raise(0.5f)` 호출 → 구독 위젯 없어도 에러 안 남.

실패 시 롤백 + 사유 1문장.
