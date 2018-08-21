# vek 03/08/2018
# Автоматическое развертывание MailToOwnCloud
#
# Используется python-simple-http-logger
# https://github.com/kovalewvladimir/python-simple-http-logger
#
# Invoke-WebRequest                                          `
# -Uri "http://<server>:9000/MailToOwnCloud/<name_log>.log" `
# -Method Post -Body @{"t"= $logString}                  `
# | Out-Null

# Логирование
$logDir  = $env:APPDATA + "\log_scripts_gpo\"
if (-not (Test-Path -Path $logDir)) {
    New-Item -Path $logDir -ItemType directory
}
$logFile = $logDir + "deployMailToOwnCloud.log"
Get-Date >> $logFile
$logString = ""

# Путь до csv файла с доступом
$pathCsvFile = $PSScriptRoot + "\access.csv"
# Имя ярлыка программы
$nameLink = "Ссылка для отправки.lnk"
# Путь до ярлыка программы
$pathLink = "\\<СЕРВЕР>\NETLOGON\MailToOwnCloud\" + $nameLink
# Путь до папки SendTo
$pathSendTo = $env:APPDATA + "\Microsoft\Windows\SendTo\"
# Путь до ярлыка в SendTo
$pathLinkSendTo = $pathSendTo + $nameLink
# Текущая дата
$currentDate = Get-Date
# Парсим доступы
$access = Import-Csv -Path $pathCsvFile

$(
    foreach($a in $access) {
        if ($a.USER.ToUpper() -ne $env:USERNAME) {
            continue
        }
        $date = Get-Date -Date $a.Date
        if ($currentDate -lt $date) {
            # Копирование ярлыка
            if (Test-Path -Path $pathLinkSendTo) {
                $logString += "INFO: Ярлык уже существует у пользователя $($env:USERNAME) на компьютере $($env:COMPUTERNAME). Будет удален $($date)"
            } else {
                $logString += "INFO: Копирование ярлыка пользователю $($env:USERNAME) на компьютере $($env:COMPUTERNAME)"
                Copy-Item -Path $pathLink -Destination $pathSendTo
                Invoke-WebRequest                                          `
                    -Uri "http://<server>:9000/MailToOwnCloud/_COPY.log" `
                    -Method Post -Body @{"t"= $logString}                  `
                | Out-Null
            }
        } else {
            # Удаление ярлыка
            if (Test-Path -Path $pathLinkSendTo) {
                $logString += "INFO: Удаление ярлыка у пользователя $($env:USERNAME) на компьютере $($env:COMPUTERNAME)"
                Remove-Item -Path $pathLinkSendTo
                Invoke-WebRequest                                            `
                    -Uri "http://<server>:9000/MailToOwnCloud/_DELETE.log" `
                    -Method Post -Body @{"t"= $logString}                    `
                | Out-Null
            } else {
                $logString += "INFO: Ярлык уже удален у пользователя $($env:USERNAME) на компьютере $($env:COMPUTERNAME). Удален $($date)"
            }
        }
        # Вывод лога
        $logString

        # Отправка лога на сервер
        Invoke-WebRequest                                                         `
            -Uri "http://<server>:9000/MailToOwnCloud/$($env:COMPUTERNAME).log" `
            -Method Post -Body @{"t"= $logString}                                 `
        | Out-Null
    }
) *>&1>> $logFile