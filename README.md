# StackManager v2.0

**Management CLI for Kubernetes Deployment Stacks**

StackManager is a powerful command-line interface (CLI) and proxy service for managing Kubernetes deployment stacks. It simplifies the management of environments, stacks, applications, ingresses, volumes, and images across Rancher/RKE2 and ArgoCD.

---

## Features

### CLI Features
- **Environment Management**: Create, configure, and manage multiple Kubernetes environments
- **Stack Management**: Create, build, sync, and delete deployment stacks
- **Application Management**: Manage applications with template support
- **Ingress Management**: Configure and manage ingress resources with automatic TLS
- **Volume Management**: Create and manage persistent volumes and claims
- **Image Management**: Manage container images and their configurations
- **Template Support**: Create and use reusable application templates
- **Remote Proxy**: Connect to and manage remote StackManager Proxy instances
- **Auto-discovery Commands**: Automatic command discovery for all resource types

### Proxy Service Features
- **Rancher/RKE2 Integration**: Full namespace and resource management
- **ArgoCD Integration**: Application deployment and synchronization
- **Longhorn Integration**: Persistent volume management
- **Security Hardened**: Rate limiting, input validation, security headers, and sanitized error messages
- **REST API**: Comprehensive API for all CLI operations

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        StackManager CLI                         │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐ │
│  │   Commands   │  │   Services   │  │    Shared Models      │ │
│  │  (49+ cmd)   │  │  (5 services) │  │  (Stack, App, etc.)   │ │
│  └──────────────┘  └──────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     StackManager Proxy                          │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐ │
│  │  Controllers │  │  Services     │  │     Utilities         │ │
│  │  (4+ ctlr)  │  │  (4+ svc)     │  │  (Regex, Validation)  │ │
│  └──────────────┘  └──────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┬─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────┐
│  Rancher/RKE2  │   │    ArgoCD     │   │   Longhorn    │
│  (Namespace)   │   │  (Apps, Repo)  │   │  (Volumes)     │
└───────────────┘   └───────────────┘   └───────────────┘
```

---

## Installation

### CLI Tool

**NuGet Package**: `Talaryon.StackManager`

```bash
# Install the latest version
dotnet tool install --global Talaryon.StackManager

# Install a specific version
dotnet tool install --global Talaryon.StackManager --version 2.0.0

# Update to the latest version
dotnet tool update --global Talaryon.StackManager
```

**Verify Installation**:
```bash
stackmgr --version
```

### Proxy Service

**Container Image**: `ghcr.io/talaryonlabs/stackmgr-proxy`

**Quick Start with Docker**:
```bash
docker run -d \
  -p 5380:5380 \
  -e STACKMGR_ACCESS_TOKEN=your-secure-token \
  -e STACKMGR_RKE2_URL=https://your-rke2-api \
  -e STACKMGR_RKE2_ACCESS_TOKEN=base64-encoded-token \
  -e STACKMGR_RKE2_PROJECT=your-project \
  -e STACKMGR_ARGOCD_URL=https://your-argocd-api \
  -e STACKMGR_ARGOCD_ACCESS_TOKEN=base64-encoded-token \
  -e STACKMGR_ARGOCD_PROJECT=your-project \
  -e STACKMGR_LONGHORN_URL=https://longhorn-api \
  ghcr.io/talaryonlabs/stackmgr-proxy:latest
```

**Kubernetes Deployment**:
See [Deployment](#deployment) section for Kubernetes manifests.

---

## Quick Start

### 1. Create a Remote Proxy Connection

```bash
# Add a remote proxy
stackmgr remote add my-proxy https://your-proxy-domain.com --access-token your-token

# Test the connection
stackmgr remote test my-proxy

# List all remotes
stackmgr remote
```

### 2. Create an Environment

```bash
# Create a new environment
stackmgr new env dev

# Set as default for this session
stackmgr default --env dev
```

### 3. Configure Environment Settings

```bash
# Configure app repository for templates
stackmgr configure global --app-repository https://github.com/your-org/your-templates

