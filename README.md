# CupCake
Various bits and bobs concerning attempts to make a scalable service that pulls large data sets from a database and returns them.

Write a simple service that reads 1 million rows from a postgres database (a postgres function is provided `public.get_data()`) and returns them to a client. 

So far my test environment is postgres running in Kubernetes on a MacBook Pro with the services run outside of K8s.

A client is provided - the tests below are from the client making 50 concurrent async calls to the service.

Example client call: `dotnet run bin/Debug/netcoreapp2.2/client.dll -s Go -n 50 -l`

### Current Results

Test case | Avg time to response | Avg time to first record | Avg time to all | Server Memory Usage (est) | Server CPU (est)
------------ | ------------- | ------------- | ------------- | ------------- | -------------
ASP.NET Core 2.2, load data into List<string> |115s|115s|128s|5.6GB|150s
ASP.NET Core 2.2, streaming |57s|57s|104s|170MB|77s
Go|44s|44s|108s|8.6MB|105s
 
