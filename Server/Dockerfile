FROM python:3

WORKDIR /opt/

ADD https://raw.githubusercontent.com/kovalewvladimir/MailToOwnCloud/master/Server/requirements.txt requirements.txt 
ADD https://raw.githubusercontent.com/kovalewvladimir/MailToOwnCloud/master/Server/delete_files.py delete_files.py

RUN pip3 install -r requirements.txt

ENV HOST=host         \
    USER=user         \
    PASSWD=pwd        \
    TIME_RELOAD=86400 \
    DAYS=30           \
    LOGFILE=/var/log/delete_files.log

CMD python /opt/delete_files.py --host ${HOST} --user ${USER} --pwd ${PASSWD} --time-reload ${TIME_RELOAD} --days ${DAYS} --log ${LOGFILE}