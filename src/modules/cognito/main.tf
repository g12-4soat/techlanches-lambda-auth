resource "aws_cognito_user_pool" "tech_lanches_clientes_pool" {
  name                     = "tech-lanches-clientes-pool"
  mfa_configuration        = "OFF"
  alias_attributes         = ["email", "preferred_username"]
  auto_verified_attributes = ["email"]

  username_configuration {
    case_sensitive = false
  }

  tags = {
    Name = "tech_lanches_clientes_pool"
  }
}

resource "aws_cognito_user_pool_client" "tech_lanches_clientes_pool_client" {
  name                = "pool-client"
  explicit_auth_flows = ["ALLOW_REFRESH_TOKEN_AUTH", "ALLOW_USER_SRP_AUTH", "ALLOW_ADMIN_USER_PASSWORD_AUTH"]
  user_pool_id        = aws_cognito_user_pool.tech_lanches_clientes_pool.id
}