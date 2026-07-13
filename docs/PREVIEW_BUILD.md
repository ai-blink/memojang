# 포터블 미리 보기 빌드

현재 버전은 `0.1.0-preview.1`입니다. Memojang 미리 보기 빌드는 Windows x64용 자체 포함 포터블 폴더로 게시됩니다. 별도 설치나 .NET Desktop Runtime 설치 없이, 게시 폴더 전체를 복사하거나 압축 해제한 뒤 실행할 수 있습니다.

```powershell
dotnet publish .\src\Memojang.App\Memojang.App.csproj -p:PublishProfile=PreviewFolder
```

출력 폴더:

```text
artifacts\publish\Memojang-0.1.0-preview.1-win-x64\
```

실행 확인:

```powershell
Start-Process .\artifacts\publish\Memojang-0.1.0-preview.1-win-x64\Memojang.App.exe
```

배포할 때는 위 폴더의 파일을 모두 함께 제공해야 합니다. 이 빌드는 Windows x64 전용이며, 아직 미리 보기 버전입니다.
