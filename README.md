## About the app

You can use this application to sort an integer array. The software service consists of three operations:<br/>
- Enqueue: Send a request with an input array. If your input is successfully enqueued for sorting you should get an Id of the operation.
- Get Jobs: You can retrieve a list of all running and completed operations.
- Get Job: After you have enqueued your input array you get the <mark>Id</mark> for it. You can use that Id to retrive the results and status of the operation.

## Running the app

This solution is designed very simple and does not depend on any external infrastructure. You can just open up <mark>Exercise.sln</mark> on visual studio and run PublicApi web project.

#### To generate data:

You can use <mark>Client</mark> project to execute some requests with random arrays. By default the project makes 1000 concurrent connections and 100 requests per connections. You can change these values using <mark>ConcurrentWorkers</mark> and <mark>RequestsPerWorker</mark> configuration values. Also if you change <mark>PublicApi</mark>'s port or host it somewhere else and still want to use this application you can change <mark>RemoteAddress</mark> value in the config.

### Endpoints

* /Queue/enqueue POST  - example request body: 

    ```
    [  10, 0, 9, 1, 8, 2, 7, 3, 6, 4, 5 ]
    ```
* /Queue/get-jobs GET  - example request query: 

    ```
    /Queue/get-jobs?PageNumber=0&PageSize=500
    ```
* /Queue/get-jobs GET  - example request query: 

    ```
    /Queue/get-job?jobId=1cf54c56-b237-4930-8b8d-561d9b473f3a
    ```
We might be handling too many requests to retrieve from server. So, pagination is added for get-jobs operation.
### Authentication and Authorization

For the sake of simplicity, only two users are defined:
- admin:admin with roles: admin, user
- john:john with roles: user

Here are the rules:
- "Enqueue" operation may be executed by any user,
- "Get Jobs" operation may only be executed by admin role,
- "Get Job" operation do not require any authentication.
- To make a request you should first invoke login:
    ```
    /login
    {
      "userName": "admin",
      "password": "admin"
    }
    ```
You can use returned token as a bearer token.
#### To disable or enable authentication and authorization:
Just change UseAuthentication config value to true or false.
By default it is disabled.
### Run unit-tests

You can either run the unit tests from terminal with these commands:
```
cd tests/UnitTests
dotnet test
```
Or use Test Explorer on Visual Studio. <br/><br/>
The main purpose of this project is to demonstrate the fundemantals of dotnet WebApi and multithreaded queue processing.<br/>

Please feel free to ask if you have any questions.