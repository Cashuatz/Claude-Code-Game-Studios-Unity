# Proto — Unity 프로토타이핑 경량 브랜치

> ⚠️ **이 브랜치는 Claude Code Game Studios(CCGS) Unity 포크의 프로토타이핑 경량 버전입니다.**
> 20시간 이내 / 3분 플레이 가능한 Unity 빌드를 만드는 데 최적화되었습니다.

## 바로 시작

1. Claude Code에서 이 프로젝트를 엽니다.
2. **첫 메시지에 한 문장으로 아이디어를 말합니다** — 예: "좀비 피해서 3분 버티는 게임". Claude 가 바로 받아 컨셉 정리와 시작 스크립트까지 만듭니다.
3. Unity Hub에서 이 프로젝트를 열고 **Play** 버튼을 누릅니다.

전체 가이드: [`docs/PROTO-QUICKSTART.ko.md`](docs/PROTO-QUICKSTART.ko.md)

## Proto vs main 차이

| 항목 | Proto (이 브랜치) | main (정식 CCGS) |
|------|------------------|------------------|
| 노출된 스킬 | 12개 | 82개 |
| 노출된 에이전트 | 9개 | 41개 |
| GDD/ADR 강제 | ❌ | ✅ |
| 디렉터 게이트 | ❌ | ✅ |
| VKL 검증 루프 | ✅ (`/verify`) | ✅ |
| "May I write?" 승인 | ❌ | ✅ |
| Git 자동 대행 | ✅ | ❌ (사용자 수동) |
| 대상 사용자 | 비개발자 포함 | 엔지니어 |
| 목표 시간 | 20시간 / 3분 빌드 | 제한 없음 |

## main으로 돌아가려면

```bash
git checkout main
```

숨긴 스킬·에이전트는 자동 복원됩니다.

## 구성

- **Unity**: 6.x LTS, URP, 3D, Input System, Unity MCP
- **워크스페이스**: `Assets/Scripts/Proto/` 하위에서 작업
- **브랜치**: `Proto` (이 브랜치에서만 커밋)

## 문서

- [`CLAUDE.md`](CLAUDE.md) — Claude 작업 규칙
- [`docs/PROTO-QUICKSTART.ko.md`](docs/PROTO-QUICKSTART.ko.md) — 1페이지 사용자 가이드
- [`docs/templates/proto-game-concept.md`](docs/templates/proto-game-concept.md) — 컨셉 템플릿
- [`.internal/README.full.md`](.internal/README.full.md) — main 브랜치 전체 문서 (참고용)
- [`.internal/UPGRADING.md`](.internal/UPGRADING.md) — 템플릿 업그레이드 가이드 (참고용)

## 라이선스

MIT — 자세한 내용은 [`LICENSE`](LICENSE) 참조.
