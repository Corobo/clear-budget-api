FROM hashicorp/terraform:1.6

RUN apk add --no-cache bash curl git jq make

WORKDIR /infra

COPY infra/docker/terraform-runner/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
