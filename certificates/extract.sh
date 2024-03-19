#/bin/sh

openssl pkcs12 -in $1.pfx -nocerts -nodes -out $1.key
openssl rsa -in $1.key -out $1-private.key
openssl rsa -in $1.key -pubout -out $1-public.key