# Configure environment settings
stackmgr configure env dev \
  --repository https://github.com/your-org/your-manifests \
  --remote my-proxy \
  --vault my-vault \
  --outpost my-outpost \
  --cert-issuer letsencrypt-prod \
  --registry-credentials my-registry-creds
```

> **Note**: Rancher/RKE2 and ArgoCD API settings are configured in the Proxy service through environment variables, not through the CLI.

### 4. Create and Manage a Stack

```bash
# Create a new stack
stackmgr new stack my-app

# Add an application to the stack
stackmgr new app --env dev --stack my-app backend --template my-template

# Configure the stack
stackmgr configure stack --env dev my-app --auto-sync true

# Build the stack (generates kustomization.yaml)
stackmgr build --stack my-app

# Sync the stack with Rancher and ArgoCD
stackmgr sync --stack my-app --apply
```

---

## Command Reference

### Main Commands

| Command | Description |
|---------|-------------|
| `new` | Create new resources (env, stack, app, ingress, volume, image) |
| `get` | Get/list resources |
| `describe` | Show detailed information about a resource |
| `configure` | Configure resource settings |
| `delete` | Delete resources |
| `migrate` | Migrate app or image resources |
| `build` | Build stack manifests (kustomization.yaml) |
| `sync` | Sync stack with remote (Rancher + ArgoCD) |
| `default` | Set default environment/stack for the session |
| `remote` | Manage remote proxy connections |

### Resource Types

| Resource | Description |
|----------|-------------|
| `env` / `environment` | Kubernetes environment with API connections |
| `stack` | Deployment stack containing apps, ingresses, volumes |
| `app` | Application within a stack |
| `ingress` | Ingress resource for HTTP routing |
| `volume` | Persistent volume for storage |
| `image` | Container image configuration |

---

## Usage Examples

### Environment Management

```bash
# Create a new environment
stackmgr new env staging

# List all environments
stackmgr get env

# Describe an environment
stackmgr describe env dev

# Configure environment
stackmgr configure env dev \
  --repository https://github.com/org/manifests \
  --remote my-proxy \
  --vault my-vault \
  --outpost my-outpost \
  --cert-issuer letsencrypt-prod \
  --registry-credentials my-registry-creds

# Delete an environment
stackmgr delete env old-env
```

### Stack Management

```bash
# Create a new stack
stackmgr new stack production

# List all stacks in an environment
stackmgr get stack --env dev

# Describe a stack
stackmgr describe stack --env dev my-stack

# Configure a stack
stackmgr configure stack --env dev my-stack --auto-sync true

# Build stack manifests
stackmgr build --stack my-stack

# Sync stack with remote (dry-run)
stackmgr sync --stack my-stack

# Sync and apply changes
stackmgr sync --stack my-stack --apply

# Delete a stack
stackmgr delete stack --env dev old-stack
```

### Application Management

```bash
# Create an empty app
stackmgr new app --env dev --stack my-stack frontend

# Create an app from template
stackmgr new app --env dev --stack my-stack backend --template my-template

# Create an app from template with dev branch
stackmgr new app --env dev --stack my-stack api --template my-api --dev

# List all apps in a stack
stackmgr get app --env dev --stack my-stack

# Describe an app
stackmgr describe app --env dev --stack my-stack my-app

# Configure app parameters
stackmgr configure app --env dev --stack my-stack my-app \
  --param key1=value1 \
  --param key2=value2

# Migrate app from its template
stackmgr migrate app --env dev --stack my-stack my-app

# Delete an app
stackmgr delete app --env dev --stack my-stack old-app
```

### Ingress Management

```bash
# Create a new ingress
stackmgr new ingress --env dev --stack my-stack --app my-app --port 80 app.example.com

# Create an ingress with annotations
stackmgr new ingress --env dev --stack my-stack --app my-app --port 80 api.example.com \
  --annotation "nginx.ingress.kubernetes.io/rewrite-target=/$"

