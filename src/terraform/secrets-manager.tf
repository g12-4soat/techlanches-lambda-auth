resource "aws_secretsmanager_secret" "lambda_auth_credentials" {
  name                    = var.secrets-name
  recovery_window_in_days = 0
}

resource "aws_secretsmanager_secret_version" "lambda_auth_credentials" {
  secret_id = aws_secretsmanager_secret.lambda_auth_credentials.id
  secret_string = jsonencode({
    Region : var.region,
    UserPoolId : module.cognito.grupo_usuario_id,
    UserPoolClientId : module.cognito.client_pool_id,
    UserTechLanches : var.user-techlanches,
    EmailDefault : var.email-default
    PasswordDefault : var.password-default,
  })
}
