# stackmgr
Management CLI for Deployment Stack

## Create a new environment
```
stackmgr new env dev
stackmgr default env dev            # set dev as default environment in this session
```
If you don't set the default environment, you have to specify the environment with --env|--environment option every time.

### Configure environment settings

Configure Rancher/RKE2 API Settings:
```
stackmgr configure env dev \
  --rke2-access-token <token> \
  --rke2-url <url> \
  --rke2-project-id <project-id>
```

Configure ArgoCD API Settings:
```
stackmgr configure env dev \
  --argocd-access-token <token> \
  --argocd-url <url> \
  --argocd-project <project> \
  --argocd-repository <repository>      # repository of your stack manifests
```
And then you can test the settings:
```
stackmgr env test <environment-name>
```

## Stack Management

### Create a new stack
```
stackmgr new stack teststack
```

### Sync your stack with Rancher and ArgoCD
This command creates a new namespace in Rancher and deploys the application to ArgoCD:
```
stackmgr sync stack teststack
```

### AutoSync (ArgoCD)
To enable/disable autosync in ArgoCD, use following command:
```
stackmgr configure stack teststack --auto-sync true|false
# You have to sync it afterwards
stackmgr sync stack teststack
```

## App Management
### Create a new empty app
```
stackmgr new app teststack testapp
```

### Create a new app from template
You have to configure the app manifest repository first:
```
stackmgr configure env dev --app-repository <repository>
```
Now you can create a new app from a template:
```
stackmgr new app teststack testapp --template <template-name>
```

## Ingress Management
### Create a new ingress
```
stackmgr new ingress --name testapp test.example.com
```


## Build you stack
After you have made changes to your stack, you have to build the kustomization.yaml file
```
stackmgr build teststack
```
Now you have to commit and push the changes to your git repository.