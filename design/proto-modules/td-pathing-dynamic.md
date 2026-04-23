# Module: td-pathing-dynamic

> 참고: [`_catalog-td.md`](./_catalog-td.md) / [`_conventions-td.md`](./_conventions-td.md)

## 1. Purpose

Q2-C. 플레이어 구조물 변화에 따라 FlowField **동적 재빌드**. HR-TD-5 명시적 예외.

## 2. Hard Rules

- HR-TD-5 무효화 — 우회 허용.
- 재빌드 트리거: 허브 추가/제거 이벤트. 링크 변경은 영향 없음 (링크는 시각적).
- 재빌드 쿨다운 500ms (연속 변경 시 throttle).

## 3. Public API

```csharp
namespace Proto.TD.Pathing
{
    public sealed class DynamicFlowFieldPathing : FlowFieldPathing
    {
        public void RequestRebuild();
        public event Action OnRebuilt;
    }
}
```

## 4. Dependencies

- Required: `td-flowfield-builder`, `td-hub-network`, `td-sim-core`, `td-enemy-core`.

## 5. Default Prefabs/Assets

- 스크립트: `DynamicFlowFieldPathing.cs`.
- SO: `FlowField_Dynamic.asset`.

## 6. Skill Hook

`/proto-td-pathing dynamic`:

1. `DynamicFlowFieldPathing.cs` 생성.
2. Builder 자동 설치.
3. `HubNetwork.OnHubAdded` / `OnHubRemoved` 구독 → throttled RequestRebuild.

## 7. Verification

- 허브 배치 → 500ms 내 재빌드.
- 연속 10회 배치 → 재빌드는 최대 2~3회.
- 자동 플레이테스트(HR-TD-8) 실행 시 결정론 경고 표기.
