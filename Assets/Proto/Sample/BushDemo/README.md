# BushDemo — Bush Billboard 렌더 데모

공용 모듈 `Runtime/Environment/Bush/BushBillboard.cs` 와 셰이더 `VFX/Environment/Shaders/ProtoEnv_BushBillboard.shader` 가 실제로 렌더되는 모습을 확인하는 최소 데모 씬.

## 구성

```
BushDemo/
├── Runtime/BushDemo.cs    — 런타임에 20/50/100 quads 프로시저럴 배치
├── Scenes/BushDemo.unity  — 지면 + 참조 Sphere + 3 그룹 부쉬
└── README.md
```

## 공용 모듈과의 관계

- `Runtime/Environment/Bush/BushBillboard.cs` (공용) — MeshRenderer + 2-sided 빌보드 컴포넌트.
- `VFX/Environment/Shaders/ProtoEnv_BushBillboard.shader` (공용) — Alpha clip + 람베르트 라이팅, 2 sided.
- `VFX/Environment/Textures/T_Env_LeafCluster*` (공용) — 기본 잎 텍스처.
- 이 Sample 의 `BushDemo.cs` 는 단지 "여러 개를 배치해 렌더 대비를 본다" 는 시연 코드.

## 실행 방법

1. `BushDemo.unity` 를 Unity 에서 연다.
2. Play. 자동으로 지면 위에 부쉬 그룹 3개 배치되고 라벨 ("quads=20" 등) 이 위에 뜬다.
3. Scene View 에서 카메라 각도를 바꿔가며 빌보드 회전·라이팅 반응 확인.

## 활용 포인트

- 배경 풀밭·잎사귀 렌더 저비용 솔루션 평가용.
- Alpha clip threshold / 라이팅 각도 / 카메라 각도 바뀔 때 시각 아웃풋 체크.
- 대비 기준: 참조 Sphere (Lit + 기본 Standard) 와의 라이팅 대비가 자연스러운지.
