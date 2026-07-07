# 이미지 그리드 생성 벤치마크 종합

Date: 2026-07-07

## 요약

이 문서는 Memojang 디자인 세션에서 진행한 이미지 그리드 생성 실험과 관련 스킬 정리를 종합한다. 핵심 결론은 표면적으로는 모두 "그리드 이미지"처럼 보여도, 목적별로 평가 기준을 분리해야 한다는 점이다.

- 아이콘 스프라이트는 exact-cell crop, 투명 배경 처리, 작은 크기 가독성을 본다.
- 캐릭터 일관성 그리드는 동일 인물/오브젝트 유지, 의상/비율/스타일 연속성을 본다.
- 장면/스토리보드 그리드는 컷 연속성, 인물/배경 유지, 장면 흐름을 본다.
- 다중 이미지 프롬프트 이행 그리드는 각 칸이 자기 프롬프트의 배경/인물/구도/스타일을 지켰는지 본다.

가장 중요한 프로세스 교훈은 source fidelity다. 사용자가 기존 생성 시트나 후보 번호를 제공하면, 그 정확한 원본을 복사하고 잘라야 한다. 비슷하게 다시 그린 이미지를 쓰는 것은 결과가 좋아 보여도 실패다.

## 스킬 분류

| 작업 | 스킬 | 핵심 질문 | 기본 그리드 | 주요 실패 모드 |
|---|---|---|---|---|
| 프로덕션 아이콘 추출 | `production-icon-sprite` | 투명 PNG/ICO로 쓸 수 있게 정확히 분할됐는가? | `2x2` | 제공 원본 대신 재구성, 작은 크기 가독성 실패 |
| 아이콘 배치 한계 벤치 | `icon-sprite-bench-2x2/3x3/5x5` | 한 장에 몇 개 아이콘까지 유지되는가? | `2x2` 프로덕션, `3x3` 탐색, `5x5` 스트레스 | 아이콘 누락/중복, 셀 경계 침범, 20px 가독성 실패 |
| 캐릭터/오브젝트 일관성 | `visual-grid-consistency-bench` | 모든 칸이 같은 대상처럼 보이는가? | `2x2`; `3x3/4x4` 스트레스 | 정체성 드리프트, 의상/세부 디테일 변화, 비율 흔들림 |
| 장면/스토리보드 그리드 | `scene-sprite-grid-bench` | 컷 흐름과 장면 연속성이 유지되는가? | `2x2` 기본, `3x3` 스트레스 | 패널 병합, 인물/배경 드리프트, 장면 흐름 불명확 |
| 다중 이미지 프롬프트 이행 | `multi-image-prompt-adherence-bench` | 각 칸이 자기 프롬프트를 따르는가? | `2x2`, `3x3`, `4x4` | 프롬프트 스와핑, 스타일 누수, 텍스트 생성, 세부 조건 누락 |

## 실험 결과

### 1. 앱 아이콘 스프라이트

초기 2x2 앱 아이콘 후보에서는 네 개의 3D Memojang 방향이 나왔다. 선택된 방향은 privacy-document 후보였지만, 중간에 중요한 프로세스 오류가 있었다. 제공된 생성 시트에서 3번을 정확히 잘라야 했는데, 비슷하게 로컬에서 재구성한 이미지가 먼저 적용됐다. 이후 제공된 원본 시트를 authoritative source로 삼아 3번 후보를 exact crop으로 교체했다.

근거:

- 원본 시트: `artifacts/app-icons/memojang-3d-original-2x2/memojang-original-generated-2x2-source.png`
- 선택 crop: `artifacts/app-icons/memojang-3d-original-2x2/split/03-privacy-document.png`
- 검증 프리뷰: `artifacts/app-icons/memojang-3d-original-2x2/selected-03-validation-preview.png`
- 앱 자산: `src/Memojang.App/Assets/Memojang.ico`

교훈:

- 시트에서 후보 번호를 고르면 반드시 그 시트의 해당 셀을 자른다.
- alpha/corner 검증은 필요하지만 충분하지 않다. 선택 셀과 최종 이미지가 시각적으로 일치하는지도 봐야 한다.
- Windows 앱 아이콘은 `.ico`에 `16, 20, 24, 32, 48, 64, 128, 256px` 크기를 포함하는 것이 좋다.

### 2. 캐릭터 일관성 그리드

