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

## Local emulation
In your local storage emulator (Azurite), create a blob container named `images2`. Then, add a new blob to the `images2` container. This will trigger the function, and you should see the logs in the Output window of Visual Studio indicating the name and size of the new blob.
