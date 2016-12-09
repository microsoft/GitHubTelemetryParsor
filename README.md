# GitHubTelemetryParsor
Github Traffic API only has two weeks historic data. This tool helps to collect the data and put it into SQL database.

##Get Started
To use this tool, please follow the steps:

1. Prepare your SQL Sever.
2. Execute the (table create command)<https://github.com/Microsoft/GitHubTelemetryParsor/tree/master/TableCreation>
3. Open GithubTelemetryParse.sln in Visual Studio.
4. Define Parameter in app.config.
  * Github Account
  * Github Personal Access Token. (check (here)<https://github.com/blog/1509-personal-api-tokens> if you don't have yet)
  * SQL ADO.NET connection string
  * Repos path like "Microsoft/GitHubTelemetryParsor"
5. Build the Solution and Run
