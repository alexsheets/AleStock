AleStock is a portfolio project I am currently working on to combine skills of web development and ML programming to pursue an interest in stocks.

Current Buildout:

--> Users can supply their own API key for multiple websites (SimFin, Marketstack) to view stock information. Deployed using PythonNET which can run Python code within .NET projects. Requests use urllib package.

--> Stock finance reports are stored in database after being requested once in order for quicker retrieval/less network requests.

--> DB linked to free database service Supabase. Project contains full setup of Supabase connection in an ASP.NET project which proved difficult to find online. Includes login and registration to use Supabase auth system as usage of the Supabase project is not authorized without being saved to the associated users table.

--> Chat built using C# SignalR package. Currently allows for general messaging, private messaging, and group messaging.

In Progress:

--> Implement AI assistance

--> Finishing up addition of Marketstack API

--> Adding functionality for ML regressions and predictions

Future Ideas:

--> Adding Yahoo API for stock information and news

--> Adding stock backtesting

--> Would like to build out a react chat application in the future to be on top of the ASP.NET project

--> Adding 'paper trading' ability
