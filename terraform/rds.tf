resource "aws_db_subnet_group" "main" {
  name        = "notism-db-subnet-${replace(aws_vpc.main.id, "vpc-", "")}"
  description = "Notism RDS subnet group"
  subnet_ids  = aws_subnet.private[*].id

  tags = {
    Name = "notism-db-subnet"
  }
}

resource "aws_db_instance" "main" {
  identifier     = "notism-db"
  engine         = "postgres"
  engine_version = "16.13"
  instance_class = "db.t4g.micro"

  allocated_storage     = 20
  storage_type          = "gp3"
  db_name               = "notism_db"
  username              = "notismadmin"
  password              = var.db_password
  port                  = 5432

  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  publicly_accessible    = false
  multi_az               = false

  skip_final_snapshot       = true
  deletion_protection       = false
  backup_retention_period   = 1
  apply_immediately         = true

  tags = {
    Name = "notism-db"
  }
}
