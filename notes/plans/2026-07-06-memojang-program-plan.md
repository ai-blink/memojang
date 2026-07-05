# Memojang 프로그램 계획서

작성일: 2026-07-06
상태: 구현 전 계획 초안
권장 스택: .NET 10 WPF

## 1. 한 줄 목표

Windows 11 메모장에 익숙한 사용자가 거의 같은 감각으로 빠르게 메모를 열고 편집하면서, Markdown과 Markdown 표를 보기 좋게 다루고, 필요할 때 화면 캡처에서 창 내용을 제외할 수 있는 Windows 11 전용 문서 편집기 Memojang을 만든다.

## 2. 문제

Windows 11 메모장은 가볍고 익숙하지만 Markdown 표 작성, 보기 좋은 렌더링, 문서 편집용 도구 모음, 보안용 캡처 제외 같은 요구를 한 번에 만족시키기 어렵다.

근거와 가정:

| 구분 | 내용 |
|---|---|
| 관찰 | 최신 Notepad는 Markdown, 경량 서식, 맞춤법, AI 기능까지 확장되고 있으나 단순 편집기 정체성을 유지하려는 긴장이 있다. |
| 사용자 요구 | "윈도우11 메모장과 똑같은 UI/기능/거의 클론" + Markdown/표 + 캡처 제외 + 약간의 문서 편집 툴바. |
| 핵심 막힘 | 일반 메모장 감각을 잃지 않으면서 Markdown 표를 예쁘게 보고 수정하는 흐름이 없다. |
| 안 풀면 손해 | Electron류 무거운 편집기나 Notepad++류 개발자 편집기로 넘어가야 하며, 보안 캡처 제외 요구는 별도 앱에서 다시 해결해야 한다. |

## 3. 사용자와 맥락

대표 사용자는 Windows 11에서 빠른 메모, README, 회의록, 간단한 표 문서를 자주 다루는 개인 사용자 또는 내부 업무 사용자다.

사용 환경:

| 항목 | 전제 |
|---|---|
| OS | Windows 11 우선, Windows 10 2004 이상은 기술적으로 가능하나 V1 공식 목표는 Windows 11 |
| 입력 | 키보드 중심, 마우스/터치 보조 |
| 문서 | `.txt`, `.md`, `.markdown`, `.log`, 일반 확장자 없는 텍스트 |
| 보안 | 앱 창 단위 캡처 제외 토글. 물리 카메라나 모든 원격/비표준 캡처까지 보장하지는 않음 |
| 배포 | 단일 사용자 설치, MSIX 또는 self-contained installer 후보 |

## 4. 사용자 여정

| 단계 | 사용자 행동 | 생각/감정 | 막히는 점 | 개선 아이디어 |
|---|---|---|---|---|
| 1 | 파일을 열거나 새 탭을 만든다 | 메모장처럼 바로 쓰고 싶다 | 앱이 무겁거나 낯설면 이탈 | Windows 11 Notepad와 유사한 창, 메뉴, 탭, 상태바 |
| 2 | Markdown 문서를 편집한다 | 문법은 알지만 결과도 보고 싶다 | **표와 서식이 소스만으로는 보기 어렵다** | 편집/미리보기/표 도우미를 한 흐름으로 제공 |
| 3 | 문서 편집 툴바를 쓴다 | Word처럼 크면 부담스럽다 | 기능이 과하면 메모장 감각이 사라짐 | 굵게, 기울임, 제목, 목록, 링크, 표 정도만 |
| 4 | 민감한 메모를 다룬다 | 화면 공유나 캡처에 안 나왔으면 한다 | 캡처 차단이 앱마다 불확실함 | 개인정보 보호 모드 토글과 상태 표시 |
| 5 | 저장하고 닫는다 | 내가 예상한 방식으로 저장되어야 한다 | 인코딩/줄끝/복구가 흐트러지면 불신 | 인코딩/줄끝 표시, 저장 전 변경 감지, 세션 복원 옵션 |

끝난 뒤 사용자는 별도 무거운 에디터를 열지 않고 Memojang 안에서 간단한 Markdown 문서를 작성, 검토, 저장한다.

