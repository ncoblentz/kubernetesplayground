# Learning Kubernetes Multi-Node Ingress

- [Learning Kubernetes Multi-Node Ingress](#learning-kubernetes-multi-node-ingress)
  - [Experiment Todos](#experiment-todos)
  - [Notes](#notes)
    - [Setup](#setup)
    - [Ingress Notes](#ingress-notes)
      - [Pink](#pink)
      - [Blue](#blue)
      - [Red 404](#red-404)
  - [Later](#later)

## Experiment Todos
- [ ] addon-metallb
- [ ] ingress
  - [x] global default 404 (colorappred)
  - [ ] difference between default * and default in specific ingress
  - [ ] web app that
    - [ ] diffentiates by color
      - [ ] reports
        - [ ] client ip
        - [ ] server ip
        - [ ] hostname
        - [ ] url path
      - [ ] test cases
        - [ ] rewrite
        - [ ] non-rewrite
        - [ ] each ingress controller type
        - [ ] headers to forward client ip
        - [ ] forge headers resistance
        - [ ] dump env variables
- [ ] external DNS
- [ ] rewrite rules
  - [ ] simple & REGEX
- [ ] let's encrypt / cert-manager

## Notes

### Setup

__VM Setup__

- I started with ~~two~~ one Ubuntu 22.04.1 laptop~~s~~.
- I then created two KVM instances ~~on each laptop~~ also running Ubuntu 22.04.1. (Each has 2 vCPUs and 4096 of ram)   
  - `sudo apt update && sudo apt upgrade && sudo apt install docker && sudo apt autoremove`. 
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
- `microk8s enable dns hostpath-storage dashboard helm ingress rbac`

edit `~/.bashrc` and add:

```
alias kubectl='microk8s kubectl'
source <(kubectl completion bash)
```

__Cluster Creation__
- Master Node on Laptop 1: `microk8s add-node`
- Worker Node on Laptop 1: `microk8s join 192.168.122.251:25000/c71a575baa373ad498c7fe02b4525de5/53b2fdd8c544 --worker`

### Ingress Notes
```bash
$ kubectl get nodes -o wide
NAME               STATUS   ROLES    AGE   VERSION   INTERNAL-IP      EXTERNAL-IP   OS-IMAGE             KERNEL-VERSION      CONTAINER-RUNTIME
g7ubuntu2204k8n2   Ready    <none>   15m   v1.26.1   192.168.122.37   <none>        Ubuntu 22.04.2 LTS   5.19.0-32-generic   containerd://1.6.8
g7ubuntuk8n1       Ready    <none>   16m   v1.26.1   192.168.122.61   <none>        Ubuntu 22.04.2 LTS   5.19.0-32-generic   containerd://1.6.8

$ sudo bash -c "echo '192.168.122.37 k8testblue.com' >> /etc/hosts"
$ sudo bash -c "echo '192.168.122.37 k8testpink.com' >> /etc/hosts"
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



## Later
https://microk8s.io/docs/nfs
https://www.ibm.com/docs/en/cloud-private/3.2.0?topic=console-namespace-is-stuck-in-terminating-state   