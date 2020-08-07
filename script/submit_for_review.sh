#!/bin/bash
# parameter:
#       файл работы на проверку
#       адрес сервиса
#       название критерия для проверки
#       файл-название работы

# hard-coded for testing
# WORK_FILE="E:/scientific_writing-2019/6304_KovynevMV/paper.md"
# SERVICE_ADR="http://localhost:4444"
# CRITERIA_NAME=""
# WORKNAME_FILE=""


if [ "$#" -gt "0" ]; then
    # input from command line parameter 
    WORK_FILE=$1
    SERVICE_ADR=$2
    CRITERIA_NAME=$3
    WORKNAME_FILE=$4
else
    # read parameters from console
    echo "Введите путь к работе"
    read WORK_FILE
    echo "Введите адрес сервиса"
    read SERVICE_ADR
    echo "Введите название критерия для проверки"
    read CRITERIA_NAME
    echo "Введите файл-название работы"
    read WORKNAME_FILE
fi

echo "Путь к работе = '$WORK_FILE'"
echo "Адрес сервиса = '$SERVICE_ADR'"
echo "Название критерия для проверки = '$CRITERIA_NAME'"
echo "Файл-название работы = '$WORKNAME_FILE'"

# run curl command to send request and get the response in form 'response,return_code'

RESPONSE=$(curl -w "|%{http_code}" -s \
                    -F file=@$WORK_FILE         \
                    -F paperName=@$WORKNAME_FILE             \
                    -F criteriaName="$CRITERIA_NAME"         \
                    $SERVICE_ADR/Home/CurlUploadFile
            )

RESULT_ID=$(echo "$RESPONSE" | cut -d '|' -f 1)
RESPONSE_CODE=$(echo "$RESPONSE" | cut -d '|' -f 2)

# print result 
if [ "$RESPONSE_CODE" = "200" ]; then
    echo "Результат: $SERVICE_ADR/Home/Result?id=$RESULT_ID";
else
    echo "Произошла ошибка. Пожалуйста, проверьте путь к сервису и путь к файлу работы.";
fi