## 5. 핵심 흐름

1. 앱 실행: 마지막 세션 복원 또는 빈 새 문서로 시작한다.
2. 파일 열기/새 탭: 일반 텍스트 파일은 즉시 편집, Markdown 파일은 Markdown 툴바와 보기 모드를 활성화한다.
3. 편집: 기본은 Notepad와 같은 단순 텍스트 편집이며, Markdown 도구 버튼은 선택 영역에 문법을 삽입하거나 토글한다.
4. 표 편집: 표 영역에서 "표 편집"을 누르면 행/열 추가, 정렬, 너비 맞춤, CSV 붙여넣기 변환을 underlying Markdown 표로 반영한다.
5. 보기 전환: 원문, 분할 미리보기, 읽기 보기 중 하나를 선택한다.
6. 보안 토글: 개인정보 보호 모드를 켜면 현재 최상위 창에 캡처 제외 플래그를 적용하고 상태바에 표시한다.
7. 저장/종료: 인코딩, 줄끝, 변경 상태를 보존하고 실패 시 복구 가능한 임시 저장본을 남긴다.

실패와 재진입:

| 상황 | 동작 |
|---|---|
| Markdown 파싱 실패 | 원문 편집은 유지하고 미리보기 영역에 오류 위치를 표시 |
| 캡처 제외 API 실패 | 상태바에 실패 사유 표시, 앱 사용은 계속 |
| WebView2 런타임 없음 | 원문 편집과 표 도우미는 유지, 미리보기만 비활성화 또는 설치 안내 |
| 큰 파일 열기 | Markdown 렌더링 지연, 원문 편집 우선 |

## 6. 화면 / 출력 의도

| 화면 | 핵심 행동 | 1순위 정보 | 2순위 정보 | 헷갈릴 점 방지 |
|---|---|---|---|---|
| 메인 편집 화면 | 텍스트 입력 | 본문 | 탭, 제목, 저장 상태 | Notepad처럼 빈 공간이 곧 작업 공간이어야 함 |
| Markdown 툴바 | 서식 삽입 | 굵게/기울임/제목/목록/링크/표 | 원문/분할/읽기 보기 | 버튼 수를 제한해 리본 UI처럼 보이지 않게 함 |
| 표 편집 패널 | 행/열 수정 | 현재 표 미리보기 | 정렬, 행/열 추가/삭제 | 스프레드시트가 아니라 Markdown 표 보조임을 명확히 함 |
| 상태바 | 문서 상태 확인 | 줄/열, 인코딩, 줄끝 | Markdown 모드, 캡처 보호 상태 | 클릭 가능한 상태 항목은 메뉴와 동일한 동작 |
| 설정 | 앱 행동 조정 | 테마, 세션 복원, 보안 기본값 | 글꼴, 탭 크기, 줄바꿈 | 기본값은 Windows 11 Notepad와 유사하게 |

## 7. 우선순위

| 구분 | 내용 |
|---|---|
| **안 함** | 1. Notepad 이름/아이콘/상표/리소스 복제는 하지 않음. 법적/브랜드 리스크가 큼; 2. V1에서 협업, 클라우드 동기화, AI 작성 기능은 제외. 메모장 클론의 가벼움이 먼저임; 3. V1에서 완전한 Word/Excel식 문서 편집기는 만들지 않음. Markdown 중심 문서 보조까지만 |
| Must | Windows 11 Notepad에 가까운 탭, 메뉴, 편집, 찾기/바꾸기, 저장, 인코딩/줄끝, 상태바 |
| Must | Markdown 파일의 원문 편집, 예쁜 미리보기, 표 렌더링, 표 편집 도우미 |
| Must | 창 단위 캡처 제외 토글과 실패 표시 |
| Should | Windows 11 라이트/다크/시스템 테마, Mica/Fluent 느낌, 단축키 호환 |
| Should | autosave/session restore를 사용자가 켜고 끌 수 있게 함 |
| Could | 맞춤법, 인쇄, 최근 파일, portable 모드, 플러그인 없는 간단한 확장 설정 |

