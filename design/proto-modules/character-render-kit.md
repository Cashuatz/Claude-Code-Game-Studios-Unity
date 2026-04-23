# Module: CharacterRenderKit

> 참고: [`_catalog-turn3d.md`](./_catalog-turn3d.md) / [`_conventions.md`](./_conventions.md)

## 1. Purpose

캐릭터 시각화 방식(**2D 빌보드** vs **3D 스킨드 메쉬**)을 공통 API 뒤로 숨긴다.
유닛 스폰·애니메이션·피격 플래시·좌/우 엣지 일러스트 컷인 같은 연출 코드가
렌더 타입에 **분기하지 않도록** 보장 (HR-7 Liskov 치환).

Unit 본체(MovementAgent + TurnSystem 등록)는 렌더 타입에 무관하고,
`ICharacterRenderer` 하위로 렌더 방식을 주입한다.

## 2. Hard Rules

- **HR-7 준수**: 렌더 타입 분기 금지. `if (renderer is Billboard) ...` 같은 코드 금지.
- **HR-11 준수**: 컷인 일러스트는 uGUI Canvas (World Space 또는 Screen Space) 로 렌더.
- 빌보드는 `Camera.main.transform` 참조 금지 — `CameraRig` 에서 active 카메라 조회.
- 스킨드 메쉬 애니메이션은 `Animator` 사용 OK (rigidbody 와 무관).
- 모든 Renderer 는 `Unit_Base.prefab` 하위에 **자식 GameObject**로 붙는다.
  Unit 본체 프리팹은 렌더 타입에 무지.

## 3. Public API

```csharp
namespace Proto.Render
{
    public enum RenderKind { Billboard2D, Skinned3D }

    public interface ICharacterRenderer
    {
        RenderKind Kind { get; }
        Transform RootTransform { get; }        // 렌더 GameObject 루트
        Bounds VisualBounds { get; }            // 프레이밍용

        void PlayAnimation(string clipName, bool loop = false);
        void StopAnimation();
        void SetFacing(Vector3 worldDirection); // 2D: flipX, 3D: rotate
        void TriggerHitFlash(Color color, float durationSec);

        event Action<string> AnimationEnded;    // clipName
    }

    public interface IUnit
    {
        // Proto.Turn.IUnit 와 동일 (재사용)
        ICharacterRenderer Renderer { get; }
    }

    /// <summary>좌/우 엣지 일러스트 컷인 연출.</summary>
    public interface ICutinDirector
    {
        /// <param name="side">Left or Right</param>
        /// <param name="portrait">표시할 Sprite</param>
        /// <param name="durationSec">지속 시간</param>
        void Show(CutinSide side, Sprite portrait, float durationSec);
        void HideAll();

        event Action<CutinSide> Shown;
        event Action<CutinSide> Hidden;
    }

    public enum CutinSide { Left, Right }
}

[CreateAssetMenu(menuName = "Proto/Render/CharacterDefinition")]
public class CharacterDefinition : ScriptableObject
{
    public string displayName;
    public RenderKind renderKind;

    [Header("Billboard (used if renderKind=Billboard2D)")]
    public Sprite billboardSprite;
    public Vector2 billboardSize = new Vector2(1.5f, 2f);

    [Header("Skinned (used if renderKind=Skinned3D)")]
    public GameObject skinnedPrefab;            // SkinnedMeshRenderer + Animator 포함

    [Header("Cutin Portrait (optional)")]
    public Sprite cutinPortrait;
}
```

### 구현

- `BillboardRenderer : MonoBehaviour, ICharacterRenderer`
  - `SpriteRenderer` 자식. `LateUpdate` 에서 active 카메라 방향으로 회전.
  - "애니메이션" 은 Sprite 배열 순환 (단순 frame-by-frame).
- `SkinnedRenderer : MonoBehaviour, ICharacterRenderer`
  - `Animator` + `SkinnedMeshRenderer` 래핑.
  - `PlayAnimation(clip)` → `Animator.CrossFade(clip, 0.1f)`.

### CutinDirector 구현

- `CutinDirector : MonoBehaviour, ICutinDirector`
  - Canvas (Screen Space - Overlay) 위에 좌/우 `Image` 2개 배치.
  - Show 시 화면 밖 → 안으로 Tween (코루틴 or DOTween 금지 — 기본 `Mathf.Lerp`).

