# =============================================================================
# Monitoring Module — kube-prometheus-stack + Grafana via Helm
# =============================================================================

resource "helm_release" "prometheus_stack" {
  name             = "kube-prometheus-stack"
  repository       = "https://prometheus-community.github.io/helm-charts"
  chart            = "kube-prometheus-stack"
  version          = "55.5.0"
  namespace        = "monitoring"
  create_namespace = true
  timeout          = 600

  values = [
    yamlencode({
      grafana = {
        adminPassword = var.grafana_admin_pass
        persistence   = { enabled = true, size = "5Gi" }
        sidecar       = { dashboards = { enabled = true } }
      }
      prometheus = {
        prometheusSpec = {
          retention      = "30d"
          storageSpec    = { volumeClaimTemplate = { spec = { resources = { requests = { storage = "50Gi" } } } } }
        }
      }
      alertmanager = {
        alertmanagerSpec = {
          storage = { volumeClaimTemplate = { spec = { resources = { requests = { storage = "5Gi" } } } } }
        }
      }
    })
  ]
}

resource "helm_release" "cert_manager" {
  name             = "cert-manager"
  repository       = "https://charts.jetstack.io"
  chart            = "cert-manager"
  version          = "v1.13.0"
  namespace        = "cert-manager"
  create_namespace = true

  set {
    name  = "installCRDs"
    value = "true"
  }
}

resource "helm_release" "ingress_nginx" {
  name             = "ingress-nginx"
  repository       = "https://kubernetes.github.io/ingress-nginx"
  chart            = "ingress-nginx"
  version          = "4.8.0"
  namespace        = "ingress-nginx"
  create_namespace = true
}