## 8. 성공 지표

| 문제와 짝이 되는 지표 | 사용자 관측 가능한 완료 판정 |
|---|---|
| Markdown 표가 보기 어렵다 | 사용자가 `.md` 표 문서를 열어 원문을 망가뜨리지 않고 행/열을 추가한 뒤, 분할/읽기 보기에서 정렬된 표를 확인하고 저장할 수 있다. |
| 메모장 감각이 사라진다 | 사용자가 새 파일 작성, 열기, 저장, 찾기, 바꾸기, 줄바꿈, 테마 전환을 Windows 11 Notepad와 같은 기대 순서로 수행한다. |
| 캡처에 민감한 내용이 노출된다 | 개인정보 보호 모드를 켠 창이 Windows Snipping Tool, Xbox Game Bar/화면 녹화, OBS Window Capture, Recall 스냅샷 검증에서 내용이 제외되거나 빈 영역으로 처리된다. |

성공으로 보지 않는 프록시:

| 프록시 | 이유 |
|---|---|
| 빌드 성공 | 실제 캡처 제외나 Markdown 편집 경험을 증명하지 않음 |
| 렌더링 라이브러리 도입 | 사용자가 표를 안전하게 수정할 수 있는지 증명하지 않음 |
| Notepad처럼 보이는 스크린샷 | 메뉴/단축키/저장/복구 흐름의 실제 사용성을 증명하지 않음 |

## 9. 검증 계획

| 항목 | 방법 |
|---|---|
| 담당 | 구현자 1차, 사용자 수동 검수 2차 |
| 시점 | V1 기능 슬라이스마다, 특히 Markdown 표/캡처 제외 완료 시 |
| 수동 검증 | Windows 11에서 실제 Notepad와 나란히 두고 메뉴, 탭, 상태바, 단축키, 저장 흐름 비교 |
| 보안 검증 | 보호 모드 on/off 상태에서 Snipping Tool, OBS, Teams/Zoom 공유, Recall 가능 환경을 캡처 테스트 |
| Markdown 검증 | GFM 표, 링크, 목록, 코드 블록, 한글/영문 혼합, CRLF/LF, UTF-8/UTF-16 파일 샘플 |
| Plan B | WPF 미리보기 안정성이 낮으면 WebView2 미리보기를 읽기 보기 전용으로 제한 |
| Plan C | 캡처 제외가 특정 캡처 앱에서 실패하면 "보호 미보장" 표시와 앱/Windows 빌드별 호환성 표를 제공 |

## 10. 접근법 비교

| 접근 | 핵심 아이디어 | 장점 | 단점/리스크 | 복잡도 | 판단 |
|---|---|---|---|---|---|
| A. .NET 10 WPF 네이티브 중심 | WPF 창/메뉴/편집기 + Markdig 미리보기 + 표 도우미 | Windows 11 감각, Win32 API 직접 접근, 배포/성능 유리 | 완전 WYSIWYG Markdown은 직접 설계 필요 | 중 | 추천 |
| B. Electron | Web 기반 에디터와 Electron 창 | Markdown WYSIWYG/표 라이브러리 활용이 빠름, 크로스 플랫폼 | Chromium/Node 포함으로 무겁고, 보안 업데이트 부담, 네이티브 Notepad 감각 재현 비용 | 중 | 비추천 |
| C. WPF shell + WebView2 WYSIWYG editor | 네이티브 창 안에 Toast UI/ProseMirror류 에디터 삽입 | 표 WYSIWYG가 가장 빠르게 가능 | WebView2 의존, 보안/IPC/파일 접근 격리 설계 필요, Notepad보다 웹앱 느낌 | 중상 | V2 후보 |

권장 결론:

V1은 .NET 10 WPF로 간다. 이유는 목표가 크로스 플랫폼이 아니라 Windows 11 Notepad에 가까운 네이티브 프로그램이고, 캡처 제외가 Windows 최상위 창 API와 직접 맞물리며, .NET 10은 LTS라 제품화 기준으로 안정적이기 때문이다. 단, Markdown 미리보기에는 WebView2를 제한적으로 쓰고, Markdown 편집의 중심은 네이티브 텍스트 편집기로 둔다.

