#!/bin/bash

helm install --name service-test-postgres -f values.yaml stable/postgresql

## to fully delete
# helm delete service-test-postgres
# helm del --purge service-test-postgres
# kubectl delete pvc data-service-test-postgres-postgresql-0