terraform {
  backend "s3" {
    bucket = "tech-lanches-terraform"
    key    = "tech-lanches-terraform-auth/terraform.tfstate"
    region = "us-east-1"
  }
}

provider "aws" {
  region = var.region
}
