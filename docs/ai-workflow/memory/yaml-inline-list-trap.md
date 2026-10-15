---
name: yaml-inline-list-trap
description: "에셋 YAML에서 「키:  - 항목」처럼 리스트 첫 항목이 키와 한 줄에 붙으면 유니티가 조용히 null 참조로 읽는다 — 09-26 무한 초월 조합"
metadata:
  node_type: memory
  type: feedback
  modified: 2026-09-26T04:13:07.157Z
---

`ingredients:  - kind: 0`처럼 리스트 첫 항목이 키와 한 줄에 붙으면, 유니티는 에러 없이 **항목 1개 · 참조 null**로 읽는다. 조합식 네 개가 09-02(aac69e98)부터 재료 없이 무한 조합됐다. 09-26에 구현담당1이 combineall 판에서 초월 360기로 발견했다. 앞서 에셋을 손으로 만들 때 m_Name 뒤 빈 줄 하나로 필드가 전부 0이 된 적도 있다.

**Why:** 텍스트로는 멀쩡해 보이고 에셋 목록·개수 검사도 통과한다(재료 개수 1 ≠ 0). 로드된 **참조 값**을 봐야 드러난다.

**How to apply:** 스크립트로 .asset을 고치거나 만들면 ① `grep -rlE "^  [a-zA-Z_]+: +- " Assets/Data`로 한 줄 붙은 리스트가 없는지 보고 ② 에디터에서 로드해 참조가 null이 아닌지 프로브로 확인한다(RecipeIngredientProbe.Check). 지금은 CombineSystem.IsBroken이 깨진 식을 막는다(e34dc236). 개수만 세는 검사는 믿지 말 것([[one-metric-is-never-enough]]).

**09-29 두 번째 모양:** 값이 따옴표 없이 `[`로 시작(`skillName: [히든조합]…`) → 흐름 목록으로 읽다 깨져 **그 뒤 필드 전부 기본값**(triggerType=쿨다운, levels 빔). 히든 3종 스킬 5개가 처음부터 한 번도 안 나갔다(be502a04). 생성기에 yaml_scalar 넣음. 판정법: 계측에 「레벨없음」이 프레임마다 찍히면 이것. 전수 검사는 `^\s*key: [\[{]` 중 정상 흐름 매핑/목록이 아닌 것.
