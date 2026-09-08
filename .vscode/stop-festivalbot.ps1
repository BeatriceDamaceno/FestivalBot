$targets = Get-CimInstance Win32_Process | Where-Object {
    $_.Name -eq "FestivalBot.exe" -or
    ($_.Name -eq "dotnet.exe" -and (
        $_.CommandLine -match "FestivalBot\\.dll" -or
        $_.CommandLine -match "FestivalBot\\.exe"
    ))
}

foreach ($proc in $targets) {
    Stop-Process -Id $proc.ProcessId -Force -ErrorAction SilentlyContinue
}
