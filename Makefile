# Terraform runner via Docker Compose

apply-keycloak:
	@echo "🚀 Applying Keycloak module..."
	TARGET_DIR=keycloak docker-compose -f docker-compose.terraform.yml up --build --abort-on-container-exit

apply-rabbitmq:
	@echo "🚀 Applying RabbitMQ module..."
	TARGET_DIR=rabbitmq docker-compose -f docker-compose.terraform.yml up --build --abort-on-container-exit

apply-all:
	@echo "🚀 Applying both Keycloak and RabbitMQ modules..."
	TARGET_DIR=all docker-compose -f docker-compose.terraform.yml up --build --abort-on-container-exit

clean:
	@echo "🧹 Cleaning up Terraform container..."
	docker-compose -f docker-compose.terraform.yml down
