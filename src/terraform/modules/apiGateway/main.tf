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

data "aws_vpcs" "selected" {
  filter {
    name   = "isDefault"
    values = ["true"]
  }
}

data "aws_vpc" "eks_vpc" {
  id = data.aws_vpcs.selected.ids[0]
}

data "aws_subnets" "selected" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpcs.selected.ids[0]]
  }
}

data "aws_subnet" "selected" {
  for_each = toset(data.aws_subnets.selected.ids)

  id = each.value
}

resource "aws_lb" "example" {
  name               = "example"
  internal           = true
  load_balancer_type = "network"

  subnet_mapping {
    subnet_id = "subnet-08707f569968d3479"
  }
}

resource "aws_api_gateway_vpc_link" "eks_vpc_link" {
  name        = "eks-vpc-link"
  description = "VPC link to connect to Amazon EKS"

  target_arns = ["arn:aws:elasticloadbalancing:us-east-1:637423589454:loadbalancer/net/ab2dfaa502a2e408fac41e93c3dc785b/22ce980298574f2e"]
}

resource "aws_api_gateway_integration" "eks_integration" {
  rest_api_id = aws_api_gateway_rest_api.tech_lanches_api_gateweay.id
  resource_id = aws_api_gateway_method.cadastro.resource_id
  http_method = aws_api_gateway_method.cadastro.http_method

  type                    = "HTTP_PROXY"
  integration_http_method = "POST"
  uri                     = "http://ab2dfaa502a2e408fac41e93c3dc785b-22ce980298574f2e.elb.us-east-1.amazonaws.com:5050"
  connection_type         = "VPC_LINK"
  connection_id           = aws_api_gateway_vpc_link.eks_vpc_link.id
}
