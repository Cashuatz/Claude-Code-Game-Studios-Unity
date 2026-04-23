# Proto Genres — Index & Router

> 수강생이 `/proto-start` 에서 한 문장 아이디어를 말하면, Claude 는 이 문서를
> 참조해 **어느 장르 번들로 진입할지** 먼저 결정한다. 장르가 정해지면 그 장르의
> `_questionnaire-<genre>.md` 로 이동하여 세부 질문을 시작한다.
>
> **다장르 대응의 핵심 원칙**:
> - 장르마다 **카탈로그·질문지·컨벤션 3종 세트** 를 독립적으로 보유한다.
> - **공유 가능한 모듈**(CameraRig, StageGraph, HUDKit 등) 은 여러 장르가 재사용.
> - 수강생은 한 번에 **하나의 장르**만 선택. 혼합(예: TD + 턴제) 은 프로토 범위 밖.

## 장르 목록

| ID | 장르 | 카탈로그 | 질문지 | 컨벤션 | 상태 |
|---|---|---|---|---|---|
| `turn3d` | 블아류 쿼터뷰 유사턴제 3D 액션 | `_catalog-turn3d.md` ✅ | `_questionnaire-turn3d.md` ✅ | `_conventions.md` (공통) | **3종 세트 완료** (전용 conventions 없음) |
| `td` | 타워 디펜스 (네트워크 허브 연결형, 엔드필드류) | `_catalog-td.md` ✅ | `_questionnaire-td.md` ✅ | `_conventions-td.md` ✅ + 공통 | **3종 세트 완료** |
| `rail-shooter` | 1인칭 건슈팅 액션 (하우스 오브 더 데드류 온-레일 슈터) | `_catalog-rail-shooter.md` ✅ | `_questionnaire-rail-shooter.md` ✅ | `_conventions-rail-shooter.md` ✅ + 공통 | **3종 세트 완료** |
| (TBD) | 추가 장르는 본 표 확장 | — | — | — | — |

## 장르 라우팅 룰 (수강생 프롬프트 → 장르 매핑)

수강생의 한 문장에 포함된 키워드/컨셉으로 장르를 자동 판정. 애매하면 역질문.

### `turn3d` 진입 조건

**키워드**: "턴제", "블루아카이브", "블아", "JRPG", "유사턴제", "전략 RPG", "캐릭터 배치 전투"
**컨셉 패턴**:
- 플레이어가 **여러 유닛** 을 조작
- **턴/라운드** 개념 명시
- 스킬/필살기 컷신 요구
- 캐릭터성 강조 (일러스트·성우·서사)

**애매한 경우 Claude 질문 예시**:
> "유닛을 여러 명 조작하고 턴마다 스킬 쓰는 느낌인가요?"

### `td` 진입 조건

**키워드**: "타워 디펜스", "TD", "디펜스", "명일방주", "엔드필드", "타워 짓", "지키는 게임", "네트워크", "연결", "전력"
**컨셉 패턴**:
- 적이 **경로를 따라 진입**, 플레이어는 **방어 구조물 배치**
- 자원 제약 하 타워 / 허브 / 선 배치
- 웨이브 구조 암시

**애매한 경우 Claude 질문 예시**:
> "적이 알아서 들어오고 저는 타워 놓고 막는 느낌인가요?"

### `rail-shooter` 진입 조건

**키워드**: "건슈팅", "슈팅", "하우스 오브 더 데드", "타임 크라이시스", "에임", "엄폐", "불릿타임", "좀비", "사격", "온레일", "1인칭 총"
**컨셉 패턴**:
- **1인칭 시점**, 플레이어 **이동은 자동/제한**, 에임만 조작
- **엄폐 / 자리 이동** 키 언급
- 적이 **플레이어에게 다가오거나 등장 위치 고정**
- "오락실 게임" / "고전 슈팅" 언급

**애매한 경우 Claude 질문 예시**:
> "시점 고정이고 마우스로 조준만 하는 방식인가요? (이동은 자동)"

### 모두 해당 안 될 때

Claude 는 수강생에게 1회 **장르 재설명 요청**:

> "만들고 싶은 게임을 조금만 더 구체적으로 말씀해 주세요. 예를 들어:
> - (1) 여러 캐릭터 턴제 전투
> - (2) 타워 짓고 방어
> - (3) 1인칭 슈팅
> 중 가장 가까운 건 어떤 거예요?"

여기서도 애매하면 `turn3d` 를 **기본 장르**로 (기존 카탈로그가 가장 성숙).

## 공유 모듈 매트릭스

여러 장르가 재사용할 수 있는 모듈 — 각 장르 카탈로그에서 **import** 로 참조.

