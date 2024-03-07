terraform {
  backend "s3" {
    bucket = "techlanches-terraform-auth"
    key    = "techlanches-lambda-auth/terraform.tfstate"
    region = "us-east-1"
  }
}

provider "aws" {
  region = var.region
}