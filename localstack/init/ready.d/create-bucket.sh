#!/bin/bash
# Runs inside the LocalStack container once it is ready: creates the bucket the API uses.
set -e
BUCKET="${BUCKETNAME:-salessystem-reports}"
if awslocal s3api head-bucket --bucket "$BUCKET" 2>/dev/null; then
  echo "Bucket $BUCKET already exists"
else
  awslocal s3 mb "s3://$BUCKET"
  echo "Bucket $BUCKET created"
fi
