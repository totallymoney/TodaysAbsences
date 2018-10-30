#!/bin/bash

set -e

if [ $# -ne 1 ]; then
    echo "$0": usage: deploy.sh ENVIRONMENT
    echo "$0": eg: deploy.sh stage
    echo "$0": eg: deploy.sh production
    exit 1
fi

ENVIRONMENT=$1

yarn run sls deploy --stage "$ENVIRONMENT" --force --verbose