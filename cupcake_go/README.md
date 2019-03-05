#Cupcake go

To build the dockerfile

`docker build . -t riskfirstacr.azurecr.io/cupcake/go-svc:1.0`

To push it to azure first login (Credentials available in portal)

`docker login riskfirstacr.azurecr.io`

Then push the image

`docker push riskfirstacr.azurecr.io/cupcake/go-svc:1.0`