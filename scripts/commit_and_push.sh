#!/bin/sh
# Script para añadir, commitear y pushear. Uso: ./commit_and_push.sh "Mensaje de commit"
if [ -z "$1" ]; then
  echo "Uso: ./commit_and_push.sh \"Mensaje de commit\""
  exit 1
fi

git add -A
if [ $? -ne 0 ]; then
  echo "Error: git add falló. Asegúrate de tener git instalado y de estar en el directorio del repositorio."
  exit 1
fi

git commit -m "$1"
if [ $? -ne 0 ]; then
  echo "Error: git commit falló. Revisa el estado del repositorio."
  exit 1
fi

git push
if [ $? -ne 0 ]; then
  echo "Error: git push falló. Revisa la configuración del remoto y tus credenciales."
  exit 1
fi

echo "Commit y push completados con éxito."