## 11. 기술 결정

| 영역 | 선택 | 이유 |
|---|---|---|
| Runtime | .NET 10 LTS, `net10.0-windows` | 2025-11 릴리스된 LTS이며 3년 지원 라인. Windows 데스크톱 앱에 적합 |
| UI | WPF | Windows 전용 그래픽 UI 프레임워크이며 .NET에서 지원. .NET 10 WPF는 성능/Fluent 관련 개선이 있음 |
| Editor | AvalonEdit 우선 검토 | WPF 기반 텍스트 편집 컴포넌트, 줄 번호/하이라이트/대형 텍스트에 유리 |
| Markdown parse | Markdig | .NET용 CommonMark/GFM 계열 Markdown 처리에 적합 |
| Preview | WebView2 또는 WPF FlowDocument | HTML/CSS 표 렌더링 품질은 WebView2가 유리. 단, 로컬 HTML만 사용하고 외부 네트워크 차단 |
| Capture protection | `SetWindowDisplayAffinity(WDA_EXCLUDEFROMCAPTURE)` P/Invoke | Windows 10 2004+에서 캡처 제외 값 지원. WPF 최상위 HWND에 적용 |
| Packaging | MSIX 또는 Velopack/Wix 후보 | Windows 11 배포, 업데이트, 서명 전략에 맞춰 결정 |
| Testing | xUnit + UI smoke + 수동 캡처 매트릭스 | 캡처 제외는 자동화 한계가 있어 수동 증거 필요 |

## 12. 기능 범위

V1 기능:

| 묶음 | 포함 |
|---|---|
| Notepad 기본 | 새 파일, 열기, 저장, 다른 이름 저장, 인쇄 후보, 찾기/바꾸기, 실행 취소/다시 실행, 잘라내기/복사/붙여넣기, 모두 선택 |
| Windows 11 UX | 탭, 상태바, 설정, 라이트/다크/시스템 테마, 기본 단축키, 창 크기/위치 복원 |
| 텍스트 처리 | UTF-8, UTF-8 BOM, UTF-16 LE/BE, ANSI fallback, CRLF/LF/CR 표시와 변환 |
| Markdown | `.md` 자동 인식, 원문/분할/읽기 보기, 제목/굵게/기울임/목록/링크/코드/표 툴바 |
| Markdown 표 | 표 삽입, 행/열 추가/삭제, 정렬, CSV 붙여넣기 변환, 표 미리보기 |
| 보안 | 개인정보 보호 모드, 캡처 제외 적용/해제, 실패 상태 표시, 시작 시 기본값 설정 |

V2 후보:

| 후보 | 이유 |
|---|---|
| 완전 WYSIWYG Markdown canvas | 구현 난도가 높아 V1의 Notepad 감각을 흐릴 수 있음 |
| AI 작성/요약 | Notepad 최신 방향과 맞지만 인증/개인정보/비용 설계가 커짐 |
| 플러그인/문법 하이라이트 확장 | 단순 메모장 클론 목표를 벗어날 수 있음 |
| 클라우드 동기화 | 보안/계정/충돌 해결 범위가 큼 |

## 13. 참고 레포에서 가져올 교훈

| 레포 | 관찰 | 계획 반영 |
|---|---|---|
| 59de44955ebd/Notepatch | 작은 단일 문서 편집기, Notepad 메뉴/대화상자/문자열 리소스 감각, 다크 모드, 인코딩/줄끝/상태바 세부 기능 | "작고 익숙한 편집기"를 제품 원칙으로 삼되, 코드/리소스 복사는 하지 않음 |
| wesmar/notepad | Win32/RichEdit 중심의 초경량 Notepad clone, 파일 I/O와 인코딩, 찾기/바꾸기, 상태바, 단축키가 명확함 | WPF에서도 Win32 편집기처럼 저장/인코딩/단축키/상태바를 핵심 품질로 검증 |

라이선스 주의:

| 항목 | 방침 |
|---|---|
| Notepatch | GitHub 페이지에서 명시 라이선스가 바로 확인되지 않았으므로 코드 복사 금지, 동작 참고만 |
| wesmar/notepad | MIT로 표시되지만 V1에서는 코드 이식 없이 기능/검증 목록 참고만 |
| Microsoft Notepad | 상표, 아이콘, 리소스, 정확한 시각 자산 복제 금지. "Windows 11 Notepad-like" 자체 UI로 구현 |

## 14. 구현 계획

큰 단계:

1. 프로젝트 골격: .NET 10 WPF 앱, 기본 창, 탭, 메뉴, 상태바, 설정 저장소를 만든다.
2. 기본 편집기: 파일 열기/저장, 인코딩/줄끝 처리, 찾기/바꾸기, 단축키를 Notepad 기대 흐름에 맞춘다.
3. Markdown 경험: Markdig 파이프라인, 미리보기, Markdown 툴바, 표 도우미를 붙인다.
4. 보안 모드: WPF 최상위 HWND에 캡처 제외 API를 적용하고 상태/오류/호환성 표시를 만든다.
5. Windows 11 polish: 테마, Mica/Fluent 느낌, 설정, 세션 복원, 접근성 키보드 흐름을 다듬는다.
6. 검증/패키징: Markdown 샘플, 캡처 앱 매트릭스, 설치/업데이트 방식을 검증한다.

파일 소유권 초안:

| 영역 | 예상 파일/폴더 |
|---|---|
| 앱 진입/DI | `src/Memojang.App/App.xaml`, `src/Memojang.App/App.xaml.cs` |
| Shell UI | `src/Memojang.App/Shell/*` |
| Editor | `src/Memojang.App/Editor/*` |
| Markdown | `src/Memojang.App/Markdown/*` |
| Table tools | `src/Memojang.App/Markdown/Tables/*` |
| Windows interop | `src/Memojang.App/WindowsInterop/*` |
| Settings/session | `src/Memojang.App/Settings/*`, `%LocalAppData%/Memojang` |
| Tests | `tests/Memojang.Tests/*`, `samples/markdown/*` |

## 15. 완료 기준

| 기준 | 통과 조건 |
|---|---|
| 계획 완료 | 이 문서가 UX 목표, 기술 선택, 기능 범위, 검증 계획, 리스크를 포함한다 |
| V1 구현 완료 | Windows 11에서 메모장급 텍스트 편집 + Markdown 보기/표 편집 + 캡처 제외 토글이 실제로 동작한다 |
| 제품 감각 | 기본 사용자는 Notepad에서 넘어와도 메뉴/단축키/저장 동작을 다시 배울 필요가 거의 없다 |
| 보안 정직성 | 캡처 제외 성공/실패를 상태로 보여주고, 보장하지 못하는 캡처 유형을 문서화한다 |

## 16. 리스크와 대응

| 리스크 | 영향 | 대응 |
|---|---|---|
| "똑같은 UI"가 상표/저작권/트레이드드레스 리스크가 될 수 있음 | 배포 리스크 | 이름/아이콘/정확한 리소스 복제 금지, Windows 11 친화적 자체 UI로 표현 |
| Markdown WYSIWYG를 WPF만으로 만들면 범위가 커짐 | 일정 지연 | V1은 원문 편집 + 예쁜 미리보기 + 표 도우미, V2에 완전 WYSIWYG |
| WebView2 미리보기가 보안 표면을 넓힘 | XSS/파일 접근 리스크 | 로컬 정적 HTML, CSP, 외부 URL/스크립트 차단, 사용자 Markdown HTML sanitize |
| 캡처 제외가 모든 도구/환경에서 동일하지 않을 수 있음 | 보안 기대 불일치 | 지원 OS/앱 매트릭스와 실패 표시, 물리 카메라/비표준 캡처 미보장 명시 |
| 큰 파일과 Markdown 렌더링이 충돌 | 성능 저하 | 일정 크기 이상은 원문 편집 우선, 미리보기 debounce/비활성화 |

## 17. 검증 명령 초안