# Create a secured ingress (HTTPS)
stackmgr new ingress --env dev --stack my-stack --app my-app --port 443 secure.example.com --secured

# List all ingresses
stackmgr get ingress --env dev --stack my-stack

# Describe an ingress
stackmgr describe ingress --env dev --stack my-stack my-ingress

# Delete an ingress
stackmgr delete ingress --env dev --stack my-stack old-ingress
```

### Volume Management

```bash
# Create a new volume
stackmgr new volume --stack my-stack --env dev data-volume --size 10Gi --access-mode ReadWriteOnce

# List all volumes
stackmgr get volume --env dev --stack my-stack

# Describe a volume
stackmgr describe volume --env dev --stack my-stack data-volume

# Delete a volume
stackmgr delete volume --env dev --stack my-stack old-volume
```

### Image Management

```bash
# Add a new image configuration
stackmgr new image --stack my-stack --env dev my-image:latest

# List all images
stackmgr get image --env dev --stack my-stack

# Describe an image
stackmgr describe image --env dev --stack my-stack my-image

# Migrate image to a new version
stackmgr migrate image --env dev --stack my-stack --name my-image new-image:latest

# Delete an image
stackmgr delete image --env dev --stack my-stack old-image
```

### Template Management

```bash
# List available templates
stackmgr get template

# Describe a template
stackmgr describe template my-template
```

### Remote Proxy Management

```bash
# Add a remote proxy
stackmgr remote add production https://stackmgr-proxy.prod.com --access-token xxx

# Set access token for a remote
stackmgr remote set production --access-token new-token

# Test a remote connection
stackmgr remote test production

# Remove a remote
stackmgr remote remove old-proxy

# List all remotes
stackmgr remote

# Generate deployment manifest (WIP)
stackmgr remote generate production proxy.example.com --cert-issuer letsencrypt-prod
```

### Default Settings

```bash
# Set default environment for the session
stackmgr default --env dev

# Set default stack for the session
stackmgr default --stack my-stack

# Set both in one command
stackmgr default --env dev --stack my-stack

# View current defaults
stackmgr default
```

---

## Configuration

### Local Configuration File

Configuration is stored in `~/.stackmgr/local.json` with encrypted access tokens.

**Example Configuration**:
```json
{
  "app_repository": "https://github.com/org/templates",
  "remotes": [
    {
      "name": "production",
      "url": "https://stackmgr-proxy.prod.com",
      "access_token": "<encrypted>"
    }
  ],
  "defaults": {
    "environment": "dev",
    "stack": "my-stack"
  },
  "debug_mode": false
}
```

### Environment Configuration

Each environment has a `.env.yaml` file:

```yaml
name: dev
version: environment.talaryon.io/v1beta
vault: my-vault
outpost: my-outpost
certIssuer: letsencrypt-prod
registryCredentials: my-registry-creds
repository: https://github.com/org/manifests
remote: my-proxy
```

### Stack Configuration

Each stack has a `.stack.yaml` file:

```yaml
name: my-app
version: stack.talaryon.io/v1beta
namespace: my-namespace
enableAutoSync: true
images: []
apps: []
ingresses: []
volumes: []
```

---

## Deployment

### Proxy Service Deployment

Kubernetes manifests are available in the `deployment/` directory:

```yaml
# deployment.proxy.yaml - Deployment
# svc.proxy.yaml - Service (ClusterIP:5380)
# ingress.proxy.yaml - Ingress with TLS
# configMap.proxy.yaml - Configuration
# secrets.proxy.yaml - Secrets
```

**Apply all manifests**:
```bash
kubectl apply -f deployment/deployment.proxy.yaml \
              -f deployment/svc.proxy.yaml \
              -f deployment/configMap.proxy.yaml \
              -f deployment/secrets.proxy.yaml \
              -f deployment/ingress.proxy.yaml
