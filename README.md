# stackmgr
Management CLI for Deployment Stack

## Stack Managament
stackmgr list-stacks

### Create/Delete stack and namespace
stackmgr new-stack [stack-name]
stackmgr delete-stack [stack-name]

### Add/Remove stack from ArgoCD
stackmgr enable-stack [stack-name]
stackmgr disable-stack [stack-name]

### Update kustomization.yaml
stackmgr migrate-stack [app-name]

## App Management
stackmgr list-available-apps
stackmgr list-apps [stack-name]

### Create custom app
stackmgr new-app [app-name] [stack-name] --from-template [template-app]
stackmgr migrate-app [full-app-name] [stack-name] --from-template [template-app]
stackmgr remove-app [full-app-name] [stack-name]
