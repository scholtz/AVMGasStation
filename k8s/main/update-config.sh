kubectl apply -f deployment-api.yaml -n biatec-gas-station
kubectl delete configmap gas-station-api-main-conf -n biatec-gas-station
kubectl create configmap gas-station-api-main-conf --from-file=conf-api -n biatec-gas-station
kubectl rollout restart deployment/gas-station-api-app-deployment -n biatec-gas-station
kubectl rollout status deployment/gas-station-api-app-deployment -n biatec-gas-station
