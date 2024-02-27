terraform {
   backend "s3" {
    bucket = "techlanches-terraform"
    key    = "techlanches-lambda-auth/terraform.tfstate"
    region = "us-east-1"
  }
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.6.2"
    }
  }
}

provider "aws" {
  region  = var.region
}