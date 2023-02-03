FROM python:3.8

WORKDIR /opt/

ADD requirements.txt requirements.txt
RUN pip3 install -r requirements.txt

ADD delete_files.py delete_files.py

ENV HOST=host         \
    USER=user         \
    PASSWD=pwd        \
    TIME_RELOAD=86400 \
    DAYS=365          \
    LOGFILE=/var/log/delete_files.log

CMD python /opt/delete_files.py --host ${HOST} --user ${USER} --pwd ${PASSWD} --time-reload ${TIME_RELOAD} --days ${DAYS} --log ${LOGFILE}