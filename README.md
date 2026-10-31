# devops-platform

An end-to-end DevOps portfolio project: a small ASP.NET Core API taken from source code to a monitored Kubernetes deployment, with every step automated and tested.

## Roadmap

Each step is delivered as an issue, a feature branch and a pull request that CI must pass before merge.

- [ ] **Step 1. Application and CI.** ASP.NET Core minimal API (.NET 10), xUnit tests, GitHub Actions: format check, build, tests, coverage.
- [ ] **Step 2. Container image.** Multi-stage non-root Dockerfile, Hadolint, container smoke test, Trivy scan, publish to GHCR.
- [ ] **Step 3. Kubernetes.** Helm chart, `helm lint` and kubeconform, end-to-end deploy to a kind cluster in CI.
- [ ] **Step 4. Observability.** Prometheus metrics, kube-prometheus-stack, ServiceMonitor and a Grafana dashboard.
- [ ] **Step 5. Infrastructure as Code.** Terraform for Azure (AKS, ACR, Log Analytics), validated in CI; plan, apply and destroy on demand via OIDC.
- [ ] **Step 6. Continuous delivery.** Versioned releases and on-demand deployment to AKS.

## License

[MIT](LICENSE)
