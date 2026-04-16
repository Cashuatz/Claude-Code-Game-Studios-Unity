---
Document Role: Project Policy / Project Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: This project only
---

# INPUT_NORMALIZATION_TEMPLATE.md (Project Layer)

> 이 템플릿은 project layer 입력 정제용 overlay 템플릿이다.
> **Overlay Base:** `.vkl/core/ROLE_AND_RULES.md`, `.vkl/core/OUTPUT_CONTRACT.md`, `.vkl/core/ESCALATION_POLICY.md`
> 이 저장소에는 별도의 core `INPUT_NORMALIZATION_TEMPLATE.md`가 없으므로, 위 core 정책 문서와 `.vkl/project/*.project.md`를 함께 참조한다.
>
> **CCGS 입력 형식:** 이 템플릿은 CCGS(Claude Code Game Studios) /dev-story 입력 형식과 호환된다. Story ID 기반으로 검증 루프를 추적한다.

---

## Blank Field Rules (core 규칙과 동일, 재확인용)

| 필드 상태 | Claude 행동 |
|-----------|-------------|
| **빈 칸 (empty)** | Claude는 해당 필드에 대해 사용자에게 질문한다. 추측, 기본값 대입, 생략 **모두 금지**. |
| **"TBD" 또는 "미정"** | Claude는 해당 필드를 **불확실 영역으로 등록**하고 루프를 진행하되, 매 판정에서 해당 불확실성이 결론에 영향을 주는지 명시적으로 검토한다. 최종 Decision이 "pass"가 되려면 모든 TBD 필드가 해소되어야 한다. |
| **"N/A"** | 해당 필드가 이번 루프에 해당하지 않음을 의미한다. Claude는 수용하되, N/A 판단이 합리적인지 1회 검증 질문을 던진다. |

---

## Template Fields

아래 각 필드를 채워서 사용한다. `[ ]` 안의 설명은 지우고 실제 내용을 기입한다.

---

### [Story ID]

```
[CCGS dev-story 형식의 고유 식별자]
예: STORY-COMBAT-001, STORY-UI-012, STORY-SAVE-003
```

**작성 가이드:** `STORY-{시스템}-{번호}` 형식을 사용한다. 시스템 예시: COMBAT, UI, SAVE, ECONOMY, PHYSICS, SCENE, ASSET, BALANCE.

---

### [Task]

```
[이번 검증 루프의 목표를 한 문장으로 기술한다.]
```

**작성 가이드:** 동사 + 대상 + 완료 상태로 쓴다. Unity 게임 프로젝트에서는 "어떤 시스템/컴포넌트의 어떤 동작이 GDD/ADR의 어떤 규칙을 올바르게 구현했는지 검증한다" 형식이 이상적이다.

**프로젝트 예시:**
- "CombatSystem의 데미지 계산이 GDD combat-system.md#damage-formula를 정확히 구현했는지 검증한다."
- "SceneTransitionManager가 씬 전환 시 DontDestroyOnLoad 오브젝트를 올바르게 유지하는지 검증한다."
- "InventorySystem의 아이템 합성이 GDD economy.md#crafting-rate의 conversion_rate를 올바르게 적용하는지 검증한다."

---

### [GDD 참조]

```
[이번 루프와 관련된 GDD(게임 디자인 문서) 섹션]
예: docs/game-design/combat-system.md#damage-formula
    docs/game-design/economy.md#crafting-rate
    docs/game-design/progression.md#level-stat-table
```

**작성 가이드:** GDD 경로는 프로젝트 루트 기준으로 명시한다. 해당 섹션의 핵심 공식/수치를 발췌하여 기록한다. GDD가 없거나 해당 섹션이 미작성이면 "TBD"로 기입하고 FT-01(Spec Gap) 위험에 등록한다.

---

### [수락 기준 (Acceptance Criteria)]

```
AC-1: [Given/When/Then 형식 또는 간결한 조건문]
AC-2:
AC-3:
```

**작성 가이드:** 각 수락 기준은 검증 가능(testable)해야 한다. "올바르게 동작한다"와 같은 모호한 기준은 금지. 구체적 수치, 상태, 동작을 명시한다.

