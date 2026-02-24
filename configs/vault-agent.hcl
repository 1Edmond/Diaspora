vault {
  address = "http://vault:8200"
}

auto_auth {
  method {
    type = "approle"
    
    config = {
      role_id_file_path = "/vault/creds/role-id"
      secret_id_file_path = "/vault/creds/secret-id"
      remove_secret_id_file_after_reading = false
    }
  }

  sink {
    type = "file"
    config = {
      path = "/vault/token"
      mode = 0644
    }
  }
}

cache {
  use_auto_auth_token = true
}

listener "tcp" {
  address = "127.0.0.1:8007"
  tls_disable = true
}