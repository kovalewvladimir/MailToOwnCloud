import argparse
import logging.config
import time
from datetime import datetime, timedelta

from webdav3.client import Client


def delete_files():
    # Конфиг для подключения по webdav`у
    options = {
        'webdav_hostname': args.host,
        'webdav_login': args.user,
        'webdav_password': args.pwd
    }

    try:
        client = Client(options)
        root_path = client.list('/')[1:]

        count_all, count_delete = 0, 0
        for userPath in root_path:
            folders_user_path = client.list(userPath)[1:]
            for folder in folders_user_path:
                try:
                    date = datetime.strptime(folder, '%Y.%m.%d_%H-%M.%S/')
                except ValueError:
                    continue
                count_all += 1
                if datetime.today() > date + timedelta(days=args.days):
                    path = userPath + folder
                    client.clean(path)
                    count_delete += 1
                    logger.info(f'DELETE:{date} - {path}')

        logger.info('Просканировано папок: %d\tУдаленно папок: %d' % (count_all, count_delete))
    except Exception as ex:
        logger.error(ex)


def main():
    # Запуск бесконечного цикла
    while True:
        time.sleep(args.time_reload)
        delete_files()


if __name__ == '__main__':

    # Аргументы командной строки
    parser = argparse.ArgumentParser(description='Автоматическое удаление старых файлов')
    parser.add_argument('--host', required=True, help='Имя сервера')
    parser.add_argument('--user', required=True, help='Логин')
    parser.add_argument('--pwd', required=True, help='Пароль')
    parser.add_argument('--time-reload', default=86400, type=int,
                        help='Перезапустить скрипт через (в секундах) (default: %(default)s)')
    parser.add_argument('--days', default=180, type=int, help='Кол-во дней (default: %(default)s)')
    parser.add_argument('--log', default='delete_files.log', help='Файл лога (default: ./%(default)s)')
    args = parser.parse_args()

    # Логирование
    dictLogConfig = {
        'version': 1,
        'handlers': {
            'fileHandler': {
                'class': 'logging.FileHandler',
                'formatter': 'verbose',
                'filename': args.log,
            },
            'consoleHandler': {
                'class': 'logging.StreamHandler',
                'formatter': 'verbose'
            },
        },
        'loggers': {
            'delete_files': {
                'handlers': ['fileHandler', 'consoleHandler'],
                'level': 'INFO',
            }
        },
        'formatters': {
            'verbose': {
                'format': '%(asctime)s - %(levelname)s - %(message)s'
            }
        }
    }
    logging.config.dictConfig(dictLogConfig)
    logger = logging.getLogger('delete_files')

    try:
        main()
    except KeyboardInterrupt:
        logger.info('\n\n---Конец---')
        exit()
