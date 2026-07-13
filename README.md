# Memojang

> Windows 11용 가벼운 텍스트·Markdown 편집기

**현재 버전: `0.1.0-preview.1`**

Memojang은 메모장처럼 빠르고 익숙한 편집 경험에 Markdown과 표 미리 보기, 이미지 삽입, 창 캡처 제외 기능을 더한 .NET 10 WPF 앱입니다.

## 주요 기능

- 새 문서, 열기, 저장, 다른 이름으로 저장과 변경 내용 확인
- 여러 문서 탭, 최근 문서, 모두 저장
- 줄·열, 글자 수, 인코딩, 줄바꿈, 문서 모드, 캡처 상태 표시
- `.txt`, `.md`, `.markdown` 문서 모드 자동 감지
- 제목, 문단, 표, 이미지, 구분선, 코드 블록, 강조, 목록을 지원하는 Markdown 미리 보기
- 저장한 Markdown 문서의 사이드카 자산 폴더를 활용한 이미지 삽입
- 제목, 목록, 굵게, 기울임, 취소선, 링크, 표, 이미지, 미리 보기 도구 모음
- 글꼴, 글자 크기, 찾기, 이동, 확대/축소 설정
- 라이트·다크 테마, 다국어 UI 기반, 아랍어 RTL 레이아웃
- Windows 창 캡처 제외 요청과 실제 적용 상태 표시

## 포터블 배포

Memojang은 Windows x64용 **자체 포함 포터블 폴더**로 게시할 수 있습니다. .NET Desktop Runtime 설치나 별도 설치 과정 없이 폴더 전체를 복사하거나 압축 해제한 뒤 `Memojang.App.exe`를 실행하면 됩니다.

```powershell
dotnet publish .\src\Memojang.App\Memojang.App.csproj -p:PublishProfile=PreviewFolder
```

게시 결과:

```text
artifacts\publish\Memojang-0.1.0-preview.1-win-x64\
```

배포할 때는 결과 폴더 안의 모든 파일을 함께 제공하세요. 현재 배포본은 Windows x64 전용 미리 보기 버전입니다.

```powershell
Start-Process .\artifacts\publish\Memojang-0.1.0-preview.1-win-x64\Memojang.App.exe
```

## 개발 환경

- Windows 11
- .NET 10 SDK

```powershell
dotnet --info
dotnet build .\Memojang.sln
dotnet test .\Memojang.sln
```

디버그 실행:

```powershell
Start-Process .\src\Memojang.App\bin\Debug\net10.0-windows\Memojang.App.exe
```

## 유의 사항

Memojang은 워드프로세서나 스프레드시트 편집기가 아닙니다. 창 캡처 제외는 Windows가 제공하는 최선형 기능이며, 모든 캡처 경로를 차단하는 DRM 기능이 아닙니다. 앱은 실제 적용 결과를 `표시됨`, `제외됨`, `제한됨`, `사용 불가` 상태로 보여 줍니다.

## 프로젝트 구성

- `src/Memojang.App/` - WPF 앱 소스
- `tests/Memojang.Tests/` - 파서, 문서 모드, 이미지, 표, 셸 도우미 테스트
- `doc/INTENT.md` - 제품 목표와 비목표
- `docs/PREVIEW_BUILD.md` - 포터블 미리 보기 빌드 안내
- `rules/` - 개발 진행 상황, 로드맵, 의사결정
- `notes/plans/`, `notes/research/` - 계획과 리서치 기록

## 기여와 개발 규칙

프로젝트 지침의 기준 문서는 `CLAUDE.md`입니다. 메모장다운 가벼운 Windows UX를 유지하고, Markdown 미리 보기 개선은 저장 원문을 바꾸지 않는 렌더링 동작으로 제한합니다.
