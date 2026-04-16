---
Document Role: Project Policy / Project Reference
Update Policy: Claude read-only. 사람 승인 없이 수정 금지. 변경 필요 시 .vkl/proposals/에 제안서 작성.
Owner: Human (project lead)
Scope: This project only
---

# TEST_RELATIONS.project.md

> **Document Role: Project Reference** -- `.vkl/core/TEST_RELATIONS.base.md`를 **overlay(확장)**한다.
> core의 7가지 relation pattern(Permutation Invariance, Monotonicity, Round-Trip, Representation Invariance, Negative Oracle, Property/Invariant, Counterexample Logging)은 그대로 유효하며, 이 문서는 Unity 게임 개발 프로젝트에 특화된 metamorphic relation과 property를 추가 정의한다.
> **Overlay Of:** `.vkl/core/TEST_RELATIONS.base.md`
> **Merge Rule:** core relation catalog를 먼저 로드한 뒤 이 project relation catalog를 추가 적용한다. 이 문서는 core MR-XX를 대체하지 않는다.
>
> **ID 규칙:** 프로젝트 relation은 `MR-P-XXX` 형식을 사용한다. core의 relation ID(`MR-01~MR-07`)와 중복되지 않는다.
>
> **Counterexample 기록:** core Section 7의 Minimal Counterexample Logging 규칙이 동일하게 적용된다. project relation 실패 시 counterexample의 `relation` 필드에는 `MR-P-XXX` 또는 `MR-P-XXX-[NNN]`를 사용할 수 있다.

---

## Project-Specific Metamorphic Relations

---

### MR-P-001: 단조성 (Monotonicity) -- 레벨 상승 시 공격력 증가

