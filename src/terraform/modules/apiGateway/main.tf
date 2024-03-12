resource "aws_api_gateway_rest_api" "tech_lanches_api_gateweay" {
  name = "tech-lanches-api-gateway"
  tags = {
    Name = "techlanches_apigateway"
  }
}

# resource "aws_api_gateway_resource" "proxy" {
#   rest_api_id = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
#   parent_id   = aws_api_gateway_rest_api.tech_lanches_api_gateweay.root_resource_id
#   path_part   = "{proxy+}"
# }

# resource "aws_api_gateway_method" "proxy" {
#   rest_api_id   = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
#   resource_id   = aws_api_gateway_resource.proxy.id
#   http_method   = "ANY"
#   authorization = "NONE"
# }

resource "aws_api_gateway_resource" "auth" {
  rest_api_id = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  parent_id   = aws_api_gateway_rest_api.tech_lanches_api_gateweay.root_resource_id
  path_part   = "auth"
}

resource "aws_api_gateway_method" "auth" {
  rest_api_id   = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  resource_id   = aws_api_gateway_resource.auth.id
  http_method   = "POST"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_auth" {
  rest_api_id             = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  resource_id             = aws_api_gateway_method.auth.resource_id
  http_method             = aws_api_gateway_method.auth.http_method
  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = var.arn_lambda_auth
}

resource "aws_api_gateway_resource" "cadastro" {
  rest_api_id = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  parent_id   = aws_api_gateway_rest_api.tech_lanches_api_gateweay.root_resource_id
  path_part   = "cadastro"
}

resource "aws_api_gateway_method" "cadastro" {
  rest_api_id   = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  resource_id   = aws_api_gateway_resource.cadastro.id
  http_method   = "POST"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_cadastro" {
  rest_api_id             = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  resource_id             = aws_api_gateway_method.cadastro.resource_id
  http_method             = aws_api_gateway_method.cadastro.http_method
  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = var.arn_lambda_cadastro
}

resource "aws_api_gateway_deployment" "gateway_deployment" {
  depends_on = [
    aws_api_gateway_integration.lambda_auth,
    aws_api_gateway_integration.lambda_cadastro
  ]
  rest_api_id = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  stage_name  = var.environment
}

# Saiba mais: http://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-control-access-using-iam-policies-to-invoke-api.html
resource "aws_lambda_permission" "apigateway_lambda_auth" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = var.nome_lambda_auth
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigateway_lambda_cadastro" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = var.nome_lambda_cadastro
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.execution_arn}/*/*"
}