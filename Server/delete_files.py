import easywebdav
import argparse
import logging
import logging.config
from datetime import datetime, timedelta


def main():
    # Аргументы командной строки
    parser = argparse.ArgumentParser(description='Автоматическое удаление старых файлов')
    parser.add_argument("-host", required=True, help="Имя сервера")
    parser.add_argument("-user", required=True, help="Логин")
    parser.add_argument("-pwd",  required=True, help="Пароль")
    parser.add_argument("-days", default=180, type= int, help="Кол-во дней (default: %(default)s)")
    parser.add_argument("-log", default="delete_files.log", help="Файл лога (default: %(default)s)")
    args = parser.parse_args()

    # Логирование
    dictLogConfig = {
        "version":1,
        "handlers":{
            "fileHandler":{
                "class":"logging.FileHandler",
                "formatter":"myFormatter",
                "filename":args.log,
            }
        },
        "loggers":{
            "exampleApp":{
                "handlers":["fileHandler"],
                "level":"INFO",
            }
        },
        "formatters":{
            "myFormatter":{
                "format":"%(asctime)s - %(levelname)s - %(message)s"
            }
        }
    }
    logging.config.dictConfig(dictLogConfig)
    logger = logging.getLogger("exampleApp")

    # Конфиг для подключения по webdav`у
    options = {
        'host': args.host,
        'username': args.user,
        'password': args.pwd,
    }
    webdav = easywebdav.connect(**options)

    try:
        # Получение списка файлов из корня
        # В возвращаемом списке первый элемент нужно удалить
        # первый элемент - корень
        directory = webdav.ls("remote.php/dav/files/%s/"%(args.user))
        directory.pop(0)

        # Пробираюсь по всем папкам (уровень вложенности = 2)
        # удаляю старые папаки
        count_all, count_delete = 0, 0
        for dir1 in directory:
            dir2 = webdav.ls(dir1.name)
            dir2.pop(0)
            for dir3 in dir2:
                count_all += 1
                date = datetime.strptime(dir3.mtime,"%a, %d %b %Y %H:%M:%S %Z")
                path = dir3.name
                if datetime.today() > date + timedelta(days=args.days):
                    webdav.rmdir(path, safe=False)
                    count_delete += 1
                    logger.info("DELETE:%s - %s"%(date, path))

        logger.info("Просканировано папок: %d\tУдаленно папок: %d"%(count_all, count_delete))
    except Exception as ex:
        logger.error(ex)


if __name__ == "__main__":
    main()