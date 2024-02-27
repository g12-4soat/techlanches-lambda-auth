terraform {
   backend "s3" {
    bucket = "techlanches-terraform"
    key    = "techlanches-infra-db/terraform.tfstate"
    region = var.region
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