FROM hashicorp/terraform:1.6

RUN apk add --no-cache bash curl git jq make

WORKDIR /infra

COPY entrypoint.sh /entrypoint.sh
COPY ../../Makefile /infra/Makefile

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
