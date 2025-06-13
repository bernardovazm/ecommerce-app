#!/bin/bash

echo "Iniciando setup"

if ! command -v docker &> /dev/null; then
    echo "Docker não está instalado"
    exit 1
fi

if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "Docker Compose não está instalado"
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo ".NET SDK não está instalado"
    exit 1
fi

if ! command -v node &> /dev/null; then
    echo "Node.js não está instalado"
    exit 1
fi

chmod +x run-sonar-analysis.sh
chmod +x docker/init-db.sh

dotnet tool install --global dotnet-sonarscanner --version 5.14.0

cd web
npm install
cd ..

docker compose up db rabbitmq sonarqube -d

until curl -s http://localhost:9000/api/system/status | grep -q "UP"; do
    sleep 10
done

curl -s -u admin:admin -X POST "http://localhost:9000/api/projects/create?project=ecommerce-fullstack&name=Ecommerce%20Full%20Stack%20Application"
curl -s -u admin:admin -X POST "http://localhost:9000/api/projects/create?project=ecommerce-api&name=Ecommerce%20API"
curl -s -u admin:admin -X POST "http://localhost:9000/api/projects/create?project=ecommerce-web&name=Ecommerce%20Web"

echo "Setup concluído"
