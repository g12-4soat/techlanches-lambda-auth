variable "environment" {
  description = "Development"
  default     = "Development"
  type        = string
}

variable "arn_lab_role" {
  description = "ARN da labRole"
  type        = string
  default     = "arn:aws:iam::590184128247:role/LabRole"
}

variable "region" {
  default     = "us-east-1"
  description = "Região default do AWS Academy"
}