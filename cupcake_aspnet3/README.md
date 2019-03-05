# ASP.NET Core v3 preview 2
An attempt to use Async Streams. Needed to add a hacky home brew JSON formatter because the built in one failed.

I'm sure this one is going to get a lot better

---

## Pre-requisites

.NET Core 3.0-proview2 sdk

---

## Dockerfile

To build the dockerfile

`docker build . -t riskfirstacr.azurecr.io/cupcake/aspnet3-async-enumerable-svc:1.0`

To push it to azure first login (Credentials available in portal)

`docker login riskfirstacr.azurecr.io`

Then push the image

`docker push riskfirstacr.azurecr.io/cupcake/aspnet3-async-enumerable-svc:1.0`