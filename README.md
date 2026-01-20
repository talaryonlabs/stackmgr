# stackmgr
Management CLI for Deployment Stack

## Stack Managament
- stackmgr list-stacks

### Create/Delete stack and namespace
- stackmgr new-stack [name]
- stackmgr delete-stack [name]

### Add/Remove stack from ArgoCD
- stackmgr enable-stack [name]
- stackmgr disable-stack [name]

### Update kustomization.yaml
- stackmgr migrate-stack [name]

## App Management
- stackmgr list-available-apps
- stackmgr list-apps --stack [stack-name]

### Create custom app
- stackmgr new-app --app [app-name] --stack [stack-name] --from-template [template-app] --dev
- stackmgr migrate-app --app [full-app-name] --stack [stack-name] --from-template [template-app] --dev
- stackmgr remove-app --app [full-app-name] --stack [stack-name]
