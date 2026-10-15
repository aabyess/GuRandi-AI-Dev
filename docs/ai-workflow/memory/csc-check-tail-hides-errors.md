---
name: csc-check-tail-hides-errors
description: csc_check.sh는 오류를 위에 찍고 마지막 줄은 늘 「csc_check done」 — tail -1로 보면 통과로 착각한다
metadata:
  node_type: memory
  type: feedback
  modified: 2026-10-06T01:43:13.162Z
---

`Tools/ui/csc_check.sh`(와 `csc_check_editor.sh`)는 error 줄을 먼저 찍고 **마지막 줄은 항상 「csc_check done」**이다. 종료 코드도 0.
2026-10-06 PM이 `| tail -1`로 보고 「통과」라 믿고 if 중괄호 빠진 코드(CS1023)를 커밋·푸시 → 에디터가 컴파일 오류로 모든 팀원의 gameshot을 거절했다(구현담당3이 발견·수정 6607c309e).

**Why:** 출력 형식상 성공과 실패의 마지막 줄이 같다.
**How to apply:** 결과는 `| grep -c error` 또는 출력 전체로 본다. 0줄이어야 통과. [[pm-operating-rules]]
