---
name: proto-start
description: "프로토타이핑 경량 모드 진입점. 한 문장 아이디어를 받아 3분 플레이 가능한 Unity 게임의 첫 스크립트까지 단일 흐름으로 생성. 승인 게이트 없음. Proto 브랜치 전용."
argument-hint: "[한 문장 아이디어 — 선택]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Bash, Task
agent: prototyper
model: sonnet
---

# /proto-start — 프로토타이핑 진입점

**대상**: 유니티·Git·프로그래밍 경험이 전무한 사용자가 20시간 이내 3분 빌드 목표로 시작.
**전제**: Proto 브랜치 / URP 3D / Unity MCP 연결됨 / Unity 프로젝트 셋업 완료 (`Assets/` 존재).

이 스킬은 **승인 게이트·디렉터 게이트·GDD·ADR 강제 없이** 단일 흐름으로 진행합니다. 사용자가 "잠깐"이라고 하지 않는 한 멈추지 않습니다.

---

## Phase 0: 브랜치 검증 (2줄)

```bash
git branch --show-current
```

결과가 `Proto`가 아니면: "지금 Proto 브랜치가 아닙니다. `git checkout Proto` 실행하시겠어요?" 묻고 **즉시 멈춤**. 사용자 확인 후 전환.

---

## Phase 1: 아이디어 한 문장 받기

인자에 아이디어가 있으면 그걸 사용. 없으면 딱 한 번만 묻기:

> 뭘 만들고 싶어요? 한 문장이면 됩니다. 예시:
> - "좀비 피해서 3분 버티는 게임"
> - "돌아다니며 코인 모으는 게임"
> - "총 쏴서 과녁 맞추는 게임"

답변을 받으면 **되묻지 않고 진행**.

---

## Phase 2: 컨셉 즉시 작성 (승인 없이)

한 문장을 받아서 아래 10줄 템플릿을 채워 `design/proto-concept.md`에 **바로 씁니다**. 먼저 1줄 요약만 말하고 Write.

> "컨셉을 `design/proto-concept.md`에 정리합니다."

템플릿: `docs/templates/proto-game-concept.md` 참조. 필드:
- 게임 제목 (한 문장에서 추출, 없으면 Claude가 제안)
- 장르
- 한 문장 요약
- 3분 플레이 시나리오 (시간대별 3~4줄)
- 조작 (키보드 키 매핑 3~4개)
- 성공 조건 (플레이어가 뭘 하면 이김)
- 참고 게임 (유명 게임 1~2개)

**파일 쓴 후**: "컨셉 정리했어요. 바로 시작용 스크립트 만들게요." 한 줄만 말하고 다음 Phase로.

---

## Phase 3: Assets/ 존재 확인

```bash
test -d Assets/Scripts || mkdir -p Assets/Scripts/Proto
```

`Assets/` 없으면:
> "Unity 프로젝트가 아직 연결되어 있지 않습니다. Unity 프로젝트 셋업을 먼저 완료해주세요. (별도 세션에서 진행 중일 수 있습니다.)"

멈추고 사용자에게 제어 넘김. **추측으로 셋업 시도 금지.**

---

## Phase 4: 첫 스크립트 작성

`Assets/Scripts/Proto/PlayerController.cs` 생성. URP 3D 전제, `CharacterController` 기반 WASD 이동 + 점프.

```csharp
using UnityEngine;

namespace Proto.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController _controller;
        private Vector3 _velocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 move = transform.right * h + transform.forward * v;
            _controller.Move(move * moveSpeed * Time.deltaTime);

            if (_controller.isGrounded)
            {
                if (_velocity.y < 0f) _velocity.y = -2f;
                if (Input.GetButtonDown("Jump"))
                {
                    _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}
```

쓰기 전 1줄 요약: "`Assets/Scripts/Proto/PlayerController.cs`에 WASD+점프 컨트롤러를 작성합니다."

---

## Phase 5: Unity 안내

사용자에게 이렇게 말하기:

> "스크립트 완성. 이제 Unity에서:
> 1. `Assets/Scripts/Proto/PlayerController.cs`를 씬의 플레이어 오브젝트에 붙이세요.
> 2. 플레이어 오브젝트에 `CharacterController` 컴포넌트가 있는지 확인.
> 3. Play 버튼 클릭."

Unity MCP가 연결되어 있으면 추가:
> "또는 제가 Unity MCP로 씬에 자동 배치할까요?"

---

## Phase 6: 자동 커밋 + 푸시

사용자에게 추가 요청이 없으면:

```bash
git add design/proto-concept.md Assets/Scripts/Proto/PlayerController.cs
git commit -m "시작: [컨셉명] 기본 컨트롤러 + 컨셉 문서"
git push origin Proto
```

한 줄 요약: "여기까지 저장했어요. 이어서 뭐 할까요?"

---

## Phase 7: 이터레이션 루프 (선택)

사용자가 이어서 "점프 추가", "적 만들어줘" 같이 말하면:
- Claude가 해당 기능을 1줄 요약 후 즉시 구현
- 파일 작성 후 자동 커밋
- 사용자에게 "Play로 확인해보세요" 안내

**절대 하지 말 것**:
- GDD/ADR 작성 제안
- 스프린트 계획 제안
- 디렉터 게이트/검증 루프 제안
- 아키텍처 결정 요청

**대신**:
- 빠르게 작동하는 코드를 씀
- 리팩터링 안 함 (프로토타입은 버려도 되는 코드)
- 에러 핸들링 최소화

---

## 완료 조건

`/proto-start`는 **Phase 6까지 도달하면 성공**. Phase 7 이터레이션은 무한히 이어질 수 있으며, 각 기능 추가도 같은 원칙 (1줄 요약 → 즉시 작성 → 자동 커밋).
