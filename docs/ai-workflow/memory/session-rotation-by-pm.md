---
name: session-rotation-by-pm
description: 10-09 사장님: 대화 기록이 길어졌다고 판단되면 PM이 알아서 팀원·PM 세션을 새로 열어 진행(승인 불필요)
metadata:
  type: feedback
---

사장님 10-09: 「너가 이제 판단해서 세션 대화 기록 길어진 거 같으면 새 세션 열어서 진행해, pm도 마찬가지로」. CLAUDE.md의 「팀원 갈아 끼우기는 사장님께 먼저 말하고 승인」을 대체 — **팀원도 PM 판단으로 교체**, 사장님께는 한 줄 보고.

**Why:** 토큰 비용(대화가 길수록 한 번 대답에 드는 토큰이 늘어남).

**How to apply:** 팀원은 큰 일 하나 끝낸 뒤(일하는 중엔 미룸) handoff_<역할>.md 정리·커밋 → `spawn_team.sh --fresh <역할>`. PM은 NEXT_SESSION 갱신·커밋·푸시 → `spawn_pm.sh`. 임시 세션(blender2·blender3)은 일 끝나면 닫고 다시 만들지 않는다. [[team-model-policy]] [[temp-sessions-allowed]]
