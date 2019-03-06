You can use helm with this settings file to install postgres and create the test database.

`helm install --name service-test-postgres -f values.yaml stable/postgresql`

It also sets up the user name and password to correspond with that in the services. Clearly this is not top security stuff!