```

**Required Environment Variables**:
```
STACKMGR_ACCESS_TOKEN          # Proxy access token
STACKMGR_RKE2_URL              # Rancher/RKE2 API URL
STACKMGR_RKE2_ACCESS_TOKEN     # Base64 encoded RKE2 token
STACKMGR_RKE2_PROJECT         # RKE2 project ID
STACKMGR_ARGOCD_URL            # ArgoCD API URL
STACKMGR_ARGOCD_ACCESS_TOKEN  # Base64 encoded ArgoCD token
STACKMGR_ARGOCD_PROJECT       # ArgoCD project
STACKMGR_LONGHORN_URL          # Longhorn API URL
```

---

## Project Structure

```
stackmgr/
├── src/
│   ├── StackManager.CLI/          # CLI application
│   │   ├── Commands/              # 49+ command classes
│   │   │   ├── Environments/     # Environment commands
│   │   │   ├── Stacks/           # Stack commands
│   │   │   ├── Apps/             # Application commands
│   │   │   ├── Ingresses/        # Ingress commands
│   │   │   ├── Volumes/          # Volume commands
│   │   │   ├── Images/           # Image commands
│   │   │   ├── Templates/        # Template commands
│   │   │   └── Resources/        # Generic resource commands
│   │   ├── Arguments/             # CLI argument definitions
│   │   ├── Options/               # CLI option definitions
│   │   ├── Models/                # Data models
│   │   │   └── Kubernetes/        # Kubernetes resource models
│   │   ├── Services/              # CLI services
│   │   ├── Builder/               # Stack builder
│   │   ├── Exceptions/            # Custom exceptions
│   │   ├── Extensions/            # Extension methods
│   │   └── Serialization/         # YAML serialization
│   │
│   ├── StackManager.Proxy/        # Proxy service
│   │   ├── Controller/            # API controllers
│   │   ├── Services/              # Service implementations
│   │   ├── Models/                # API models
│   │   └── Utilities/             # Utility classes
│   │
│   └── StackManager.Shared/       # Shared library
│       └── Models/                # Shared data models
│
├── deployment/                    # Kubernetes manifests
│   ├── deployment.proxy.yaml
│   ├── svc.proxy.yaml
│   ├── ingress.proxy.yaml
│   ├── configMap.proxy.yaml
│   └── secrets.proxy.yaml
│
├── tests/                         # Unit tests
├── Changes.md                     # Change log
├── LICENSE
├── NOTICE
└── README.md
```

---

## Security

The StackManager Proxy v2.0 includes comprehensive security improvements:

- **Input Validation**: All API endpoints validate input data
- **Rate Limiting**: 100 requests per minute per client
- **Security Headers**: X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, CSP
- **Error Message Sanitization**: Sensitive details removed from error responses
- **Request Size Limits**: 1MB maximum request size
- **Encrypted Configuration**: Access tokens are encrypted at rest
- **File Permissions**: Restricted file/directory permissions on Unix systems

---

## Versioning

StackManager follows semantic versioning:

- **Major**: Breaking changes, significant new features
- **Minor**: Backward-compatible new features
- **Patch**: Bug fixes, small improvements

Version information is managed via GitVersion with continuous delivery mode.

---

## License

This project is licensed under the terms of the license file included in the repository.

---

## Support

For issues, questions, or contributions, please visit the project repository:
- **Repository**: https://github.com/talaryonlabs/stackmgr
- **Issues**: https://github.com/talaryonlabs/stackmgr/issues
- **Package**: https://nuget.pkg.talaryon.dev/packages/talaryon.stackmanager/
- **Container**: http://ghcr.io/talaryonlabs/stackmgr-proxy

---

## Contributing

Contributions are welcome! Please ensure:
1. All tests pass
2. Code follows existing style and patterns
3. New features include appropriate tests
4. Documentation is updated for new features

---

*Built with .NET 10.0 by Talaryon Labs*
