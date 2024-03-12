output "grupo_usuario_id" {
  description = "Id do grupo de usuário"
  value       = aws_cognito_user_pool.tech_lanches_clientes_pool.id
}

output "client_pool_id" {
  description = "Id do client pool"
  value       = aws_cognito_user_pool_client.tech_lanches_clientes_pool_client.id
}