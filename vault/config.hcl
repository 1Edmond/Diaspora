storage "file" {
  path = "/vault/data"
}

listener "tcp" {
  address       = "0.0.0.0:8200"
  tls_disable   = 1
}

api_addr = "http://vault:8200"
cluster_addr = "http://vault:8201"

ui = true
disable_mlock = false

max_lease_ttl = "768h"
default_lease_ttl = "168h"