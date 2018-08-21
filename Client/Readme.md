# Клиент

## Настройка программы MailToOwncloud.exe

`MailToOwnCloud.exe.Config` - конфигурационный файл

Параметры:

* `server` - сервер OwnCloud
* `login` - имя пользователя OwnCloud
* `password` - пароль пользователя OwnCloud
* `thunderbird_exe` - путь до exe почтового клиента (Thunderbird)
* `thunderbird_args` - аргументы командной строки для запуска почтового клиента (Thunderbird)
* `thunderbird_is_days` - добавлять в письмо информацию о сроке хранения файла
* `thunderbird_days` - кол-во дней хранения файла
* `thunderbird_days_text` - текст сообщения о сроке хранения файла
* `message_error_title` - заголовок сообщения ошибки
* `message_error` - текст сообщения ошибки

Пример конфигурационного файла:
```
    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
        <appSettings>
            <add key="server" value="server" />
            <add key="login" value="login" />
            <add key="password" value="password" />
            <add key="thunderbird_exe" value="C:\Program Files (x86)\Mozilla Thunderbird\thunderbird.exe" />
            <add key="thunderbird_args" value="-compose body={0}" />
            <add key="thunderbird_is_days" value="True" />
            <add key="thunderbird_days" value="180" />
            <add key="thunderbird_days_text" value="Ссылка будет доступна до {0}" />
            <!--Обязательно оборачиваете сообщение {0}[Сообщение]{1} -->
            <add key="message_error_title" value="{0}Error!{1}" />
            <add key="message_error" value="{0}Error! Обратитесь к системному администратору{1}" />
        </appSettings>
    </configuration>
```


## Deploy через GPO

Скопируйте скомпилированную программу на любую общедоступную шару. Файлы которые нужно скопировать:

* `MailToOwnCloud.exe` - программа
* `MailToOwnCloud.exe.Config` - конфигурационный файл
* `access.csv` - файл с доступами
* `WebDAVClient.dll` - библиотека для работы с протоколом WebDAV
* `WpfAnimatedGif.dll` - библиотека для работы с gif анимацией

### Первый способ (рекомендуется)

1. Заходим на контроллере домена: `Пуск` -> `Администрирование` -> `Управление групповыми политиками`
2. Создаем объект групповой политики, указав его имя
3. Нажимаем по созданному объекту правой кнопкой мыши и выбираем `Изменить`
4. Заходим: `Конфигурация пользователя` -> `Настройка` -> `Конфигурация Windows`
5. Нажимаем правой кнопкой мыши на `Ярлыки` и выбираем `Создать`
6. Заполняем: 
    * `Действие` - `Обновить`
    * `Имя` - `Ссылка для отправки`
    * `Размещение` - `Отправить`
    * `Конечный путь` - <путь до `MailToOwnCloud.exe`>
7. Сохраняем

### Второй способ (через логон-скрипт, **не рекомендуется**)

GPO -> logon -> deploy.ps1