구현 후 기본 명령:

```powershell
dotnet --info
dotnet build .\Memojang.sln
dotnet test .\Memojang.sln
```

수동 검증 체크:

| 검증 | 기대 |
|---|---|
| `samples/markdown/table-heavy.md` 열기 | 표가 읽기 좋게 렌더링되고 표 도우미로 행/열 수정 가능 |
| Snipping Tool 캡처 | 보호 모드 on에서 창 내용이 캡처되지 않음 |
| OBS Window Capture | 보호 모드 on/off 차이가 확인됨 |
| UTF-8/LF 파일 저장 | 기존 줄끝과 인코딩 정책이 보존 또는 명시적으로 변환됨 |
| Notepad 비교 | 기본 단축키와 메뉴 흐름이 크게 어긋나지 않음 |

## 18. 참고 출처

| 출처 | 계획에 반영한 내용 |
|---|---|
| Microsoft Learn - WPF documentation | WPF는 .NET에서 사용하는 Windows용 오픈소스 GUI 프레임워크 |
| Microsoft Learn - What's new in WPF for .NET 10 | .NET 10 WPF 릴리스, 성능 개선, Fluent 스타일 개선 |
| Microsoft .NET lifecycle/download | .NET 10은 LTS 및 권장 다운로드 라인 |
| Microsoft Learn - SetWindowDisplayAffinity | `WDA_EXCLUDEFROMCAPTURE` 동작, DWM 조건, DRM 보장 아님 |
| Microsoft Learn - Manage Recall | 앱이 `WDA_EXCLUDEFROMCAPTURE`로 Recall/스크린샷 제외 가능 |
| Electron Docs | Electron은 Chromium/Node를 포함한 크로스 플랫폼 데스크톱 프레임워크 |
| Electron BrowserWindow Docs | `setContentProtection(true)`가 Windows에서 `WDA_EXCLUDEFROMCAPTURE`를 호출 |
| Electron Security Docs | Electron은 Chromium/Node 보안 업데이트와 IPC/프로토콜 보안 부담이 있음 |
| Notepatch GitHub | Notepad 클론에서 메뉴, 상태바, 인코딩, 다크 모드가 핵심 디테일임 |
| wesmar/notepad GitHub | RichEdit, 파일 I/O, Unicode, 상태바, 단축키, 성능 검증 관찰 |
| AvalonEdit GitHub | WPF 기반 텍스트 편집 컴포넌트 후보 |
| Markdig GitHub | .NET Markdown parser 후보 |
| WebView2 docs | WPF 앱에 WebView2를 붙여 HTML/CSS preview 가능 |
| TOAST UI Editor | WYSIWYG Markdown/table이 중요할 때 V2 또는 WebView2 삽입 후보 |

## 19. 현재 결정

이번 계획의 finish line은 "구현 착수 전, Windows 11 Notepad-like Markdown editor의 스택과 V1 범위를 결정할 수 있는 프로그램 계획서"다.

결정:

| 항목 | 값 |
|---|---|
| 스택 | .NET 10 WPF |
| 제품 방향 | Windows 11 Notepad-like, 단순함 우선 |
| Markdown 방향 | V1은 원문 편집 + 예쁜 미리보기 + 표 도우미 |
| 보안 방향 | 앱 창 단위 캡처 제외 토글, 보장 한계 명시 |
| 다음 단계 | 사용자가 이 계획을 승인하면 WPF 프로젝트 스캐폴딩과 V1 첫 슬라이스 구현 |

## Step 1. App-dev init and first implementation entry

Initialize project-local workflow docs, keep this UX/product plan as the V1 source of intent, then scaffold the .NET 10 WPF solution in the next implementation slice.

## Acceptance

- `CLAUDE.md`, `doc/INTENT.md`, `rules/dev-*`, app/skill templates, diagram index, and `.codex/state/dev-docs-done` exist.
- `rules/dev-roadmap.md` stays milestone-level and points to the next WPF scaffold slice.
- `git diff --check`, secret scan, and planning artifact validation pass before claiming `DOCS_CURRENT`.