**프로젝트 예시:**
```
AC-1: Given 레벨 10 Warrior, When 기본 공격 시, Then 데미지 = base_attack * (1 + level * 0.1) = 100
AC-2: Given 씬 전환 (MainMenu → GameScene), When 전환 완료, Then GameManager.Instance != null
AC-3: Given Addressables 키 "prefab_enemy_goblin", When LoadAssetAsync, Then 로드 성공 + 프리팹 유효
```

---

### [구현 범위 (Scope)]

```
포함:
- [이번 루프에서 구현/검증하는 범위]

제외:
- [이번 루프에서 명시적으로 제외하는 범위]
```

**작성 가이드:** Unity 게임 프로젝트에서 자주 제외되는 항목: 셰이더/VFX 품질, 사운드, 네트워크 동기화(해당 루프 대상이 아닌 경우), UI 폴리싱, 성능 최적화, 다른 플랫폼 빌드 테스트.

---

### [참조 ADR (Architecture Decision Records)]

```
- [ADR 경로 및 핵심 결정 요약]
예: docs/adr/ADR-003-singleton-pattern.md -- "모든 매니저는 DontDestroyOnLoad 싱글톤 패턴 사용"
    docs/adr/ADR-007-save-system.md -- "JSON 직렬화, PlayerPrefs 사용 안 함"
```