## 4. Dependencies

- **Required**:
  - `CameraRig` — 빌보드 방향 계산
  - `MovementCore` — `Unit_Base.prefab` 하위에 렌더 붙임
- **Optional**:
  - `TurnSystem` — Hit flash 가 데미지 이벤트 수신 시 트리거
  - `HUDKit` — 컷인과 HUD 레이어 관리

## 5. Default Prefabs/Assets

| 경로 | 내용 |
|------|------|
| `Assets/Proto/Prefabs/Render/Renderer_Billboard.prefab` | `SpriteRenderer` + `BillboardRenderer` MB |
| `Assets/Proto/Prefabs/Render/Renderer_Skinned.prefab` | 빈 Root + `SkinnedRenderer` MB (skinnedPrefab 은 런타임에 주입) |
| `Assets/Proto/Prefabs/UI/CutinRoot.prefab` | Canvas + `CutinDirector` MB + 좌/우 Image 2개 |
| `Assets/Proto/Data/Units/Character_Placeholder_Billboard.asset` | 테스트용 `CharacterDefinition` |
| `Assets/Proto/Data/Units/Character_Placeholder_Skinned.asset` | 테스트용 |

Runtime 스크립트:
- `RenderKind.cs`, `ICharacterRenderer.cs`, `CharacterDefinition.cs`
- `BillboardRenderer.cs`, `SkinnedRenderer.cs`
- `ICutinDirector.cs`, `CutinSide.cs`, `CutinDirector.cs`
- `Unit.cs` (Proto.Turn.IUnit + Proto.Render.IUnit 통합 구현; MovementAgent 와 연결)

## 6. Skill Hook

`/proto-character <billboard|skinned>` 호출 시:

1. `Assets/Proto/Runtime/Render/` 생성 (첫 호출 시).
2. 공통 스크립트 + `Unit.cs` 설치 (첫 호출 시).
3. 지정 렌더 타입의 `Renderer_*.prefab` + `*Renderer.cs` 설치.
4. `CharacterDefinition` placeholder asset 생성 (해당 타입).
5. `Unit_Base.prefab` 에 선택한 렌더 프리팹 **자식으로** 붙여 Variant 생성:
   - Billboard: `Unit_Billboard.prefab` (Variant of Unit_Base)
   - Skinned: `Unit_Skinned.prefab` (Variant of Unit_Base)
6. `ProtoMain.unity` 에 테스트 인스턴스 1개 스폰.
7. 콘솔 에러 확인.

첫 호출이든 아니든 `CutinRoot.prefab` + `CutinDirector` 은 **공통 인프라**로 간주,
첫 호출 시 자동 설치.

옵션 인자:
- `--no-spawn` — 씬에 테스트 유닛 스폰하지 않음.
- `--with-cutin-demo` — Play 시 좌/우 컷인 1회 자동 재생 (시각 검증용).

## 7. Verification

1. `read_console(types=["error"])` → 0.
2. `find_gameobjects(search_term="Unit_Billboard", search_method="by_name")` 또는 `Unit_Skinned` ≥ 1.
3. `execute_code` 스모크:
   ```csharp
   var unit = UnityEngine.Object.FindObjectOfType<Proto.Render.Unit>();
   UnityEngine.Debug.Assert(unit.Renderer != null);
   UnityEngine.Debug.Assert(unit.Renderer.RootTransform != null);

   // HR-7 검증 — 렌더러 타입 분기 금지 (Liskov). Kind 만 참조 가능.
   var kind = unit.Renderer.Kind;
   unit.Renderer.PlayAnimation("Idle");
   unit.Renderer.TriggerHitFlash(Color.red, 0.2f);
   ```
4. `manage_camera(action="screenshot", include_image=True)` → 유닛이 카메라에 잡히는지.
5. `--with-cutin-demo` 지정 시 좌/우 Image 가 화면에 들어왔다 나가는지 스크린샷.

**오라클**:
- OR-COMPILE-01, OR-RUNTIME-01, OR-VISUAL-01
- OR-HARD-RULE-07 (HR-7 렌더 폴리모프 준수)
