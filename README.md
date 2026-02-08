# Azure Functions Development Demo

This is a demo repository for various Azure Functions triggers and bindings. It contains few samples to demostrate how to use different triggers and bindings in Azure Functions.

It is intended for learning and experimentation purposes and it is not meant for production use. It uses local storage emulators and other development tools to simulate Azure services.

It is recommended to use this repository in conjunction with the official Azure Functions documentation and other learning resources.

It is assumed that you have some basic knowledge of Azure Functions and C# programming.

It is using Visual Studio 2026 and .NET 10, which are the latest versions as of Jan 2026.

Last, it is doing a hello world type of implementation for each trigger and binding, to keep things simple and focused on the core concepts.

## Prerequisites

- Visual Studio 2026
- .NET 10 SDK
- Azurite V3.35.0 or later (for local storage emulation)
- Microsoft Azure Storage Explorer V1.40.2 or later (optional, for inspecting storage data)


## The projects

## 1. BlobTimerFunc - C#

This project demonstrates the use of a Timer Trigger in an Azure Function using C#. The function is configured to run every 5 minutes and logs a message each time it is triggered.

It checks the given blob container (in this sample its "**images**") defined in the local.settings.json file, and logs a message indicating how many files in has.

## How it works

For a `BlobTimerFunc` to work, you provide a schedule in the form of a [cron expression](https://en.wikipedia.org/wiki/Cron#CRON_expression)(See the link for full details). A cron expression is a string with 6 separate expressions which represent a given schedule via patterns. The pattern we use to represent every 5 minutes is `0 */5 * * * *`. This, in plain text, means: "When seconds is equal to 0, minutes is divisible by 5, for any hour, day of the month, month, day of the week, or year".

## Running the project
1. Clone the repository to your local machine.
2. Open the solution in Visual Studio 2026.
3. Ensure you have Azurite running for local storage emulation (In command, run cmd like **"azurite --location "c:\azurite_data" --skipApiVersionCheck"**).
4. In Azure Storage Explorer, create a blob container named `images` in the local storage emulator.
5. Build and run the project.
6. Observe the logs in the Output window of Visual Studio to see the timer trigger in action.

## Deploy the project to Azure
In the Application Settings of the Azure Function App, set KeyVaultUri to the URI of your Key Vault, and set BlobConnectionString to the connection string of your Azure Storage account. This will allow the function to access the blob container and log the number of files it contains every 1 minute.

## 2. BlobTriggerFunc - C#
This project demonstrates the use of a Blob Trigger in an Azure Function using C#. The function is configured to trigger whenever a new blob is added to a specified container in Azure Storage. When triggered, it logs the name of the blob and its size.

## How it works
For a `BlobTriggerFunc` to work, you need to specify the path to the blob container and the blob name pattern in the function's attribute. In this sample, we use `input/{name}` which means that the function will trigger for any new blob added to the `input` container, and the `{name}` placeholder will capture the name of the blob.

## Running the project
## Running the Azurite in docker if it is not already running. If already running then ignore this step.
•	docker run -p 10000:10000 mcr.microsoft.com/azure-storage/azurite

## Running the SMTP server
•	docker run -d --name smtp4dev -p 3025:25 -p 1080:80 rnwood/smtp4dev
or
•	docker run -d --name smtp4dev -p 3025:3025 -p 1080:80 -e ServerOptions__Port=3025 rnwood/smtp4dev
(note: the default SMTP port is 25, but you can specify a different port if needed)

## Running the function
•	Press F5 to run the function in debug mode. This will create a blob container named "input" in the Azurite storage emulator.
•	If the function sends email, open smtp4dev UI at http://localhost:1080 to see the message(s).

