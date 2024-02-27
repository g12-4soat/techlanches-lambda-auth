resource "aws_api_gateway_rest_api" "tech_lanches_api_gateweay" {
  name = "tech-lanches-api-gateway"
  tags = {
    Name = "techlanches_apigateway"
  }
}

resource "aws_api_gateway_documentation_part" "documentation_part" {
  location {
    type = "API"
  }

  properties  = "{\"description\":\"TechChallenge_APIGateway\"}"
  rest_api_id = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
}

resource "aws_api_gateway_documentation_version" "documentation" {
  version = "1.0"
  rest_api_id = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  description = "TechChallenge - APIGateway"
  depends_on  = [aws_api_gateway_documentation_part.documentation_part]
}

resource "aws_api_gateway_resource" "proxy" {
  rest_api_id = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.id}"
  parent_id   = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.root_resource_id}"
  path_part   = "{proxy+}"
}

resource "aws_api_gateway_method" "proxy" {
  rest_api_id   = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.id}"
  resource_id   = "${aws_api_gateway_resource.proxy.id}"
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda" {
  rest_api_id = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.id}"
  resource_id = "${aws_api_gateway_method.proxy.resource_id}"
  http_method = "${aws_api_gateway_method.proxy.http_method}"
  integration_http_method = "POST"
  type = "AWS_PROXY"
  uri = "${var.arn}"
}

resource "aws_api_gateway_method" "proxy_root" {
  rest_api_id = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.id}"
  resource_id = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.root_resource_id}"
  http_method = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_root" {
  rest_api_id = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.id}"
  resource_id = "${aws_api_gateway_method.proxy_root.resource_id}"
  http_method = "${aws_api_gateway_method.proxy_root.http_method}"
  integration_http_method = "POST"
  type = "AWS_PROXY"
  uri = "${var.arn}"
}

resource "aws_api_gateway_deployment" "gateway_deployment" {
  depends_on = [
    aws_api_gateway_integration.lambda,
    aws_api_gateway_integration.lambda_root,
  ]
  rest_api_id = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.id}"
  stage_name = var.environment
}

# Saiba mais: http://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-control-access-using-iam-policies-to-invoke-api.html
resource "aws_lambda_permission" "apigateway_lambda" {
  statement_id = "AllowAPIGatewayInvoke"
  action = "lambda:InvokeFunction"
  function_name = var.nome_lambda
  principal = "apigateway.amazonaws.com"
  source_arn = "${aws_api_gateway_rest_api.tech_lanches_api_gateweay.execution_arn}/*/*"
}