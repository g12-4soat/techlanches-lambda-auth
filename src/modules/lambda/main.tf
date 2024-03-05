resource "aws_lambda_function" "tech_lanches_lambda" {
  function_name = "tech-lanches-lambda"
  filename = "Serverless/auth_lambda.zip"
  handler = "TechLanchesLambda::TechLanchesLambda.Functions_LambdaAuth_Generated::LambdaAuth"
  runtime = "dotnet8"
  role = var.arn
  tags = {
    Name = "tech-lanches-lambda"
  }
  timeout = 30
  memory_size = 512
}