**작성 가이드:** 이번 루프의 구현 방향에 영향을 주는 ADR을 모두 나열한다. ADR과 상충되는 구현은 금지(PROJECT_CONTEXT.md 용어 #14 참조). ADR이 없으면 "N/A"로 기입.

---

### [Inputs]

#### Code Files (코드 파일)

```
변경 파일:
- [프로젝트 루트 기준 경로, asmdef 소속 명시]

영향 파일 (변경 없음, 참조용):
- [프로젝트 루트 기준 경로]
```

**작성 가이드:** Unity 프로젝트의 주요 디렉토리 구조를 따른다. asmdef 단위로 파일을 그룹화하면 의존성 파악에 유리하다. ScriptableObject 에셋(.asset)도 변경 파일에 포함한다.

#### Sample Data / Minimal Reproduction Case (샘플 데이터)

```
[테스트에 필요한 구체적 데이터: 캐릭터 스탯, 아이템 목록, 씬 구성, 입력 시퀀스 등]
```

**작성 가이드:** 최소 3가지 테스트 케이스를 포함한다: (1) 정상 케이스 (happy path), (2) 경계값 케이스 (edge case), (3) 실패 기대 케이스 (negative case). 수치는 GDD 참조 섹션에서 발췌한다.

#### Environment Info (환경 정보)

```
- Unity 버전: [정확한 버전, 예: 2022.3.20f1 LTS]
- .NET 버전: [예: .NET Standard 2.1]
- Scripting Backend: [Mono / IL2CPP]
- Render Pipeline: [Built-in / URP / HDRP]
- 타겟 플랫폼: [Editor / Standalone / Android / iOS]
- 주요 패키지 버전: [Addressables, Input System, UniTask 등]
- OS: [OS 버전]
```

**작성 가이드:** Unity 버전은 정확히 명시한다 (LTS 여부 포함). Scripting Backend가 IL2CPP인 경우 FT-P-005(빌드/플랫폼 차이) 위험이 높아진다.

---

### [Success Criteria]

#### Functional Criteria (기능 기준)

```
입력 → 기대 출력:
1. [입력 조건] → [기대 결과]
2.
3.
```

**작성 가이드:** 게임플레이 공식 검증 시 GDD의 공식을 인용하고, 최소 3가지 입력값에 대한 기대 출력을 명시한다. 부동소수점 비교 시 tolerance를 반드시 기재한다.

#### Quality Criteria (품질 기준)

```
[성능 예산, 코드 품질, GC Allocation 제한 등]
```

**작성 가이드:** 게임 루프 내 코드는 GC Allocation 0을 목표로 한다. 프레임 예산(16.67ms for 60FPS)을 명시한다.

#### Time Criteria (시간 기준)

```
[실행 시간 제한, 프레임 예산]
```

#### Comparison Criteria (비교 기준)

```
[수정 전/후 비교 대상, GDD 기대값, 골든 데이터]
```

---

### [Required Logs / Intermediate Artifacts]

#### Required Logs (반드시 출력할 로그)

```
1. [어떤 클래스/메서드에서 어떤 값을 로그로 출력하는지]
2.
```

**작성 가이드:** Unity에서는 Debug.Log/LogWarning/LogError를 사용한다. 게임플레이 수치 검증 시 계산 중간값을 로그로 출력하여 공식 추적을 가능하게 한다.

#### Intermediate Artifacts (반드시 저장할 중간 산출물)

```
1.
2.
```

#### Changed Files List (변경 파일 목록)

```
예상 변경 파일:
-

실제 변경 파일 (루프 완료 후 갱신):
-
```

---

### [Failure Risks]

#### Expected Failure Types (예상 실패 유형)

```
| 실패 유형 | 발생 가능성 (상/중/하) | 이유 |
|-----------|----------------------|------|
|           |                      |      |
```

**프로젝트 참조 (core FT-01~FT-12 + project FT-P-001~FT-P-005):**

| ID | 유형 | 프로젝트 관련성 |
|----|------|----------------|
| FT-01 | Spec Gap | 높음 -- GDD 미정의 구간, 공식 빈칸 |
| FT-02 | Hidden Semantic Rule Misfill | 높음 -- 라이프사이클 순서, SO 런타임 수정, 버프 순서 |
| FT-P-001 | 씬 전환 시 상태 소실 | 높음 -- DontDestroyOnLoad, 싱글톤 중복 |
| FT-P-002 | Addressables 로드 실패 | 중간 -- 키 불일치, 카탈로그 미갱신, 메모리 릭 |
| FT-P-003 | 물리 비결정성 | 중간 -- FixedUpdate/Update 혼용, 레이어 누락 |
| FT-P-004 | UI 상태-데이터 불일치 | 중간 -- 바인딩 끊김, 지역화 키 누락 |
| FT-P-005 | 빌드/플랫폼 차이 | 높음 -- IL2CPP 리플렉션, 에디터 전용 API |

#### Hidden Semantic Rule Candidates (숨은 의미 규칙 후보)

```
[PROJECT_CONTEXT.md Section 3의 HSR 목록을 참조하여, 이번 루프에 해당하는 후보를 나열한다.]

1.
2.
```

**작성 가이드:** PROJECT_CONTEXT.md의 HSR-001~HSR-005를 검토하고, 이번 루프 범위에 해당하는 항목을 반드시 기입한다. "없음"이라고 쓰기 전에 HSR 목록 전체를 점검했는지 확인한다.

#### Environment Risks (환경 리스크)

```
1.
```

---

### [Oracles]

```
| 오라클 | 사용 여부 | 판정 방법 |
|--------|----------|----------|
| OR-01 (Spec Oracle) | | |
| OR-02 (Format Oracle) | | |
| OR-03 (Execution Oracle) | | |
| OR-04 (Observability Oracle) | | |
| OR-05 (Comparison Oracle) | | |
| OR-06 (Metamorphic Oracle) | | |
| OR-07 (Negative Oracle) | | |
| OR-08 (Human Oracle) | | |
| OR-09 (Environment Oracle) | | |
| OR-P-001 (Play Mode 실행 결과) | | |
| OR-P-002 (Profiler 측정값) | | |
| OR-P-003 (GDD 공식 대조) | | |
| OR-P-004 (컴파일러) | | |
| OR-P-005 (Test Framework 결과) | | |
```

**작성 가이드:** 최소 2개 이상의 오라클을 사용해야 한다. 게임플레이 공식 검증 시 OR-P-003(GDD 공식 대조)과 OR-P-005(Test Framework)를 필수 점검한다.

---

### [Test Relations]

#### Metamorphic Relations

```
[`.vkl/core/TEST_RELATIONS.base.md`의 7 패턴 + `.vkl/project/TEST_RELATIONS.project.md`의 MR-P-XXX 중 적용 가능한 것을 나열한다.]

MR-P-XXX:
```

**프로젝트 참조:**

| ID | 관계 | 적용 상황 |
|----|------|----------|
| MR-P-001 | 단조성 (Monotonicity) | 레벨/강화 → 스탯 증가 검증 시 |
| MR-P-002 | 불변성 (Invariant) | 합성/분해/거래 가치 보존 검증 시 |
| MR-P-003 | 순환 (Round-Trip) | 저장/로드 상태 동일성 검증 시 |
| MR-P-004 | 치환 불변 (Permutation) | 버프/스킬 적용 순서 무관 검증 시 |
| MR-P-005 | 부정 (Negative) | 사망/제한 상태 행동 불가 검증 시 |
| MR-P-006 | 스케일링 (Scaling) | AoE 범위/효과 비례 검증 시 |
| MR-P-007 | 대칭 (Symmetry) | 맵/진영 밸런스 대칭 검증 시 |

#### Property / Invariant

```
P1:
P2:
```

---

### [Output Contract]

```
필수 반환 섹션:
- [ ] Loop Goal
- [ ] Excluded Scope
- [ ] Observed Signals
- [ ] Failure Taxonomy
- [ ] Oracle Evaluation
- [ ] Metamorphic / Property Checks
- [ ] Decision
- [ ] Next Action
- [ ] Knowledge Assets Updated

추가 반환 섹션:
-
```

---

### [Human Confirmation Required]

```
[이번 루프에서 인간 확인이 필요한 항목을 나열한다. PROJECT_CONTEXT.md Section 5(HC-001~HC-007)를 참조한다.]

HC-XXX:
```

**작성 가이드:** 이 필드가 비어있으면 Claude는 "이번 루프에서 인간 확인이 필요한 항목이 정말 없는지" 1회 재점검 질문을 던진다. 게임플레이 공식/밸런스 관련 루프에서 이 필드가 N/A인 경우는 거의 없다.

---

---

## Completed Example: Unity Game -- 데미지 공식 검증

아래는 "전투 시스템의 데미지 계산이 GDD 공식을 정확히 구현했는지" 검증하는 루프의 완성된 입력이다.

---

### [Story ID]

```
STORY-COMBAT-001
```

### [Task]

```
CombatSystem의 데미지 계산 로직이 GDD combat-system.md#damage-formula의 공식
"damage = base_attack * (1 + level * growth_rate) * skill_multiplier - target_defense"를
정확히 구현했는지 검증한다.
```

### [GDD 참조]

```
- docs/game-design/combat-system.md#damage-formula
  공식: damage = base_attack * (1 + level * growth_rate) * skill_multiplier - target_defense
  조건: damage >= 1 (최소 데미지 보장)
  
- docs/game-design/combat-system.md#critical-hit
  크리티컬: final_damage = damage * crit_multiplier (기본 1.5)
  
- docs/game-design/progression.md#level-stat-table
  growth_rate: 0.1 per level
```

### [수락 기준 (Acceptance Criteria)]

```
AC-1: Given Lv10 Warrior(base_attack=50, growth_rate=0.1), skill_multiplier=1.0, target_defense=20,
      When 기본 공격, Then damage = 50 * (1 + 10 * 0.1) * 1.0 - 20 = 80
AC-2: Given Lv1 Warrior(base_attack=50), skill_multiplier=1.0, target_defense=200,
      When 기본 공격, Then damage = max(1, 50 * 1.1 - 200) = 1 (최소 데미지)
AC-3: Given Lv50 Warrior(base_attack=50), skill_multiplier=2.5, target_defense=100,
      When 스킬 공격, Then damage = 50 * (1 + 50 * 0.1) * 2.5 - 100 = 650
AC-4: Given AC-1 조건 + 크리티컬 발동,
      When 크리티컬 공격, Then final_damage = 80 * 1.5 = 120
```

### [구현 범위 (Scope)]

```
포함:
- DamageCalculator 클래스의 CalculateDamage() 메서드
- 최소 데미지 보장 로직
- 크리티컬 데미지 적용 로직
- CharacterStatsSO (ScriptableObject) 데이터 참조

제외:
- 버프/디버프에 의한 스탯 변동 (STORY-COMBAT-003에서 별도 검증)
- 데미지 표시 UI (STORY-UI-005에서 별도 검증)
- 히트 이펙트/사운드
- 네트워크 동기화
```

### [참조 ADR]

```
- docs/adr/ADR-005-stat-system.md -- "모든 스탯 계산은 정수 연산. 최종 결과만 float→int 변환"
- docs/adr/ADR-002-scriptable-object.md -- "런타임 스탯은 SO를 Instantiate하여 복제본 사용"
```

### [Inputs]

#### Code Files (코드 파일)

```
변경 파일:
- Assets/Scripts/Combat/DamageCalculator.cs (CCGS.Combat asmdef)

영향 파일 (변경 없음, 참조용):
- Assets/Scripts/Combat/CombatManager.cs (CCGS.Combat asmdef)
- Assets/Data/Characters/WarriorStatsSO.asset (ScriptableObject)
- Assets/Scripts/Stats/CharacterStatsSO.cs (CCGS.Stats asmdef)
```

#### Sample Data / Minimal Reproduction Case (샘플 데이터)

```
테스트 캐릭터 3종:
1. 정상 케이스: Lv10 Warrior, base_attack=50, growth_rate=0.1, skill_multi=1.0, def=20
   - 기대 데미지: 80

2. 경계값 케이스: Lv1 Warrior, base_attack=50, growth_rate=0.1, skill_multi=1.0, def=200
   - 기대 데미지: 1 (최소 데미지)

3. 고레벨 스킬: Lv50 Warrior, base_attack=50, growth_rate=0.1, skill_multi=2.5, def=100
   - 기대 데미지: 650
```

#### Environment Info (환경 정보)

```
- Unity 버전: 2022.3.20f1 LTS
- .NET 버전: .NET Standard 2.1
- Scripting Backend: IL2CPP (빌드 시)
- Render Pipeline: URP 14.0.9
- 타겟 플랫폼: Editor (검증), Android (최종)
- 주요 패키지: Addressables 1.21.19, UniTask 2.5.0
- OS: Windows 11 Pro 10.0.22621
```

### [Success Criteria]

#### Functional Criteria (기능 기준)

```
입력 → 기대 출력:
1. (base=50, lv=10, growth=0.1, skill=1.0, def=20) → damage = 80
2. (base=50, lv=1, growth=0.1, skill=1.0, def=200) → damage = 1 (최소 데미지)
3. (base=50, lv=50, growth=0.1, skill=2.5, def=100) → damage = 650
4. (base=50, lv=10, growth=0.1, skill=1.0, def=20, crit=true) → damage = 120
```

#### Quality Criteria (품질 기준)

```
- 데미지 계산 함수는 GC Allocation 0이어야 한다 (매 프레임 호출 가능)
- 정수 연산 기반 (ADR-005 준수), 부동소수점 사용 시 최종 단계에서만
- 모든 입력에 대해 결정적 결과 (동일 입력 → 동일 출력)
```

#### Time Criteria (시간 기준)

```
- 단일 데미지 계산: 0.01ms 이내 (프레임 예산 16.67ms의 0.1% 미만)
```

#### Comparison Criteria (비교 기준)

```
- GDD 공식의 수동 계산 결과 vs 코드 실행 결과
- ADR-005의 정수 연산 규칙 준수 여부
```

### [Required Logs / Intermediate Artifacts]

#### Required Logs

```
1. DamageCalculator: "Base damage: base_attack={0}, level_factor={1}, skill_multi={2}"
2. DamageCalculator: "Raw damage before defense: {0}"
3. DamageCalculator: "Final damage (after defense, min clamp): {0}"
4. DamageCalculator: "Critical hit applied: {0} * {1} = {2}" (크리티컬 시)
```

#### Intermediate Artifacts

```
1. 테스트 케이스별 계산 단계 추적 테이블
2. EditMode 테스트 결과 리포트
```

#### Changed Files List

```
예상 변경 파일:
- Assets/Scripts/Combat/DamageCalculator.cs (공식 구현)
- Assets/Tests/EditMode/Combat/DamageCalculatorTests.cs (테스트)

실제 변경 파일 (루프 완료 후 갱신):
- (루프 완료 후 기입)
```

### [Failure Risks]

#### Expected Failure Types

```
| 실패 유형 | 발생 가능성 | 이유 |
|-----------|-----------|------|
| FT-01: Spec Gap | 중 | GDD에 "최소 데미지"의 적용 시점(크리티컬 전/후)이 미정의 |
| FT-02: Hidden Semantic Rule Misfill | 상 | growth_rate 적용 방식(가산 vs 승산) 해석 차이 |
| FT-P-004: UI 상태-데이터 불일치 | 중 | 계산된 데미지와 UI 표시 데미지의 불일치 가능 |
| FT-09: Edge-Case Fragility | 중 | 방어력이 공격력을 초과하는 극단 케이스 |
| FT-P-005: 빌드/플랫폼 차이 | 하 | IL2CPP에서 정수/실수 연산 차이 가능성 낮음 |
```

#### Hidden Semantic Rule Candidates

```
1. HSR-001: MonoBehaviour 라이프사이클 -- DamageCalculator 초기화 타이밍이 
   CharacterStatsSO 로드 완료 후여야 함
2. HSR-002: ScriptableObject 런타임 수정 -- CharacterStatsSO를 직접 수정하면 
   에디터 에셋 변경됨. 반드시 Instantiate 사용 (ADR-002)
3. HSR-005: Editor/Build 차이 -- 정수 연산 규칙(ADR-005)이 IL2CPP에서도 동일하게 동작하는지
```

#### Environment Risks

```
1. Unity 2022.3 LTS의 float 정밀도가 이전 버전과 다를 수 있음
2. IL2CPP 빌드 시 정수 오버플로우 동작이 Mono와 다를 수 있음
```

### [Oracles]

```
| 오라클 | 사용 여부 | 판정 방법 |
|--------|----------|----------|
| OR-01 (Spec Oracle) | 사용 | GDD combat-system.md#damage-formula 공식과 코드 대조 |
| OR-02 (Format Oracle) | 미사용 | -- |
| OR-03 (Execution Oracle) | 사용 | 4개 테스트 케이스 실행, 기대값 비교 |
| OR-04 (Observability Oracle) | 사용 | 계산 중간값 로그 출력 확인 |
| OR-05 (Comparison Oracle) | 사용 | 수동 계산 결과와 코드 결과 비교 |
| OR-06 (Metamorphic Oracle) | 사용 | MR-P-001(단조성), MR-P-004(치환) 검증 |
| OR-07 (Negative Oracle) | 사용 | 방어력 > 공격력 시 최소 데미지 1 반환 여부 |
| OR-08 (Human Oracle) | 대기 | "최소 데미지 적용 시점" GDD 미정의 시 확인 필요 |
| OR-09 (Environment Oracle) | 미사용 | -- |
| OR-P-001 (Play Mode 실행 결과) | 사용 | Play Mode에서 실제 전투 시 데미지 확인 |
| OR-P-002 (Profiler 측정값) | 사용 | GC Allocation 0 확인 |
| OR-P-003 (GDD 공식 대조) | 사용 | 핵심 -- GDD 공식과 코드 로직 1:1 대조 |
| OR-P-004 (컴파일러) | 사용 | 컴파일 성공 확인 |
| OR-P-005 (Test Framework 결과) | 사용 | EditMode 테스트 전체 통과 확인 |
```

### [Test Relations]

#### Metamorphic Relations

```
MR-P-001: 레벨 증가 시 데미지 단조 증가
  - Lv1 → damage_1, Lv10 → damage_10, Lv50 → damage_50
  - 기대: damage_1 <= damage_10 <= damage_50

MR-P-004: 버프 적용 순서 무관 (이번 루프에서는 크리티컬 + 기본 데미지 순서)
  - 순서 A: 기본 데미지 계산 → 크리티컬 적용
  - 순서 B: (동일한 결과여야 함 -- 크리티컬은 최종 데미지에 곱셈)

MR-P-005: 사망(HP<=0) 상태 적에게 데미지 적용 시 동작 확인
  - 사망 상태 → 데미지 계산 자체는 수행되되, HP가 음수로 가지 않는지 확인
```

#### Property / Invariant

```
P1: damage >= 1 (최소 데미지 보장, 모든 입력 조합에서)
P2: 동일 입력에 대해 반복 실행 시 동일 결과 (결정적)
P3: base_attack, level, growth_rate가 모두 양수이면 raw_damage > 0
```

### [Output Contract]

```
필수 반환 섹션:
- [ ] Loop Goal
- [ ] Excluded Scope
- [ ] Observed Signals
- [ ] Failure Taxonomy
- [ ] Oracle Evaluation
- [ ] Metamorphic / Property Checks
- [ ] Decision
- [ ] Next Action
- [ ] Knowledge Assets Updated

추가 반환 섹션:
- [ ] GDD 공식 일치 상세 리포트
```

### [Human Confirmation Required]

```
HC-005: GDD 공식의 "최소 데미지 1"이 크리티컬 적용 전인지 후인지 미정의
        (크리티컬 전이면: max(1, raw) * crit, 크리티컬 후이면: max(1, raw * crit))
HC-002: CharacterStatsSO를 런타임에서 Instantiate하는 것이 맞는지, 
        또는 읽기 전용으로 직접 참조해도 되는지 확인 (이번 루프에서는 읽기만 수행)
```