탐험가 캐릭터 실험은 같은 캐릭터를 여러 포즈와 그리드 크기로 생성했다. `2x2`는 매우 안정적으로 통과했다. `3x3`도 강했지만 하의/다리 노출에서 약한 드리프트가 보였다. `4x4`는 스트레스 테스트답게 정체성은 유지되지만, 다리/양말, crop/framing, 소품 크기, 패널별 디테일 밀도에서 흔들림이 커졌다.

| 그리드 | 점수 | 판정 | 주요 관찰 |
|---|---:|---|---|
| `2x2` | `25/25` | Pass | 정체성, 의상, 포즈 구분, 그리드 분리 모두 안정 |
| `3x3` | `24/25` | Exploration stress pass | 일부 패널에서 하의/다리 노출 드리프트 |
| `4x4` | `22/25` | Stress pass, production-default 아님 | 다리/양말, framing, 소품 크기, 디테일 밀도 흔들림 증가 |

근거:

- `artifacts/visual-grid-consistency-bench/explorer-character-2x2-consistency.png`
- `artifacts/visual-grid-consistency-bench/explorer-character-3x3-consistency.png`
- `artifacts/visual-grid-consistency-bench/explorer-character-4x4-stress.png`
- `artifacts/visual-grid-consistency-bench/explorer-character-2x2-report.md`
- `artifacts/visual-grid-consistency-bench/explorer-character-3x3-4x4-report.md`

스트레스 결과에서 나온 프롬프트 보강안:

```text
black leggings visible in every full-body panel, no bare legs,
consistent boot cuff height, keep character fully inside each panel
```

### 3. 다중 이미지 프롬프트 이행

다중 이미지 프롬프트 이행 실험은 각 칸이 서로 다른 이미지를 생성하도록 하고, 프롬프트 밀도와 그리드 크기를 함께 비교했다.

| 그리드 / 밀도 | 점수 | 판정 | 주요 관찰 |
|---|---:|---|---|
| `2x2 short` | `25/25` | Pass | 짧은 프롬프트는 거의 완벽하게 이행 |
| `3x3 medium` | `22/25` | Pass | 배경/주제 유지 강함; 1번 셀에 텍스트성 간판 발생 |
| `4x4 long` | `22/25` | Stress pass | 주요 주제는 유지; 스타일 차이가 압축되고 텍스트 회피 실패 |

근거:

- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-2x2-short.png`
- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-3x3-medium.png`
- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-4x4-long.png`
- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-2x2-3x3-4x4-report.md`

관찰된 누락 순서:

- 큰 주제/배경보다 텍스트 회피 조건이 먼저 깨진다.
- 핵심 인물/배경보다 세밀한 스타일 차이가 먼저 흐려진다.
- 큰 그리드에서도 고수준 개념은 꽤 유지되지만, production-safe로 보려면 각 셀 검토가 필요하다.

## 그리드별 실무 가이드

### 2x2

품질이 중요할 때 기본값으로 쓴다.

- 최종 아이콘 후보.
- 캐릭터/오브젝트 4변형.
- 프롬프트 이행 baseline.
- 수동 보정 가능성을 최소화해야 하는 자산.

### 3x3

탐색용으로 쓴다.

- 중간 부하의 프롬프트 유지력 테스트.
- 9개 컨셉 또는 9개 포즈 비교.
- 프롬프트를 더 조이기 전에 드리프트 패턴 관찰.

각 셀을 검토하기 전에는 production-safe로 가정하지 않는다.

### 4x4

고부하 스트레스 테스트로 쓴다.

- 세밀한 스타일 조건.
- 텍스트 금지 같은 negative constraint.
- 의상/세부 디테일 일관성.
- 정확한 구도 조건.

성공적인 4x4 결과도 기본 프로덕션 워크플로우가 아니라 모델 강건성 증거로 본다.

## 평가 루브릭

### 아이콘 스프라이트

확인 항목:

- 경계 제어.
- 스타일 일관성.
- 주제 충실도.
- `32px`, `24px`, `20px` 작은 크기 가독성.
- exact-cell crop 이후 crop 안정성.
- 투명 PNG corner alpha.
- 제공 원본이 있을 경우 선택 셀과 시각적으로 일치하는지.

### 캐릭터/오브젝트 일관성

0-5점으로 평가:

- 정체성 유지.
- 디자인 연속성.
- 변형 명확성.
- 스타일 일관성.
- 그리드 신뢰성.

