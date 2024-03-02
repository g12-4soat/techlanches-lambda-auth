resource "aws_lambda_function" "tech_lanches_lambda" {
  function_name = "tech-lanches-lambda"
  # s3_bucket = "techlanches-terraform"
  # s3_key = "techlanches-lambda-auth/aws-lambda-net8-di-ioc.zip"
  filename = "Serverless/AWSServerless1/bin/Release/net8.0/AWSServerless1.zip"
  handler = "TechLanchesLambda::TechLanchesLambda.Functions_LambdaAuth_Generated::LambdaAuth"
  runtime = "dotnet8"
  role = var.arn
  tags = {
    Name = "tech-lanches-lambda"
  }
  timeout = 30
  memory_size = 512
}