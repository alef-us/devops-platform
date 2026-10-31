# devops-platform

[![CI](https://github.com/alef-us/devops-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/alef-us/devops-platform/actions/workflows/ci.yml)

An end-to-end DevOps portfolio project: a small ASP.NET Core API taken from source code to a monitored Kubernetes deployment, with every step automated and tested.

## Roadmap

Each step is delivered as an issue, a feature branch and a pull request that CI must pass before merge.

- [x] **Step 1. Application and CI.** ASP.NET Core minimal API (.NET 10), xUnit tests, GitHub Actions: format check, build, tests, coverage. ([#1](https://github.com/alef-us/devops-platform/issues/1))
- [ ] **Step 2. Container image.** Multi-stage non-root Dockerfile, Hadolint, container smoke test, Trivy scan, publish to GHCR. ([#2](https://github.com/alef-us/devops-platform/issues/2))
- [ ] **Step 3. Kubernetes.** Helm chart, `helm lint` and kubeconform, end-to-end deploy to a kind cluster in CI. ([#3](https://github.com/alef-us/devops-platform/issues/3))
- [ ] **Step 4. Observability.** Prometheus metrics, kube-prometheus-stack, ServiceMonitor and a Grafana dashboard. ([#4](https://github.com/alef-us/devops-platform/issues/4))
- [ ] **Step 5. Infrastructure as Code.** Terraform for Azure (AKS, ACR, Log Analytics), validated in CI; plan, apply and destroy on demand via OIDC. ([#5](https://github.com/alef-us/devops-platform/issues/5))
- [ ] **Step 6. Continuous delivery.** Versioned releases and on-demand deployment to AKS. ([#6](https://github.com/alef-us/devops-platform/issues/6))

## The API

| Endpoint | Purpose |
|---|---|
| `GET /` | Service name, version, environment and host |
| `GET /healthz` | Liveness probe: the process answers |
| `GET /readyz` | Readiness probe: dependencies are usable |
| `GET/POST /api/todos`, `GET/PUT/DELETE /api/todos/{id}` | Todo CRUD with validation (RFC 7807 problem details on errors) |

## Run locally

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/DevopsPlatform.Api        # http://localhost:5212
dotnet test                                        # unit and integration tests
dotnet format --verify-no-changes                  # the same style gate as CI
```

## License

[MIT](LICENSE)
