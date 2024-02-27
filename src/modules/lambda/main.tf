resource "aws_lambda_function" "tech_lanches_lambda" {
  function_name = "tech-lanches-lambda"
  filename = "aws-lambda-net8-di-ioc.zip"
#   s3_bucket = "techchallenge-terraform-s3-auth"
#   s3_key = "TechChallenge.API/AspNetCoreFunction-CodeUri-Or-ImageUri-638350898152880179-638350898801615654.zip"
  handler = "TechLanches.API::TechLanches.API.LambdaEntryPoint::FunctionHandlerAsync"
  runtime = "dotnet6"
  role = var.arn
  tags = {
    Name = "tech-lanches-lambda"
  }
}