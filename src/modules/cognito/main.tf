resource "aws_cognito_user_pool" "tech_lanches_clientes_pool" {
  name = "tech-lanches-clientes-pool"

  username_configuration {
    case_sensitive = false
  }

  tags = {
    Name = "tech_lanches_clientes_pool"
  }
}

resource "aws_cognito_user_pool_client" "tech_lanches_clientes_pool_client" {
  name = "pool-client"

  user_pool_id = aws_cognito_user_pool.tech_lanches_clientes_pool.id
}