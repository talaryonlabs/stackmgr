# stackmgr
Management CLI for Deployment Stack

## Environment Management
```
stackmgr env init <environment-name>
stackmgr env drop <environment-name>
```

### Configure environment settings
```
stackmgr env configure <environment-name> \
  --rke2-access-token
...
```

### Test environment configuration
```
stackmgr env test <environment-name>
```

## Stack Managament
```
stackmgr stack --env|--environment [environment-name] list
```

### Create/Delete stack and namespace
```
stackmgr stack --env|--environment [environment-name] new <stack-name>
stackmgr stack --env|--environment [environment-name] delete <stack-name>
```

### Add/Remove stack from ArgoCD
```
stackmgr stack --env|--environment [environment-name] enable <stack-name>
stackmgr stack --env|--environment [environment-name] disable <stack-name>
```

### Update kustomization.yaml
```
stackmgr stack --env|--environment [environment-name] migrate <stack-name>
```

## App Management
- stackmgr list-available-apps
- stackmgr list-apps --stack [stack-name]

### Create custom app
- stackmgr new-app --app [app-name] --stack [stack-name] --from-template [template-app] --dev
- stackmgr migrate-app --app [full-app-name] --stack [stack-name] --from-template [template-app] --dev
- stackmgr remove-app --app [full-app-name] --stack [stack-name]
