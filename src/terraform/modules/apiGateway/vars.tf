variable "arn_lambda_auth" {
  description = "ARN da Lambda auth"
}

variable "arn_lambda_cadastro" {
  description = "ARN da Lambda Cadastro"
}

variable "environment" {
  description = "Deployment environment"
}

variable "nome_lambda_auth" {
  description = "Nome da Lambda Auth"
}

variable "nome_lambda_cadastro" {
  description = "Nome da Lambda Cadastro"
}
variable "load_balancer_dns" {
  default = "a304428c8c3b9415f882254828917660-1346e20a00e4ec45.elb.us-east-1.amazonaws.com"
}

variable "load_balancer_arn" {
  default = "arn:aws:elasticloadbalancing:us-east-1:637423589454:loadbalancer/net/a304428c8c3b9415f882254828917660/1346e20a00e4ec45"
}