| 모듈 | turn3d | td | rail-shooter | 장르별 차이 |
|---|---|---|---|---|
| `camera-rig` | ✅ 3모드(백/쿼터/사이드) | ✅ +TopDown/QuarterBuilder | ✅ **Fixed-Rail 모드** 추가 | 모드만 추가, 기본 API 공통 |
| `character-render-kit` | ✅ | ✅ | ✅ | 빌보드↔스킨드 추상화 동일 |
| `movement-core` | ✅ | ❌ (플레이어 이동 없음) | ✅ (고정 시점에서 자리 이동만) | rail-shooter 는 슬롯 간 점프만 |
| `stage-graph` | ✅ (노드 이동) | ✅ (적 경로) | ✅ (레일 웨이포인트) | 셋 다 노드-엣지, 해석 다름 |
| `hud-kit` | ✅ | ✅ | ✅ | 위젯 세트만 다름 |
| `sound-kit` | ✅ | ✅ | ✅ | 공통 |
| `save-kit` | ✅ | ✅ | ✅ | 공통 |
| `timeline-cue` | ✅ Intro/Ult/Outro | ❌ | ✅ 컷신/보스 등장 | rail-shooter 는 섹션 전환 연출 |
| `shader-fx-kit` | ✅ 소환/이탈 디졸브 | ✅ 건설/파괴 디졸브 | ✅ 적 등장/사망 디졸브 | URP Shader Graph 기반 공통 셰이더 라이브러리 |
| `post-process-kit` | ✅ Ult 발동 PP | ✅ 위기 경고 PP | ✅ 불릿타임/엄폐 PP | URP ScriptableRendererFeature + Volume 프로파일 |
| `path-picker` | ✅ | ❌ | ❌ | turn3d 전용 |
| `turn-system` | ✅ | ❌ | ❌ | turn3d 전용 |
| `combat-scheme` | ✅ | ❌ | ❌ | turn3d 전용 |
| `pcg-city-layout` | ✅ | ❌ | ❌ | turn3d 전용 |
| (TD 모듈들) | ❌ | ✅ | ❌ | td 전용 |
| (Rail 모듈들) | ❌ | ❌ | ✅ | rail-shooter 전용 |

## 장르 선택 후 작업 흐름

```
1. 수강생 한 문장 아이디어
   ↓
2. Claude: 장르 라우팅 (본 문서)
   ↓
3. 장르 확정 → design/proto-concept.md 에 한 줄 기록
   ↓
4. _questionnaire-<genre>.md 순서대로 질문
   ↓
5. 답 수집 완료 → _catalog-<genre>.md 의 Phase 착수 순서로 설치
   ↓
6. 각 Phase 종료마다 공통 검증 + 장르별 검증
   ↓
7. 종결 조건 충족 → 수강생에게 "플레이해보세요" 안내
```

## 신규 장르 추가 절차 (강사용)

새 장르를 추가하고 싶을 때:

1. 본 문서 장르 목록 테이블에 행 추가 (ID, 카탈로그/질문지/컨벤션 파일명).
2. 라우팅 룰 섹션에 키워드·컨셉 패턴·애매할 때 질문 추가.
3. `_catalog-<genre>.md` 작성 (모듈 번들).
4. `_questionnaire-<genre>.md` 작성 (수강생 분기 질문).
5. 필요 시 `_conventions-<genre>.md` (전용 Hard Rules).
6. 공유 모듈 매트릭스에 ✅/❌ 표기 갱신.
7. VKL proposal 필요 여부 검토 (새 failure type/oracle 필요하면 `.vkl/proposals/` 추가).

## 금지 사항

- **혼합 장르 금지**: 수강생이 "턴제 + 타워 디펜스" 같은 혼합 요청 시 → 한 장르로 정리하도록 유도. 프로토 범위 내에서 멀티장르 시도 금지.
- **라우팅 즉흥 판단 금지**: 본 문서에 없는 키워드/패턴으로 장르 결정 금지. 애매하면 재질문.
- **장르 중간 변경 제한**: 질문지 진행 중 수강생이 "아, 다른 장르로 할래" 하면 → 현재까지 수집된 답 폐기하고 처음부터 재시작. 부분 이전 금지 (번들 혼합 방지).

## 공유 VFX/셰이더 기술 참조

장르를 가로지르는 렌더링 기술. 각 장르 카탈로그에서 **레시피 ID** 로 호출.

### Dissolve (디졸브)

- **모듈**: `shader-fx-kit` 하위.
- **용도 매트릭스**:
  - turn3d: 유닛 소환/이탈, Ult 컷인 진입, 적 격파 연출
  - td: 타워 건설 등장, 허브 파괴, 적 사망
  - rail-shooter: 적 등장(벽 뚫고 나오는 느낌), 사망 잔상, 섹션 전환
- **기반 스펙**:
  - URP Shader Graph + `_DissolveNoise` (노이즈 텍스처), `_DissolveAmount` (0~1), `_EdgeColor`, `_EdgeWidth`.
  - 공통 머티리얼 프로토타입 `M_Dissolve_Base.mat` → 각 장르에서 인스턴스 복제.
  - 애니메이션은 `DOTween` 금지(외부 의존 회피), **자체 tween** `Proto.Shared.Tween.ShaderFloat` 사용.
