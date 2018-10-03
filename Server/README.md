[![](https://images.microbadger.com/badges/image/kovalewvladimir/clear-old-files-owncloud.svg)](https://microbadger.com/images/kovalewvladimir/clear-old-files-owncloud "Get your own image badge on microbadger.com")
[![](https://images.microbadger.com/badges/version/kovalewvladimir/clear-old-files-owncloud.svg)](https://microbadger.com/images/kovalewvladimir/clear-old-files-owncloud "Get your own version badge on microbadger.com")

# Развертывание (deploy)

- `mkdir -p /docker/owncloud`
- `cd /docker/owncloud`
- Скачать `Dockerfile`:
    ```
    # Только для локальной сборки контейнера kovalewvladimir/clear-old-files-owncloud
    wget https://raw.githubusercontent.com/kovalewvladimir/MailToOwnCloud/master/Server/Dockerfile
    
    # Обязательно 
    wget https://raw.githubusercontent.com/kovalewvladimir/MailToOwnCloud/master/Server/docker-compose.yml
    wget https://raw.githubusercontent.com/kovalewvladimir/MailToOwnCloud/master/Server/ru.js
    wget https://raw.githubusercontent.com/kovalewvladimir/MailToOwnCloud/master/Server/ru.json

    ```

- Создать файл `.env` в любой текстовом редакторе. **(Нельзя менять пароль через web интерфейс owncloud, иначе, сломается скрипт по удалению старых файлов.)**
    ```
    OWNCLOUD_VERSION=10.0
    OWNCLOUD_DOMAIN=localhost
    ADMIN_USERNAME=<Имя пользователя owncloud>
    ADMIN_PASSWORD=<Пароль>
    HTTP_PORT=80
    HTTPS_PORT=443
    TIME_RELOAD=<Время автоматического перезапуска скрипта delete_files.py (в секундах)>
    DAYS=<Считать файл старым через N дней>
    ```
    Пример файла:
    ```
    OWNCLOUD_VERSION=10.0
    OWNCLOUD_DOMAIN=localhost
    ADMIN_USERNAME=admin
    ADMIN_PASSWORD=admin
    HTTP_PORT=80
    HTTPS_PORT=443
    TIME_RELOAD=86400
    DAYS=180
    ```

- !!! Только для локальной сборки контейнера kovalewvladimir/clear-old-files-owncloud `docker build -t kovalewvladimir/clear-old-files-owncloud .`

- `docker-compose up -d`

# Проверка

- Просмотр лога контейнера OwnCloud

    `docker logs owncloud_owncloud_1`

    или через web интерфейс `Portainer`

# Изменить конфиг скрипта delete_files.py

- `cd /docker/owncloud`
- Измените файл `.env`
- `docker-compose up -d`
