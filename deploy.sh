#!/bin/bash

#admin_password=${1}
#mongo_password=${2}


#echo "Inserting passwords"
#Inserting mongo password
#echo "MONGO_INITDB_ROOT_PASSWORD=${mongo_password}" > .env
#echo "MongoSettings__Password=${mongo_password}" >> .env
#echo "AdminAccount__Password=${admin_password}" >> .env

cd SciencePaperAnalyzer/

echo "Stopping old configuration"
docker-compose -f ./docker-compose_prod.yml down || true

echo "Building "
docker-compose -f ./docker-compose_prod.yml build

echo "Running"
docker-compose -f ./docker-compose_prod.yml up -d

echo "Getting startup logs"

sleep 10s

docker-compose -f ./docker-compose_prod.yml logs
