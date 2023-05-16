# HL7Parser
This project has HL7ParserService (CoreWCF Service) for validating and parsing HL7 messages and HL7ParserClient (Console Application) for interaction with service. After setting up the project, the service will be available at:
http://localhost:8282/wsd-demo , https://localhost:8283/wsd-demo

## Project Setup
### MongoDatabase Configuration
Add your database connection string, database name and collection name in MongoDB section to appsettings.json file.

### Security Configuration
Add your username and password in Security section to appsettings.json.

### Startup Configuration
In Visual Studio, right-click your solution and confgure Startup Projects to Multiple Startup Projects and select "Start" action for both projects. Next, build your solution.

## Project Run
After successful project setup, run your solution. You will be able to see two console windows, one for HL7ParserService project and the other for HL7ParserClient.

## Console Application
In your console application, you will be able to see three options:
* 0 for Connectivity Testing
* 1 for Submitting Single HL7 message from console input
* 2 for Submitting Single HL7 message from message.txt file
