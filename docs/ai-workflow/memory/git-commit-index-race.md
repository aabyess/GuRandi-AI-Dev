---
name: git-commit-index-race
description: 여러 세션이 한 저장소를 쓸 때 git add 뒤 git commit은 남의 스테이징까지 삼킨다
metadata:
  type: feedback
---

`git add <파일>` 뒤 `git commit`은 **인덱스 전체**를 커밋한다. 파일을 하나씩 add해도
소용없다 — 인덱스는 세션끼리 공유되므로, 다른 세션이 같은 순간에 스테이징해두면
그것까지 같이 들어간다. 반드시 **pathspec 형태**로 커밋할 것:

```
git commit -m "..." -- 파일1 파일2 파일3
```

이러면 지정한 경로만 커밋되고 남의 스테이징은 인덱스에 남는다.
커밋 직전 `git diff --cached --name-only`로 확인하는 습관도 같이.

**Why:** 2026-09-02에 실제로 발생했다. PM의 도박소 수정 커밋(`809965a`)이
구현담당1의 `RoundManager.cs`·`Traits.meta`·`MapGenerator.cs` 변경을 통째로 삼켰다.
작업은 안 유실됐지만 커밋 메시지가 사라졌고, 나중에 이력을 읽는 사람이 그 변경이
왜 들어갔는지 알 수 없게 됐다. 이전에 "`git add <디렉터리>` 금지" 규칙을 만들었는데
그것만으로는 이 사고를 못 막는다.

**How to apply:** 팀 세션 전원에게 적용. `git add`는 확인용으로만 쓰고,
커밋은 항상 `--` 뒤에 파일을 나열한다. [[team-session-setup]]

**09-25 추가 — 새 파일은 `git commit -- <경로>`에 안 담긴다.** 배선이 새로 만든 Mob_ 프리팹·Art/Enemies 새 폴더가
두 번 빠졌다(b674cf34·a3a3900e로 뒤따라 커밋). 원인 둘: ① `commit -- 경로`는 추적 안 되던 파일을 무시한다
② `git add a b c`는 **없는 경로가 하나라도 섞이면 전부 실패**한다(2>/dev/null로 숨겨서 몰랐다).
→ 새 산출물은 **폴더 단위로 add**하고, 커밋 전 `git status --untracked-files=all <폴더>`에 `??`가 0인지 본다.

**09-29 추가 — 남의 미커밋 수정이 섞인 파일:** `git commit -- 파일`은 파일 전체를 올리므로 남의 작업까지 딸려 간다.
내 hunk만 patch로 떼고 → `GIT_INDEX_FILE=<scratch>/idx git read-tree HEAD` → `git apply --cached 내것.patch` → 커밋 → 공유 인덱스는 `git reset -q -- 그 경로들`로만 맞춘다(b6e7f8d9에서 GameHud.cs에 씀).

**🔴 예외(10-02):** 파일의 **일부만** 올릴 때(씬에서 URP 조명 데이터 빼기 등)는 `git commit -- <파일>`을 쓰면 안 된다 —
경로를 주면 인덱스가 아니라 **작업 파일 내용**을 커밋해서 걸러 둔 게 도로 들어간다(정의문 커밋에 조명 13개 섞임 → amend).
걸러서 `git update-index --cacheinfo`로 올린 뒤엔 경로 없이 `git commit`(인덱스에 그 파일들만 있는지 `git diff --cached --name-only`로 먼저 확인).