### 장면/스토리보드

0-5점으로 평가:

- 캐릭터 연속성.
- 배경/소품 연속성.
- 패널 분리.
- 서사 가독성.
- 스타일 일관성.

### 다중 이미지 프롬프트 이행

0-5점으로 평가:

- 배경 이행.
- 인물/주제 이행.
- 구도 이행.
- 이미지 스타일 이행.
- 그리드 분리.

## 프롬프트 권장사항

아이콘 스프라이트:

- 최종 자산은 `2x2`를 우선한다.
- 투명 배경이 필요하면 flat chroma-key 배경을 요구한다.
- 주제는 단순하고 셀 안에서 분리되게 한다.
- 투명 처리 전에 exact cell crop을 먼저 한다.

캐릭터 일관성:

- 머리, 안경, 의상, 핵심 액세서리, 색상 같은 identity anchor를 반복한다.
- 큰 그리드에서는 변하지 않아야 하는 의상 디테일을 명시한다.
- `3x3`, `4x4`는 모든 패널을 검토하기 전까지 스트레스 결과로 본다.

다중 이미지 프롬프트 이행:

- 짧은 프롬프트로 주제/배경 baseline을 먼저 본다.
- 중간 프롬프트로 배경, 인물, 구도, 스타일 균형을 본다.
- 긴 프롬프트로 어떤 조건이 먼저 누락되는지 본다.
- 텍스트 회피는 더 강하게 쓴다: "No signs, no letters, no numbers, no legible symbols anywhere in any panel."

## 스킬 변경 사항

이번 세션에서 생성 또는 수정된 스킬:

- `C:\Users\user\.agents\skills\production-icon-sprite\SKILL.md`
- `C:\Users\user\.agents\skills\visual-grid-consistency-bench\SKILL.md`
- `C:\Users\user\.agents\skills\scene-sprite-grid-bench\SKILL.md`
- `C:\Users\user\.agents\skills\multi-image-prompt-adherence-bench\SKILL.md`
- `C:\Users\user\.agents\skills\icon-sprite-bench-2x2\SKILL.md`
- `C:\Users\user\.agents\skills\icon-sprite-bench-3x3\SKILL.md`
- `C:\Users\user\.agents\skills\icon-sprite-bench-5x5\SKILL.md`

가장 중요한 추가 규칙:

```text
If the user provides a sprite sheet, screenshot, clipboard image path,
or generated image path, that exact file is the authoritative source.
Do not regenerate, redraw, approximate, trace, or make a lookalike.
```

이 규칙은 선택된 생성 후보를 비슷하게 재구성한 실패를 막기 위해 추가했다. 제공 원본이 있으면 결과물은 반드시 그 원본에서 추적 가능해야 한다.

## 다음 실험 제안

1. 같은 9개 컨셉으로 short, medium, long 밀도만 바꿔 테스트해 grid size와 prompt density 효과를 분리한다.
2. 아이콘이 아닌 visual grid용 deterministic crop helper를 만들어 per-cell 리포트를 자동화한다.
3. `scene-sprite-grid-bench`로 2x2 액션 시퀀스와 3x3 장면 스트레스를 실제 생성해본다.
4. 리포트에 비교 썸네일을 자동 삽입한다.
5. sheet-level 점수뿐 아니라 cell-level miss를 기록하는 scoring template을 만든다.

## 근거 아티팩트

이 문서는 로컬 세션 산출물, 생성 이미지, 스킬 파일을 근거로 작성했다. 외부 웹 소스는 사용하지 않았다.

주요 근거:

- `artifacts/app-icons/memojang-3d-2x2/bench-report.md`
- `artifacts/app-icons/memojang-3d-original-2x2/selected-03-validation-preview.png`
- `artifacts/visual-grid-consistency-bench/explorer-character-2x2-report.md`
- `artifacts/visual-grid-consistency-bench/explorer-character-3x3-4x4-report.md`
- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-2x2-3x3-4x4-report.md`

생성 이미지:

- `artifacts/visual-grid-consistency-bench/explorer-character-2x2-consistency.png`
- `artifacts/visual-grid-consistency-bench/explorer-character-3x3-consistency.png`
- `artifacts/visual-grid-consistency-bench/explorer-character-4x4-stress.png`
- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-2x2-short.png`
- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-3x3-medium.png`
- `artifacts/multi-image-prompt-adherence-bench/prompt-adherence-4x4-long.png`
