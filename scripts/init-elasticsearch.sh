#!/bin/bash
# init-elasticsearch.sh

echo "Attente du démarrage d'Elasticsearch..."
until curl -s -u elastic:elastic123 http://elasticsearch:9200/_cluster/health | grep -q '"status":"green"\|"status":"yellow"'; do
  sleep 5
done

echo "Configuration du mot de passe kibana_system..."
curl -X POST -u elastic:elastic123 "http://elasticsearch:9200/_security/user/kibana_system/_password" \
  -H "Content-Type: application/json" \
  -d '{"password":"kibana123"}'

echo "Configuration terminée!"