- **레시피 ID**: `dissolve-in` / `dissolve-out` / `dissolve-edge-glow`.
- **스킬 훅**: `/proto-shader-fx --recipe dissolve-<variant>` 가 해당 머티리얼·스크립트 설치.

### Post-Processing Custom Passes (URP PP 패스)

- **모듈**: `post-process-kit` 하위.
- **용도 매트릭스**:
  - turn3d: Ult 발동 시 레이디얼 블러 + 비네트 + 채도 튕김, 피격 시 적색 비네트
  - td: 위기 경고(기지 체력 저점) 적색 펄스, 게임 오버 그레이스케일
  - rail-shooter: 불릿타임(블루톤 + 체급 대비 그레인 감소 + 속도감 블러), 엄폐 시 가장자리 블러, 피격 적색 플래시
- **기반 스펙**:
  - URP `ScriptableRendererFeature` + `ScriptableRenderPass` 커스텀 패스.
  - `VolumeProfile` 프리셋 다수 (`PP_BulletTime.asset`, `PP_UltActive.asset`, `PP_LowHP.asset`, `PP_GameOver.asset`).
  - 전역 가중치 블렌드 `PostProcessController.SetWeight(profileId, weight, duration)` API 일원화.
  - 임의 스크립트가 `Volume.weight` 직접 조작 금지 — 반드시 컨트롤러 경유 (HR-9 이벤트 버스 원칙 연장).
- **레시피 ID** (장르 공용):
  - `pp-bullettime` — 블루톤 + 속도선 블러 + 채도 감소 (rail-shooter 불릿타임)
  - `pp-ult-active` — 레이디얼 블러 + 비네트 + 채도 튕김 (turn3d Ult 발동)
  - `pp-low-hp` — 적색 펄스 + 비네트 (td 위기, rail-shooter 저HP)
  - `pp-hit-flash` — 단발 적색 플래시 (rail-shooter 피격)
  - `pp-grayscale-over` — 그레이스케일 페이드 (공용 게임오버)
- **레시피 ID** (rail-shooter 테마 전용):
  - `pp-horror` — 녹색/청록 색조 + 가장자리 어둠 + 필름 그레인 (Q7-A 좀비)
  - `pp-cyber` — 네온 톤 + 스캔라인 + 크로매틱 어버레이션 (Q7-B 로봇)
  - `pp-weird` — 색상 왜곡 + 낮은 대비 + 블러 (Q7-D 크리처)
- **레시피 ID** (turn3d 테마 전용):
  - `pp-pastel` — 밝은 채도 + 소프트 블룸 + 따뜻한 화이트 밸런스 (Q8-A 파스텔 애니)
  - `pp-dark` — 낮은 채도 + 진한 비네트 + 차가운 그림자 (Q8-B 다크 판타지)
  - `pp-retro` — 픽셀화 / 다운샘플 + 제한 팔레트 + 디더링 (Q8-C 레트로)
- **스킬 훅**: `/proto-postfx --recipe pp-<variant>` 가 프로파일/패스/컨트롤러 설치.

### 공유 VFX Hard Rules (신규 HR 후보)

아래는 `_conventions.md` 에 **HR-12, HR-13** 으로 승격 제안 중 (강사 검토 대기):

- **HR-12 (제안)**: URP Shader Graph 외 Surface Shader / built-in 셰이더 작성 금지. 모든 커스텀 셰이더는 Shader Graph 소스로 버전 관리.
- **HR-13 (제안)**: Post-Processing 효과는 반드시 `PostProcessController` 경유. 스크립트가 `Volume` / `VolumeProfile` 을 직접 수정 금지.

→ 정식 승격까지는 장르 카탈로그에서 **소프트 룰** 로 표기.

### 아트 파이프라인 주의

- 수강생이 Shader Graph 편집을 직접 할 일은 거의 없음. **프리셋 조합만 노출**.
- 디졸브 텍스처 / 노이즈 텍스처는 `Assets/Proto/VFX/Shared/Textures/` 에 사전 배치.
- 커스텀 PP 패스는 Forward + HDR 환경 가정. URP 설정 변경 시 재검증 필요.

## 참조

- 공통 컨벤션: `_conventions.md` (HR-1 ~ HR-11, HR-12/13 제안)
- turn3d 카탈로그: `_catalog-turn3d.md`
- turn3d 질문지: `_questionnaire-turn3d.md` (**미작성** — 기존 `_catalog-turn3d.md` 는 질문지 없이 고정 스펙)
- TD 컨벤션: `_conventions-td.md` (HR-TD-1 ~ HR-TD-10)
- 공유 VFX: 본 문서 "공유 VFX/셰이더 기술 참조" 섹션
- VKL 브리지: `.claude/docs/vkl-bridge.md`
