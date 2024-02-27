module "lambda" {
  source = "./modules/lambda"
  arn = var.arn_lab_role
}

module "apiGateway" {
  source = "./modules/apiGateway"
  arn = module.lambda.lambda_arn
  environment = var.environment
  nome_lambda = module.lambda.nome_lambda
}

module "cognito" {
  source = "./modules/cognito"
}