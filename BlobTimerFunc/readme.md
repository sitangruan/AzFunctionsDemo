# BlobTimerFunc - C<span>#</span>

This project demonstrates the use of a Timer Trigger in an Azure Function using C#. The function is configured to run every 5 minutes and logs a message each time it is triggered.

It checks the given blob container (in this sample its "**images**") defined in the local.settings.json file, and logs a message indicating how many files in has.

## How it works

For a `BlobTimerFunc` to work, you provide a schedule in the form of a [cron expression](https://en.wikipedia.org/wiki/Cron#CRON_expression)(See the link for full details). A cron expression is a string with 6 separate expressions which represent a given schedule via patterns. The pattern we use to represent every 5 minutes is `0 */5 * * * *`. This, in plain text, means: "When seconds is equal to 0, minutes is divisible by 5, for any hour, day of the month, month, day of the week, or year".

## Running the project
1. Clone the repository to your local machine.
2. Open the solution in Visual Studio 2026.
3. Ensure you have Azurite running for local storage emulation (In command, run cmd like **"azurite --location c:\azurite_data"**).
4. In Azure Storage Explorer, create a blob container named `images` in the local storage emulator.
5. Build and run the project.
6. Observe the logs in the Output window of Visual Studio to see the timer trigger in action.