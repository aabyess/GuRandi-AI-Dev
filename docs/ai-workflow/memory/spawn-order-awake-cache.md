---
name: spawn-order-awake-cache
description: ⚠️ 유닛 프리팹엔 OwnedByPlayer·UnitIdentity가 없다 — Awake에서 캐시하면 영원히 null. 08-28부터 09-29까지 등급강화·스킬·특성 전부 꺼져 있었다
metadata:
  node_type: memory
  type: project
  modified: 2026-09-29T07:57:21.619Z
---

2026-09-29 감사: 유닛 프리팹 214개 전부 OwnedByPlayer·UnitIdentity 0개. UnitSpawner.Spawn이 Instantiate **뒤에** AddComponent한다(08-28 첫 소환부터). UnitAttacker는 owner(08-29~)·identity(09-02~)를 Awake에서 캐시해서 처음부터 null이었다 → 연구·강화소·특성 딜증가·스킬·피해타입·자가강화 전부 무효, 처치자 번호 −1(노루·양 크립 보상 소실). PM이 UnitAttacker를, 구현담당2가 UnitMover를 지연 조회 프로퍼티로 고쳤다(6b944848). 09-24~26 밸런스 판은 전부 이 버그 위였다([[balance-2026-09-26]]).

**Why:** Awake는 Instantiate 그 자리에서 돌고, 스포너의 AddComponent/SetData/SetOwner는 그 뒤다.
**How to apply:** 유닛에 붙는 컴포넌트에서 OwnedByPlayer·UnitIdentity는 Awake 캐시 금지. 붙을 때까지 다시 찾는 프로퍼티로 쓴다. 상점(씬 직렬화)·위습(프리팹에 있음)·멀티 거울(비활성 부모)은 안전하다. [[is-the-value-actually-used]]
