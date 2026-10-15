# AI 메모리 예시

Claude Code의 자동 메모리에 쌓인 140여 개 파일 중 일부입니다. 한 파일에 사실 하나를 담고, 맨 위 frontmatter의 `description`을 보고 다음 세션이 필요한 것을 불러옵니다.
`feedback` 유형은 사용자가 바로잡은 것과 그 이유(**Why**), 앞으로 적용하는 법(**How to apply**)을 함께 적습니다. 친구 이름은 「유닛A」처럼 가렸습니다.

| 파일 | 내용 |
|---|---|
| [tool-labels-are-not-evidence](tool-labels-are-not-evidence.md) | 도구가 붙인 라벨을 결과로 믿어 4번 틀렸다 |
| [docs-are-not-evidence](docs-are-not-evidence.md) | 조사 문서는 근거가 아니다 |
| [is-that-really-everything](is-that-really-everything.md) | 「전부 봤다」를 세어 보라 |
| [is-the-value-actually-used](is-the-value-actually-used.md) | 값이 이상하면 그 값이 실제로 쓰이는지부터 |
| [csc-check-tail-hides-errors](csc-check-tail-hides-errors.md) | 컴파일 검사 마지막 줄이 오류를 숨겼다 |
| [git-commit-index-race](git-commit-index-race.md) | 여러 세션이 한 저장소에 커밋할 때 |
| [generators-are-destructive](generators-are-destructive.md) | 생성 스크립트는 데이터를 쓴다 |
| [session-rotation-by-pm](session-rotation-by-pm.md) | 대화가 길어지면 PM이 스스로 세션을 교체 |
| [spawn-order-awake-cache](spawn-order-awake-cache.md) | 한 달 동안 스킬이 꺼져 있던 원인 |
| [yaml-inline-list-trap](yaml-inline-list-trap.md) | 생성한 YAML 한 줄이 스킬 5개를 껐다 |
