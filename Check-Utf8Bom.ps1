<#
.SYNOPSIS
    Проверяет, что файлы в индексе Git строго в UTF-8 with BOM.
    Игнорирует несуществующие файлы и бинарные файлы.
#>

# Получаем полный путь к корню репозитория
$repoRoot = git rev-parse --show-toplevel
if (-not $repoRoot) {
    Write-Host "❌ git not found" -ForegroundColor Red
    exit 1
}

# Получаем только файлы из индекса Git (уже добавленные через git add)
$stagedFiles = git diff --cached --name-only --diff-filter=AM | ForEach-Object {
    Join-Path -Path $repoRoot -ChildPath $_
}

if (-not $stagedFiles) {
    Write-Host "ℹ️ files empty." -ForegroundColor Yellow
    exit 0
}

$nonBomFiles = @()

foreach ($file in $stagedFiles) {
    # Пропускаем бинарные файлы
    if ([System.IO.Path]::GetExtension($file) -match '\.(png|jpg|jpeg|gif|pdf|exe|dll|zip|rar|7z)$') {
        continue
    }

    # Проверяем существование файла
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        Write-Host "⚠️ not found and skip: $file" -ForegroundColor Yellow
        continue
    }

    try {
        # Читаем первые 3 байта для проверки BOM
        $bytes = [System.IO.File]::ReadAllBytes($file) | Select-Object -First 3

        # Если BOM нет (EF BB BF) - добавляем в список
        if ($bytes.Count -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
            $nonBomFiles += $file
        }
    }
    catch {
        Write-Host "⚠️ Error by check $file : $_" -ForegroundColor Yellow
        continue
    }
}

if ($nonBomFiles.Count -gt 0) {
    Write-Host "❌ Error: This files in index Git not in UTF-8 with BOM:" -ForegroundColor Red
    $nonBomFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }

    foreach ($file in $nonBomFiles) {
		try {
			# Читаем содержимое файла как байты
			$contentBytes = [System.IO.File]::ReadAllBytes($file)
			
			# Создаем новый поток с BOM в начале
			$newContent = [System.Text.Encoding]::UTF8.GetPreamble() + $contentBytes
			
			# Записываем обратно
			[System.IO.File]::WriteAllBytes($file, $newContent)
			
			Write-Host "✓ Success Add BOM to file: $file" -ForegroundColor Green
		}
		catch {
			Write-Host "⚠️ Error Add BOM to $file : $_" -ForegroundColor Red
		}
    }
    exit 1  # Отменяем коммит
}
else {
    Write-Host "✓ Success: All files in index Git in UTF-8 with BOM" -ForegroundColor Green
    exit 0
}