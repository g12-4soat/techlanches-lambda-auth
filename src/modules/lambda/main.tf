resource "aws_lambda_function" "tech_lanches_lambda" {
  function_name = "tech-lanches-lambda"
  s3_bucket = "techlanches-terraform"
  s3_key = "techlanches-lambda-auth/aws-lambda-net8-di-ioc.zip"
  handler = "TechLanches.API::TechLanches.API.LambdaEntryPoint::FunctionHandlerAsync"
  runtime = "dotnet6"
  role = var.arn
  tags = {
    Name = "tech-lanches-lambda"
  }
}