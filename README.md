# Learning Kubernetes Multi-Node Ingress

- [Learning Kubernetes Multi-Node Ingress](#learning-kubernetes-multi-node-ingress)
  - [Experiment Todos](#experiment-todos)
  - [Notes](#notes)
    - [Setup](#setup)
    - [Ingress Controller Experiments](#ingress-controller-experiments)
      - [NGINX](#nginx)
      - [Traefik](#traefik)
      - [HAProxy](#haproxy)
      - [Contour](#contour)
      - [Kong](#kong)
      - [Istio](#istio)
    - [Ingress Notes](#ingress-notes)
      - [Pink](#pink)
      - [Blue](#blue)
      - [Red 404](#red-404)
      - [Diagnostic App](#diagnostic-app)
        - [Build](#build)
        - [Kubernetes](#kubernetes)
  - [Later](#later)

## Experiment Todos

- [ ] addon-metallb
- [ ] ingress
  - [X] global default 404 (colorappred)  
  - [ ] web app that
    - [X] reports
      - [X] Client IP
      - [X] Server IP
      - [X] Hostname
      - [X] URL path
      - [X] Request HTTP headers
      - [X] Environment Variables 
    - [ ] test cases
      - [ ] URL rewriting
      - [ ] each ingress controller type
        - [X] Built in NGINX
        - [ ] HAProxy
        - [ ] Contour
        - [ ] Kong
        - [ ] Traefik
        - [ ] Istio
      - [X] headers to forward client IP
      - [X] forged `X-Forwarded-For` (and similar) headers resistance
        - NGINX - replaces `X-Forwarded-For` and `X-Real-IP` with its own headers and puts the previous `X-Forwarded-For: 10.10.10.11` headers as `X-Original-Forwarded-For: 10.10.10.11`        
- [ ] external DNS
- [ ] rewrite rules
  - [ ] simple & REGEX
- [ ] let's encrypt / cert-manager

## Notes

### Setup

__VM Setup__

- I started with ~~two~~ one Ubuntu 22.04.1 laptop~~s~~.
- I then created two KVM instances ~~on each laptop~~ also running Ubuntu 22.04.1. (Each has 2 vCPUs and 4096 of ram)   
  - `sudo apt update && sudo apt upgrade && sudo apt install docker nfs-common && sudo apt autoremove`. 
  - Switched NIC to Bridged mode, selected device `virbr0`
  - edit `/etc/hostname` and change it to `g7ubuntu2204k8n1` and `g7ubuntu2204k8n2`

__Microk8s Install__

https://microk8s.io/docs/getting-started
- `sudo snap install microk8s --classic --channel=1.26`
- `sudo usermod -a -G microk8s $USER`
- `sudo chown -f -R $USER ~/.kube`
- `su - $USER`
- `microk8s status --wait-ready`
- `microk8s kubectl get nodes`
- ~~`microk8s enable dns hostpath-storage dashboard helm ingress rbac`~~
- `microk8s enable dns helm ingress`

edit `~/.bashrc` and add:

```
alias kubectl='microk8s kubectl'
alias helm='microk8s helm'
source <(kubectl completion bash)
```

__Cluster Creation__

- Master VM on Laptop 1: `microk8s add-node`
- Worker VM on Laptop 1: `microk8s join 192.168.122.251:25000/c71a575baa373ad498c7fe02b4525de5/53b2fdd8c544 --worker`

### Ingress Controller Experiments

#### NGINX

The original setup included `ingress` which is the build in NGINX controller

- https://github.com/kubernetes/ingress-nginx/blob/main/docs/user-guide/nginx-configuration/annotations.md

#### Traefik
https://www.robert-jensen.dk/posts/2021-microk8s-with-traefik-and-metallb/

```bash
$ microk8s enable community

$ microk8s disable ingress

$ microk8s enable metallb:192.168.122.240-192.168.122.250

```

Todo

#### HAProxy

```bash
$ helm repo add haproxytech https://haproxytech.github.io/helm-charts

$ helm repo update

$ helm search repo haproxy

$ helm install haproxy haproxytech/kubernetes-ingress
NAME: haproxy
LAST DEPLOYED: Thu Feb 23 13:45:47 2023
NAMESPACE: default
STATUS: deployed
REVISION: 1
TEST SUITE: None
NOTES:
HAProxy Kubernetes Ingress Controller has been successfully installed.

Controller image deployed is: "haproxytech/kubernetes-ingress:1.9.3".
Your controller is of a "Deployment" kind. Your controller service is running as a "NodePort" type.
RBAC authorization is enabled.
Controller ingress.class is set to "haproxy" so make sure to use same annotation for
Ingress resource.

Service ports mapped are:
  - name: http
    containerPort: 8080
    protocol: TCP
  - name: https
    containerPort: 8443
    protocol: TCP
  - name: stat
    containerPort: 1024
    protocol: TCP

Node IP can be found with:
  $ kubectl --namespace default get nodes -o jsonpath="{.items[0].status.addresses[1].address}"

The following ingress resource routes traffic to pods that match the following:
  * service name: web
  * client's Host header: webdemo.com
  * path begins with /

  ---
  apiVersion: networking.k8s.io/v1
  kind: Ingress
  metadata:
    name: web-ingress
    namespace: default
    annotations:
      ingress.class: "haproxy"
  spec:
    rules:
    - host: webdemo.com
      http:
        paths:
        - path: /
          backend:
            serviceName: web
            servicePort: 80

In case that you are using multi-ingress controller environment, make sure to use ingress.class annotation and match it
with helm chart option controller.ingressClass.

For more examples and up to date documentation, please visit:
  * Helm chart documentation: https://github.com/haproxytech/helm-charts/tree/main/kubernetes-ingress
  * Controller documentation: https://www.haproxy.com/documentation/kubernetes/latest/
  * Annotation reference: https://github.com/haproxytech/kubernetes-ingress/tree/master/documentation
  * Image parameters reference: https://github.com/haproxytech/kubernetes-ingress/blob/master/documentation/controller.md

```

#### Contour

Todo

#### Kong

Todo



#### Istio

Todo

### Ingress Notes

```bash
$ kubectl get nodes -o wide
NAME               STATUS   ROLES    AGE   VERSION   INTERNAL-IP      EXTERNAL-IP   OS-IMAGE             KERNEL-VERSION      CONTAINER-RUNTIME
g7ubuntu2204k8n2   Ready    <none>   15m   v1.26.1   192.168.122.37   <none>        Ubuntu 22.04.2 LTS   5.19.0-32-generic   containerd://1.6.8
g7ubuntuk8n1       Ready    <none>   16m   v1.26.1   192.168.122.61   <none>        Ubuntu 22.04.2 LTS   5.19.0-32-generic   containerd://1.6.8

$ sudo bash -c "echo '192.168.122.37 k8testblue.com' >> /etc/hosts"
$ sudo bash -c "echo '192.168.122.37 k8testpink.com' >> /etc/hosts"
$ sudo bash -c "echo '192.168.122.37 k8testdiag.com' >> /etc/hosts"
```

#### Pink

`$ kubectl apply -f pink.yaml`

```yaml
---
apiVersion: v1
kind: Namespace
metadata:  
  labels:
    kubernetes.io/metadata.name: colorapp
  name: colorapppink
---
apiVersion: v1
data:
  APP_COLOR: pink
kind: ConfigMap
metadata:
  creationTimestamp: null
  name: colorappconfig
  namespace: colorapppink
---
apiVersion: apps/v1
kind: Deployment
metadata:
  creationTimestamp: null
  labels:
    app: colorapp
  name: colorapp
  namespace: colorapppink
spec:
  replicas: 3
  selector:
    matchLabels:
      app: colorapp
  strategy: {}
  template:
    metadata:
      creationTimestamp: null
      labels:
        app: colorapp
    spec:
      containers:
      - image: kodekloud/webapp-color
        name: webapp-color
        resources: {}
        envFrom:
        - configMapRef:
            name: colorappconfig
        ports:
          - containerPort: 8080
            name: colorapp-http
---
apiVersion: v1
kind: Service
metadata:
  creationTimestamp: null
  labels:
    app: colorapp
  name: colorapp
  namespace: colorapppink
spec:
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: colorapp-http
  selector:
    app: colorapp
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  creationTimestamp: null
  name: colorapp
  namespace: colorapppink
spec:
  rules:
  - host: k8testpink.com
    http:
      paths:
      - backend:
          service:
            name: colorapp
            port:
              name: http
        path: /
        pathType: Prefix
```

#### Blue

`$ kubectl apply -f blue.yaml`

```yaml
---
apiVersion: v1
kind: Namespace
metadata:  
  labels:
    kubernetes.io/metadata.name: colorapp
  name: colorappblue
---
apiVersion: v1
data:
  APP_COLOR: blue
kind: ConfigMap
metadata:
  creationTimestamp: null
  name: colorappconfig
  namespace: colorappblue
---
apiVersion: apps/v1
kind: Deployment
metadata:
  creationTimestamp: null
  labels:
    app: colorapp
  name: colorapp
  namespace: colorappblue
spec:
  replicas: 3
  selector:
    matchLabels:
      app: colorapp
  strategy: {}
  template:
    metadata:
      creationTimestamp: null
      labels:
        app: colorapp
    spec:
      containers:
      - image: kodekloud/webapp-color
        name: webapp-color
        resources: {}
        envFrom:
        - configMapRef:
            name: colorappconfig
        ports:
          - containerPort: 8080
            name: colorapp-http
---
apiVersion: v1
kind: Service
metadata:
  creationTimestamp: null
  labels:
    app: colorapp
  name: colorapp
  namespace: colorappblue
spec:
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: colorapp-http
  selector:
    app: colorapp
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  creationTimestamp: null
  name: colorapp
  namespace: colorappblue
spec:
  rules:
  - host: k8testblue.com
    http:
      paths:
      - backend:
          service:
            name: colorapp
            port:
              name: http
        path: /
        pathType: Prefix
```

#### Red 404

`$ kubectl apply -f red.yaml`

```yaml
---
apiVersion: v1
kind: Namespace
metadata:  
  labels:
    kubernetes.io/metadata.name: colorapp
  name: colorappred
---
apiVersion: v1
data:
  APP_COLOR: red
kind: ConfigMap
metadata:
  creationTimestamp: null
  name: colorappconfig
  namespace: colorappred
---
apiVersion: apps/v1
kind: Deployment
metadata:
  creationTimestamp: null
  labels:
    app: colorapp
  name: colorapp
  namespace: colorappred
spec:
  replicas: 3
  selector:
    matchLabels:
      app: colorapp
  strategy: {}
  template:
    metadata:
      creationTimestamp: null
      labels:
        app: colorapp
    spec:
      containers:
      - image: kodekloud/webapp-color
        name: webapp-color
        resources: {}
        envFrom:
        - configMapRef:
            name: colorappconfig
        ports:
          - containerPort: 8080
            name: colorapp-http
---
apiVersion: v1
kind: Service
metadata:
  creationTimestamp: null
  labels:
    app: colorapp
  name: colorapp
  namespace: colorappred
spec:
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: colorapp-http
  selector:
    app: colorapp
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  creationTimestamp: null
  name: colorapp
  namespace: colorappred
spec:
  defaultBackend:
    service:
      name: colorapp
      port:
        name: http
```

#### Diagnostic App

##### Build

```bash
$ docker build . -t diagnosticapp -t sekhmetn/diagnosticapp:latest && docker push sekhmetn/diagnosticapp:latest
```

##### Kubernetes

`$ kubectl apply -f diagnostic.yaml`
`$ kubectl -n diagnosticapp rollout restart deployment diagnosticapp`
```yaml
---
apiVersion: v1
kind: Namespace
metadata:  
  labels:
    kubernetes.io/metadata.name: diagnosticapp
  name: diagnosticapp
---
apiVersion: apps/v1
kind: Deployment
metadata:
  creationTimestamp: null
  labels:
    app: diagnosticapp
  name: diagnosticapp
  namespace: diagnosticapp
spec:
  replicas: 1
  selector:
    matchLabels:
      app: diagnosticapp
  strategy: {}
  template:
    metadata:
      creationTimestamp: null
      labels:
        app: diagnosticapp
    spec:
      containers:
      - image: sekhmetn/diagnosticapp
        name: diagnosticapp          
        ports:
          - containerPort: 5000
            name: diag-http
---
apiVersion: v1
kind: Service
metadata:
  creationTimestamp: null
  labels:
    app: diagnosticapp
  name: diagnosticapp
  namespace: diagnosticapp
spec:
  ports:
  - name: http
    port: 5000
    protocol: TCP
    targetPort: diag-http
  selector:
    app: diagnosticapp
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  creationTimestamp: null
  name: diagnosticapp
  namespace: diagnosticapp
spec:
  rules:
  - host: k8testdiag.com
    http:
      paths:
      - backend:
          service:
            name: diagnosticapp
            port:
              name: http
        path: /
        pathType: Prefix
```


## Later

https://microk8s.io/docs/nfs
https://www.ibm.com/docs/en/cloud-private/3.2.0?topic=console-namespace-is-stuck-in-terminating-state   