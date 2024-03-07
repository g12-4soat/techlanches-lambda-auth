resource "aws_lambda_function" "tech_lanches_lambda_auth" {
  function_name = "tech-lanches-lambda-auth"
  filename      = "Serverless/auth_lambda.zip"
  handler       = "TechLanchesLambda::TechLanchesLambda.Functions_LambdaAuth_Generated::LambdaAuth"
  runtime       = "dotnet8"
  role          = var.arn
  tags = {
    Name = "tech-lanches-lambda"
  }
  timeout     = 30
  memory_size = 512
}

resource "aws_lambda_function" "tech_lanches_lambda_cadastro" {
  function_name = "tech-lanches-lambda-cadastro"
  filename      = "Serverless/auth_lambda.zip"
  handler       = "TechLanchesLambda::TechLanchesLambda.Functions_LambdaCadastro_Generated::LambdaCadastro"
  runtime       = "dotnet8"
  role          = var.arn
  tags = {
    Name = "tech-lanches-lambda"
  }
  timeout     = 30
  memory_size = 512
}