| 항목 | 내용 |
|------|------|
| **Overlay Of** | MR-02 (Monotonicity) |
| **Pattern** | Monotonicity (core pattern #2)의 프로젝트 특화 변형 |
| **Relation** | 캐릭터의 레벨이 증가하면, **공격력(또는 기타 성장 스탯)은 단조 증가**해야 한다. 레벨업 후 스탯이 감소하면 성장 공식에 결함이 있다. 이 관계는 GDD에 정의된 모든 단조 성장 스탯(HP, 방어력, 공격력 등)에 동일하게 적용된다. |
| **Formal Definition** | ```Given: 캐릭터 C, level_1 < level_2 (나머지 장비/버프 조건 동일)  When: stat_1 = calculate_attack(C, level_1)        stat_2 = calculate_attack(C, level_2)  Then: stat_1 <= stat_2``` |
| **Test Template** | ```캐릭터: Warrior, 장비 없음, 버프 없음  Test series:  - Level 1 → attack_1  - Level 10 → attack_10  - Level 50 → attack_50  - Level 99 → attack_99  기대: attack_1 <= attack_10 <= attack_50 <= attack_99``` |
| **Expected Relationship** | 스탯이 레벨에 대해 non-decreasing. 동일 레벨(plateau)은 허용하되 감소는 불허. |
| **Failure Indicates** | - 성장 공식에 오버플로우 또는 부동소수점 오류가 있다. <br>- 레벨 구간별 다른 공식이 적용되는데 경계에서 불연속이 발생한다. <br>- ScriptableObject의 레벨 테이블 데이터 입력 오류. <br>- FT-P-004(UI 상태-데이터 불일치) + FT-01(Spec Gap) 가능성. |
| **Pre-conditions** | - 장비, 버프 등 외부 요인을 제거하고 순수 레벨 효과만 측정한다. <br>- GDD에서 해당 스탯이 단조 성장으로 정의되어 있어야 한다. <br>- 레벨 캡(최대 레벨) 이하에서만 유효하다. |
| **Connected** | FT-01, FT-P-004, VC-P-D-03, OR-P-003, OR-06, MR-02 |

---

### MR-P-002: 불변성 (Invariant) -- 합성 전후 총 가치 보존

| 항목 | 내용 |
|------|------|
| **Overlay Of** | MR-06 (Property/Invariant) |
| **Pattern** | Property/Invariant (core pattern #6) -- Conservation 패턴의 프로젝트 특화 |
| **Relation** | 아이템 합성/강화/분해 시, 투입된 자원의 **총 가치(환산값)**와 산출된 결과물의 총 가치가 설계된 비율로 보존되어야 한다. "무에서 유" 또는 "가치 소실/증폭"이 기획 의도와 다르게 발생하면 경제 시스템에 결함이 있다. |
| **Formal Definition** | ```Given: 합성 재료 items = [item_1, item_2, ...], 각각 가치 v_i  When: result = craft(items), 결과물 가치 v_result  Then: v_result == sum(v_i) * conversion_rate (GDD 정의 비율)        또는 abs(v_result - sum(v_i) * rate) / sum(v_i) < tolerance``` |
| **Test Template** | ```아이템 합성:  재료: 철광석(가치 10) x3 + 목재(가치 5) x2 = 총 투입 가치 40  결과: 철제 검(가치 36) -- GDD 정의 conversion_rate = 0.9 (10% 소모)  검증: abs(36 - 40 * 0.9) / 40 < 0.01  역방향(분해):  철제 검(가치 36) → 철광석 x2 + 목재 x1 = 총 산출 가치 25  GDD 분해 rate = 0.7  검증: abs(25 - 36 * 0.7) / 36 < 0.05``` |
| **Expected Relationship** | 합성/분해의 입출력 가치 비율이 GDD에 정의된 conversion_rate와 일치한다. |
| **Failure Indicates** | - 합성 공식에서 재료 수량을 잘못 계산한다. <br>- conversion_rate가 코드와 GDD에서 다르다. <br>- 반올림/내림 처리가 의도와 다르다. <br>- 합성/분해를 반복하면 가치가 무한 증식하는 익스플로잇이 존재한다. <br>- FT-01(Spec Gap -- conversion_rate 미정의) 가능성. |
| **Pre-conditions** | - GDD에 conversion_rate가 정의되어 있어야 한다. <br>- 가치 환산 기준(기본 화폐 환산 등)이 명확해야 한다. <br>- 합성/분해가 확률적이라면 기대값으로 검증한다. |
| **Connected** | FT-01, FT-P-004, VC-P-D-03, OR-P-003, OR-06, MR-06 |

---

### MR-P-003: 순환 (Round-Trip) -- 저장 후 로드 시 상태 동일

| 항목 | 내용 |
|------|------|
| **Overlay Of** | MR-03 (Round-Trip) |
| **Pattern** | Round-Trip (core pattern #3)의 프로젝트 특화 변형 |
| **Relation** | 게임 상태를 저장(Serialize)한 후 로드(Deserialize)하면, **저장 전 상태와 로드 후 상태가 동일**해야 한다. 인벤토리, 퀘스트 진행, 캐릭터 스탯, 월드 상태 등 모든 저장 대상 데이터에 적용된다. |
| **Formal Definition** | ```Given: 게임 상태 S1  When: save(S1) → file → S2 = load(file)  Then: S1 == S2 (모든 저장 대상 필드에 대해)``` |
| **Test Template** | ```게임 상태 설정:  - 인벤토리: [철제 검 x1, 포션 x5]  - 캐릭터: Level 15, HP 230/300, 위치 (10.5, 0, -3.2)  - 퀘스트: "마을 수호" 진행 중 (3/5 처치)  저장 → 로드 후 검증:  - 인벤토리 아이템 목록과 수량 동일  - 캐릭터 레벨, HP, 위치 동일 (위치는 epsilon 허용)  - 퀘스트 상태와 진행도 동일``` |
| **Expected Relationship** | 저장 전과 로드 후의 게임 상태가 완전히 동일하다. 부동소수점 값(Vector3 등)은 직렬화 정밀도에 따른 오차를 허용한다. |
| **Failure Indicates** | - 직렬화/역직렬화 과정에서 필드 누락 (새 필드 추가 후 마이그레이션 미수행). <br>- 직렬화 불가 타입(Dictionary, delegate 등)이 저장 대상에 포함되어 무시됨. <br>- Enum 값이 변경되어 기존 저장 데이터와 불일치. <br>- Unity Object 참조(GameObject, Component)가 직렬화 후 끊어짐. <br>- FT-P-001(씬 전환 시 상태 소실)과 연관될 수 있다. |
| **Pre-conditions** | - 저장/로드 시스템이 구현되어 있어야 한다. <br>- 저장 대상 데이터의 범위가 명확하게 정의되어 있어야 한다. <br>- 부동소수점 비교 시 epsilon (예: 1e-4)을 사전 정의한다. |
| **Connected** | FT-P-001, FT-05, VC-P-V-01, OR-P-001, OR-P-005, OR-06, MR-03 |

---

### MR-P-004: 치환 불변 (Permutation) -- 버프 적용 순서 무관

| 항목 | 내용 |
|------|------|
| **Overlay Of** | MR-01 (Permutation Invariance) |
| **Pattern** | Permutation Invariance (core pattern #1)의 프로젝트 특화 변형 |
| **Relation** | 여러 버프/디버프를 적용할 때, **적용 순서에 관계없이 최종 스탯 결과가 동일**해야 한다. 버프 A를 먼저 적용한 후 B를 적용한 것과, B를 먼저 적용한 후 A를 적용한 것의 결과가 같아야 한다. |
| **Formal Definition** | ```Given: 캐릭터 C, 버프 집합 {B1, B2, ..., Bn}  When: 순서 P1 = [B1, B2, ..., Bn], 순서 P2 = [Bn, ..., B2, B1]  Then: apply_buffs(C, P1).final_stats == apply_buffs(C, P2).final_stats``` |
| **Test Template** | ```캐릭터: Warrior, base_attack = 100  버프 A: 공격력 +20% (비율)  버프 B: 공격력 +50 (고정값)  순서 1: A → B: 100 * 1.2 + 50 = 170? 또는 (100 + 50) * 1.2 = 180?  순서 2: B → A: 같은 결과여야 함  GDD 규칙: "비율 버프는 base에 적용, 고정 버프는 이후 가산"  → 순서 무관 결과: 100 * 1.2 + 50 = 170``` |
| **Expected Relationship** | 모든 순열(permutation)에 대해 최종 스탯이 동일하다. |
| **Failure Indicates** | - 버프 적용 로직이 순차적 계산(chained calculation)으로 구현되어 순서 의존적이다. <br>- 비율 버프와 고정 버프의 우선순위 규칙(GDD 정의)이 코드에 반영되지 않았다. <br>- 버프 스택 제한이 적용 순서에 따라 다른 결과를 낳는다. <br>- FT-02(Hidden Semantic Rule Misfill -- "버프 적용 순서" 규칙 해석 오류) 가능성. |
| **Pre-conditions** | - GDD에서 버프 적용 규칙(비율/고정 우선순위, 스택 제한)이 정의되어 있어야 한다. <br>- 순서 의존적인 버프가 기획적으로 의도된 경우, 해당 버프를 이 MR의 대상에서 제외한다. <br>- 테스트 시 최소 3개 이상의 버프 조합으로 검증한다. |
| **Connected** | FT-02, FT-01, VC-P-D-03, OR-P-003, OR-06, MR-01 |

---

### MR-P-005: 부정 (Negative) -- HP 0 이하 시 행동 불가

| 항목 | 내용 |
|------|------|
| **Overlay Of** | MR-05 (Negative Oracle) |
| **Pattern** | Negative Oracle (core pattern #5)의 프로젝트 특화 변형 |
| **Relation** | 캐릭터의 HP가 0 이하(사망 상태)가 되면, **이동, 공격, 스킬 사용 등 게임플레이 행동이 불가능**해야 한다. 사망 상태에서 행동이 가능하면 게임 로직에 결함이 있다. |
| **Formal Definition** | ```Given: 캐릭터 C, HP <= 0 (dead state)  When: attempt_action(C, action) for action in [move, attack, skill, interact]  Then: all(attempt_action(C, action) == BLOCKED | FAIL | NO_EFFECT)``` |
| **Test Template** | ```캐릭터: Warrior, HP = 0 (사망)  시도 행동:  1. 이동(Move) → 기대: 이동 불가, 위치 변경 없음  2. 공격(Attack) → 기대: 공격 불가, 데미지 0  3. 스킬 사용(UseSkill) → 기대: 스킬 발동 불가  4. 아이템 사용(UseItem) → 기대: 사용 불가  5. 상호작용(Interact) → 기대: 상호작용 불가  모든 행동이 차단되면 PASS``` |
| **Expected Relationship** | 사망 상태에서 모든 게임플레이 행동이 차단된다. |
| **Failure Indicates** | - 행동 실행 전 HP/생존 상태 체크가 누락되었다. <br>- 사망 상태 전이(state transition)가 즉시 적용되지 않고, 다음 프레임에 적용되어 1프레임 동안 행동 가능. <br>- 부활(Revive) 로직이 HP를 복구하기 전에 행동 가능 상태를 먼저 해제한다. <br>- FT-P-001(씬 전환 시 상태 소실 -- 사망 상태가 씬 전환 후 초기화) 가능성. |
| **Pre-conditions** | - "사망 상태"의 정의가 명확해야 한다 (HP == 0? HP <= 0?). <br>- 사망 상태에서 허용되는 행동(부활 대기, 관전 등)이 있으면 해당 행동을 제외한다. <br>- 무적(Invincible) 상태와 사망 상태의 우선순위가 명확해야 한다. |
| **Connected** | FT-02, FT-09, VC-P-D-01, OR-P-001, OR-P-005, OR-07, MR-05 |

---

### MR-P-006: 스케일링 (Scaling) -- 폭발 반경 증가 시 영향 오브젝트 증가

| 항목 | 내용 |
|------|------|
| **Overlay Of** | MR-02 (Monotonicity) |
| **Pattern** | Monotonicity (core pattern #2)와 Property/Invariant (core pattern #6)의 혼합 |
| **Relation** | 영역 효과(Area of Effect)의 범위를 증가시키면, **영향을 받는 오브젝트 수가 증가하거나 동일**해야 한다. 범위를 넓혔는데 영향 대상이 줄어들면 범위 판정 로직에 결함이 있다. |
| **Formal Definition** | ```Given: AoE 스킬 S, 범위 radius_1 < radius_2  When: targets_1 = get_targets_in_range(S, radius_1)        targets_2 = get_targets_in_range(S, radius_2)  Then: count(targets_1) <= count(targets_2)        AND targets_1 ⊆ targets_2 (부분집합 관계)``` |
| **Test Template** | ```폭발 스킬: 중심 (0, 0, 0), 대상 5체 배치  대상 위치: (1,0,0), (3,0,0), (5,0,0), (7,0,0), (10,0,0)  반경 2 → 대상 1체 [(1,0,0)]  반경 4 → 대상 2체 [(1,0,0), (3,0,0)]  반경 6 → 대상 3체 [(1,0,0), (3,0,0), (5,0,0)]  기대: 반경 증가 시 대상 수 단조 증가, 이전 대상 모두 포함``` |
| **Expected Relationship** | 범위가 넓어지면 영향 대상이 non-decreasing이며, 작은 범위의 대상은 큰 범위의 대상에 항상 포함된다. |
| **Failure Indicates** | - 범위 판정에서 Physics.OverlapSphere의 layerMask가 잘못 설정되었다. <br>- 거리 계산에서 2D/3D 혼용 (Y축 포함 여부). <br>- 범위 증가 시 충돌 감지 최적화(spatial partitioning)가 오작동한다. <br>- FT-P-003(물리 비결정성 -- 충돌 레이어 누락) 가능성. |
| **Pre-conditions** | - 대상 오브젝트가 고정 위치에 배치되어야 한다 (이동 중이면 비결정적). <br>- Physics Layer가 올바르게 설정되어 있어야 한다. <br>- AoE 판정 방식(구체/원통/원뿔 등)이 명확해야 한다. |
| **Connected** | FT-P-003, FT-10, VC-P-D-01, OR-P-001, OR-P-005, OR-06, MR-02 |

---

### MR-P-007: 대칭 (Symmetry) -- 미러맵 양팀 승률 대칭

| 항목 | 내용 |
|------|------|
| **Overlay Of** | MR-06 (Property/Invariant) |
| **Pattern** | Property/Invariant (core pattern #6)과 Representation Invariance (core pattern #4)의 혼합 |
| **Relation** | 대칭 구조의 맵(미러맵)에서 동일한 구성의 두 팀이 대전하면, **충분한 횟수의 시뮬레이션에서 양팀 승률이 통계적으로 대칭**이어야 한다. 한쪽 팀이 유의미하게 유리하면 맵 밸런스 또는 스폰 위치에 결함이 있다. |
| **Formal Definition** | ```Given: 대칭 맵 M, 동일 구성 팀 T1과 T2  When: N회 시뮬레이션 실행 (N >= 100)        win_rate_T1 = wins_T1 / N        win_rate_T2 = wins_T2 / N  Then: abs(win_rate_T1 - win_rate_T2) <= tolerance (tolerance = 0.10, 10%)``` |
| **Test Template** | ```맵: SymmetricArena (좌우 미러)  팀 구성: 양팀 모두 Warrior x3  시뮬레이션: 1000회 자동 대전 (AI vs AI)  결과:  - T1(좌측) 승률: 52.3%  - T2(우측) 승률: 47.7%  - 차이: 4.6% → tolerance 10% 이내 → PASS``` |
| **Expected Relationship** | 양팀 승률이 50:50에 가깝다 (10% 이내). 통계적 유의성 검정(chi-squared 등)을 적용할 수 있다. |
| **Failure Indicates** | - 맵이 구조적으로 비대칭이다 (시각적으로 대칭이지만 충돌/네비게이션 데이터가 비대칭). <br>- 스폰 위치가 한쪽에 유리하다 (거리, 엄폐물 접근성 등). <br>- AI 행동이 좌표축 방향에 의존한다 (좌/우 선호). <br>- 라이팅/시야 조건이 한쪽에 유리하다. <br>- 랜덤 시드가 한쪽에 체계적으로 유리한 패턴을 생성한다. |
| **Pre-conditions** | - 맵이 기획상 대칭으로 의도되어야 한다. <br>- 양팀의 캐릭터 구성과 장비가 동일해야 한다. <br>- 충분한 시뮬레이션 횟수(최소 100회, 이상적으로 1000회)가 필요하다. <br>- AI 기반 시뮬레이션을 사용하므로, AI 자체의 편향이 없어야 한다. |
| **Connected** | FT-01, FT-10, VC-P-D-03, OR-P-001, OR-06, MR-06, MR-01 |

---

## Selection Guide: Core + Project Relations

`.vkl/core/TEST_RELATIONS.base.md`의 Selection Guide를 프로젝트 맥락으로 확장한다.

### Project Decision Tree

```
Unity 게임 프로젝트에서 어떤 기능을 검증하는가?
│
├─ 캐릭터 성장/스탯 시스템 검증
│   ├─ MR-P-001 (단조성 -- 레벨↑ → 스탯↑)
│   ├─ MR-P-004 (치환 불변 -- 버프 순서 무관)
│   └─ core MR-06 (Property/Invariant -- 스탯 범위 유효)
│
├─ 경제/합성 시스템 검증
│   ├─ MR-P-002 (불변성 -- 합성 가치 보존)
│   ├─ MR-P-001 (단조성 -- 강화 레벨↑ → 효과↑)
│   └─ core MR-03 (Round-Trip -- 거래 취소 후 원복)
│
├─ 저장/로드 시스템 검증
│   ├─ MR-P-003 (순환 -- 저장→로드→상태 동일)
│   └─ core MR-04 (Representation Invariance -- 포맷 변환 후 동일)
│
├─ 전투/스킬 시스템 검증
│   ├─ MR-P-005 (부정 -- 사망 시 행동 불가)
│   ├─ MR-P-006 (스케일링 -- AoE 범위↑ → 대상↑)
│   ├─ MR-P-004 (치환 불변 -- 버프 순서 무관)
│   └─ core MR-02 (Monotonicity -- 입력↑ → 출력↑)
│
├─ 맵/밸런스 검증
│   ├─ MR-P-007 (대칭 -- 미러맵 승률 대칭)
│   ├─ MR-P-001 (단조성 -- 스탯 성장)
│   └─ core MR-06 (Property/Invariant -- 밸런스 불변량)
│
└─ 물리/충돌 시스템 검증
    ├─ MR-P-006 (스케일링 -- 범위 확대 시 감지 대상 증가)
    ├─ MR-P-005 (부정 -- 비활성 오브젝트 충돌 불가)
    └─ core MR-05 (Negative Oracle -- 잘못된 입력 시 적절한 에러)
```

### Priority Matrix (Project Extension)

| 상황 | 1순위 | 2순위 | 3순위 |
|------|-------|-------|-------|
| 캐릭터 성장 공식 구현 후 | MR-P-001 | MR-P-004 | core MR-06 |
| 아이템 합성/강화 시스템 구현 후 | MR-P-002 | MR-P-001 | core MR-03 |
| 저장/로드 시스템 구현 후 | MR-P-003 | core MR-04 | core MR-06 |
| 전투 시스템(데미지/스킬) 구현 후 | MR-P-005 | MR-P-006 | MR-P-004 |
| AoE/범위 스킬 구현 후 | MR-P-006 | MR-P-001 | core MR-02 |
| 맵 밸런스 검증 시 | MR-P-007 | MR-P-006 | core MR-06 |
| 버프/디버프 시스템 구현 후 | MR-P-004 | MR-P-001 | MR-P-005 |
| 기능 전반 리팩토링 후 (regression 점검) | MR-P-003 | MR-P-001 | MR-P-005 |

---

## Counterexample Logging: Project Extension

core Section 7의 counterexample 형식을 그대로 사용하되, `relation` 필드에는 relation 정의 ID(`MR-P-XXX`) 또는 실제 실행한 relation instance ID(`MR-P-XXX-[NNN]`)를 사용한다.

### 프로젝트 특화 context 필드

```yaml
counterexample:
  # ... core 필드 동일 ...
  context:
    loop_id: "[루프 ID]"
    environment: "Unity {version}, .NET {version}, {platform}"
    unity_play_mode: "editor | standalone | device"     # 프로젝트 추가 필드
    scripting_backend: "mono | il2cpp"                   # 프로젝트 추가 필드
    game_system: "combat | economy | save_load | physics | ui"  # 프로젝트 추가 필드
    gdd_reference: "[GDD 문서 경로#섹션]"                # 프로젝트 추가 필드
    character_config: "{class, level, equipment}"         # 프로젝트 추가 필드
    related_counterexamples: ["CE-xxx"]
    related_knowledge_assets: ["KA-xxx"]
```

---

## Failed Relation -> Knowledge Asset: Project Mapping

| Relation 실패 | 가능한 Knowledge Asset Type | Checklist 연결 |
|---------------|---------------------------|---------------|
| MR-P-001 실패 | discovered_rule -- "성장 공식에 구간 불연속 존재" | VC-P-D-03 강화, FT-01 등록 |
| MR-P-002 실패 | invariant -- "합성/분해 반복 시 가치 증식 가능" | VC-P-D-03 강화, FT-01 등록 |
| MR-P-003 실패 | discovered_rule -- "직렬화 누락 필드 발견" | VC-P-V-01 강화, FT-P-001 등록 |
| MR-P-004 실패 | discovered_rule -- "버프 적용이 순서 의존적" | VC-P-D-03 강화, FT-02 등록 |
| MR-P-005 실패 | spec_gap -- "사망 상태 정의 불완전" | VC-P-D-01 trigger, FT-02 등록 |
| MR-P-006 실패 | discovered_rule -- "AoE 범위 판정에 레이어 누락" | VC-P-D-01 trigger, FT-P-003 등록 |
| MR-P-007 실패 | discovered_rule -- "맵 구조 비대칭 또는 AI 편향 존재" | VC-P-D-03 trigger, FT-10 등록 |
