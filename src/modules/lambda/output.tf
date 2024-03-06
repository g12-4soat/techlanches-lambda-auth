output "lambda_arn" {
  description = "ARN da lambda"
  value       = aws_lambda_function.tech_lanches_lambda.invoke_arn
}

output "nome_lambda" {
  description = "Nome da Lambda"
  value       = aws_lambda_function.tech_lanches_lambda.function_name
}