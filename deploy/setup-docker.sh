#!/bin/bash
# Executa em cada EC2 antes de subir os serviços
# Amazon Linux 2023

set -e

sudo yum update -y
sudo yum install -y docker

sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker ec2-user

# Docker Compose v2
sudo curl -SL "https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64" \
  -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

echo "Docker $(docker --version) instalado."
echo "Docker Compose $(docker-compose --version) instalado."
echo "ATENÇÃO: faça logout e login novamente para o grupo docker ter efeito."
