/// <summary>
/// AI PROMPT (for documentation):
/// "Create the complete 'DalTest/Program.cs' file based on all rules in Chapter11.
/// - It must include a main loop and sub-menus for all entities.
/// - It must use Enums for menu choices.
/// - All DAL calls must be wrapped in 'try-catch' blocks.
/// - All user input must use 'TryParse' (11d, Sec8).
/// - Implement the 'Create' (manual input) for the 'Courier' entity fully.
/// - All XML documentation comments (///) must be in English."
/// </summary>
/// <remarks>
/// This XML block documents the original task / prompt for maintainers. Keep it current
/// so future contributors understand why this test harness exists and which rules it follows.
/// </remarks>

using DalApi;
using DO;
using Dal;

namespace DalTest;

/// <summary>
/// Defines the options for the main application menu.
/// </summary>
/// <remarks>
/// Each enum value maps to a top-level menu action in the interactive console.
/// Use these values when parsing user input in the main loop.
/// </remarks>
public enum MainMenuOptions
{
    Exit = 0,
    Courier = 1,
    Order = 2,
    Delivery = 3,
    Configuration = 4,
    InitializeData = 5,
    ShowAllData = 6,
    ResetAllData = 7
}

/// <summary>
/// Defines the CRUD operations available in the entity sub-menus.
/// </summary>
/// <remarks>
/// The values correspond to the sub-menu options shown to the user.
/// Use TryParse to convert numeric input into this enum safely.
/// </remarks>
public enum CrudMenuOptions
{
    Return = 0,
    Create = 1,
    ReadSingle = 2,
    ReadAll = 3,
    Update = 4,
    DeleteSingle = 5,
    DeleteAll = 6
}

/// <summary>
/// Main program class for manually testing the DAL layer.
/// (Implements Stage 1, Chapter 11)
/// </summary>
/// <remarks>
/// This program is a developer/test harness that exercises DAL implementations.
/// It intentionally performs simple console-based CRUD operations to validate DAL behavior.
/// </remarks>
public static class Program
{
    /// <summary>
    /// DAL configuration interface instance used to read and mutate global settings.
    /// </summary>
    /// <remarks>
    /// The configuration object exposes the system clock and other global parameters
    /// used by initialization and certain helper methods in this test harness.
    /// </remarks>
    private static IConfig s_dalIConfig = new ConfigImplementation();

    /// <summary>
    /// DAL courier interface instance used to create/read/update/delete couriers.
    /// </summary>
    private static ICourier s_dalICourier = new CourierImplementation();

    /// <summary>
    /// DAL order interface instance used to create/read/update/delete orders.
    /// </summary>
    private static IOrder s_dalIOrder = new OrderImplementation();

    /// <summary>
    /// DAL delivery interface instance used to create/read/update/delete deliveries.
    /// </summary>
    private static IDelivery s_dalIDelivery = new DeliveryImplementation();

    /// <summary>
    /// Program entry point and main loop.
    /// </summary>
    /// <param name="args">Command-line arguments (ignored by this harness).</param>
    /// <remarks>
    /// The main loop prints a top-level menu, reads numeric input using TryParse,
    /// and dispatches to sub-menus. All DAL calls are wrapped in try/catch to show errors.
    /// </remarks>
    public static void Main(string[] args)
    {
        try
        {
            s_dalIConfig.Clock = new DateTime(2025, 10, 15, 10, 0, 0);

            MainMenuOptions choice = MainMenuOptions.Exit;
            do
            {
                Console.WriteLine("\n--- Main Menu (Data Layer Test) ---");
                Console.WriteLine("0: Exit");
                Console.WriteLine("1: Courier Management (CRUD)");
                Console.WriteLine("2: Order Management (CRUD)");
                Console.WriteLine("3: Delivery Management (CRUD)");
                Console.WriteLine("4: Configuration Settings");
                Console.WriteLine("5: Initialize All Data (Initialization.Do)");
                Console.WriteLine("6: Show All Entities");
                Console.WriteLine("7: Reset All Data (DeleteAll + Config.Reset)");

                Console.Write("\nEnter your choice: ");

                if (!int.TryParse(Console.ReadLine(), out int numChoice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }

                choice = (MainMenuOptions)numChoice;

                switch (choice)
                {
                    case MainMenuOptions.Courier:
                        ShowCourierMenu(s_dalICourier);
                        break;
                    case MainMenuOptions.Order:
                        ShowOrderMenu(s_dalIOrder);
                        break;
                    case MainMenuOptions.Delivery:
                        ShowDeliveryMenu(s_dalIDelivery);
                        break;
                    case MainMenuOptions.Configuration:
                        ShowConfigMenu();
                        break;
                    case MainMenuOptions.InitializeData:
                        InitializeData();
                        break;
                    case MainMenuOptions.ShowAllData:
                        ShowAllData();
                        break;
                    case MainMenuOptions.ResetAllData:
                        ResetAllData();
                        break;
                    case MainMenuOptions.Exit:
                        Console.WriteLine("Exiting Program. Goodbye.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }

            } while (choice != MainMenuOptions.Exit);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- FATAL ERROR ---");
            Console.WriteLine($"An unhandled exception occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Calls the initialization routine in DalTest/Initialization.cs and handles errors.
    /// </summary>
    /// <remarks>
    /// This method delegates to the static Initialization.Do method which populates
    /// the DAL with sample couriers, orders and deliveries. Exceptions are caught and printed.
    /// </remarks>
    private static void InitializeData()
    {
        try
        {
            Initialization.Do(s_dalICourier, s_dalIOrder, s_dalIDelivery, s_dalIConfig);
            Console.WriteLine("Data initialization successful.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n--- ERROR during Initialization ---");
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Resets all data by deleting all entities and resetting configuration.
    /// </summary>
    /// <remarks>
    /// Deletes all deliveries, orders and couriers from DAL and resets configuration values.
    /// This is useful for starting a fresh test run. All operations are wrapped in try/catch.
    /// </remarks>
    private static void ResetAllData()
    {
        try
        {
            s_dalIDelivery.DeleteAll();
            s_dalIOrder.DeleteAll();
            s_dalICourier.DeleteAll();

            s_dalIConfig.Reset();

            Console.WriteLine("Database and Configuration successfully reset.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n--- ERROR during Reset ---");
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Displays all data entities by calling ReadAll on each interface and prints configuration.
    /// </summary>
    /// <remarks>
    /// Uses the DAL ReadAll() methods to enumerate existing entities and prints them using ToString().
    /// If the DAL implementation hasn't overridden ToString, you may see type names instead of content.
    /// </remarks>
    private static void ShowAllData()
    {
        try
        {
            Console.WriteLine("\n--- ALL COURIERS ---");
            s_dalICourier.ReadAll().ForEach(c => Console.WriteLine(c));

            Console.WriteLine("\n--- ALL ORDERS ---");
            s_dalIOrder.ReadAll().ForEach(o => Console.WriteLine(o));

            Console.WriteLine("\n--- ALL DELIVERIES ---");
            s_dalIDelivery.ReadAll().ForEach(d => Console.WriteLine(d));

            Console.WriteLine("\n--- CURRENT CONFIGURATION ---");
            Console.WriteLine($"Clock: {s_dalIConfig.Clock}");
            Console.WriteLine($"Admin ID: {s_dalIConfig.AdminId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n--- ERROR reading data ---");
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Displays the CRUD menu for the Courier entity and handles user choices.
    /// </summary>
    /// <param name="dal">DAL courier interface to operate on.</param>
    /// <remarks>
    /// Sub-menu supports Create, ReadSingle, ReadAll and DeleteAll options.
    /// The Create option uses InputCourier() to collect validated user input.
    /// All DAL calls are executed inside try/catch to surface DAL exceptions.
    /// </remarks>
    private static void ShowCourierMenu(ICourier dal)
    {
        CrudMenuOptions choice = CrudMenuOptions.Return;
        do
        {
            Console.WriteLine("\n--- Courier CRUD Menu ---");
            Console.WriteLine("0: Return");
            Console.WriteLine("1: Create (Manual Input)");
            Console.WriteLine("2: Read Single (by ID)");
            Console.WriteLine("3: Read All");
            Console.WriteLine("6: Delete All");

            Console.Write("\nEnter choice: ");
            if (!int.TryParse(Console.ReadLine(), out int numChoice)) continue;
            choice = (CrudMenuOptions)numChoice;

            try
            {
                switch (choice)
                {
                    case CrudMenuOptions.Create:
                        DO.Courier newCourier = InputCourier();
                        dal.Create(newCourier);
                        Console.WriteLine($"Courier created successfully with ID: {newCourier.Id}");
                        break;
                    case CrudMenuOptions.ReadSingle:
                        int id = ReadInt("Enter Courier ID (T\"Z): ");
                        var courier = dal.Read(id);
                        Console.WriteLine(courier != null ? courier.ToString() : "Courier not found.");
                        break;
                    case CrudMenuOptions.ReadAll:
                        dal.ReadAll().ForEach(c => Console.WriteLine(c));
                        break;
                    case CrudMenuOptions.DeleteAll:
                        dal.DeleteAll();
                        Console.WriteLine("All couriers deleted.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n--- COURIER ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != CrudMenuOptions.Return);
    }

    /// <summary>
    /// Displays the CRUD menu for the Order entity and handles user choices.
    /// </summary>
    /// <param name="dal">DAL order interface to operate on.</param>
    /// <remarks>
    /// Create/Update operations are omitted because interactive entry for all Order fields
    /// would be verbose; read/delete operations are available for test verification.
    /// </remarks>
    private static void ShowOrderMenu(IOrder dal)
    {
        CrudMenuOptions choice = CrudMenuOptions.Return;
        do
        {
            Console.WriteLine("\n--- Order CRUD Menu ---");
            Console.WriteLine("0: Return | 2: Read Single (by ID) | 3: Read All | 6: Delete All");
            Console.WriteLine("(Create and Update skipped due to complexity in CLI)");
            Console.Write("Enter choice: ");
            if (!int.TryParse(Console.ReadLine(), out int numChoice)) continue;
            choice = (CrudMenuOptions)numChoice;

            try
            {
                switch (choice)
                {
                    case CrudMenuOptions.ReadSingle:
                        int id = ReadInt("Enter Order ID: ");
                        var order = dal.Read(id);
                        Console.WriteLine(order != null ? order.ToString() : "Order not found.");
                        break;
                    case CrudMenuOptions.ReadAll:
                        dal.ReadAll().ForEach(o => Console.WriteLine(o));
                        break;
                    case CrudMenuOptions.DeleteAll:
                        dal.DeleteAll();
                        Console.WriteLine("All orders deleted.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n--- ORDER ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != CrudMenuOptions.Return);
    }

    /// <summary>
    /// Displays the CRUD menu for the Delivery entity and handles user choices.
    /// </summary>
    /// <param name="dal">DAL delivery interface to operate on.</param>
    /// <remarks>
    /// Delivery creation and update are omitted in this stage. Read and delete operations
    /// are provided for testing the DAL's stored delivery records.
    /// </remarks>
    private static void ShowDeliveryMenu(IDelivery dal)
    {
        CrudMenuOptions choice = CrudMenuOptions.Return;
        do
        {
            Console.WriteLine("\n--- Delivery CRUD Menu ---");
            Console.WriteLine("0: Return | 2: Read Single (by ID) | 3: Read All | 6: Delete All");
            Console.WriteLine("(Create and Update skipped due to complexity in CLI)");
            Console.Write("Enter choice: ");
            if (!int.TryParse(Console.ReadLine(), out int numChoice)) continue;
            choice = (CrudMenuOptions)numChoice;

            try
            {
                switch (choice)
                {
                    case CrudMenuOptions.ReadSingle:
                        int id = ReadInt("Enter Delivery ID: ");
                        var delivery = dal.Read(id);
                        Console.WriteLine(delivery != null ? delivery.ToString() : "Delivery not found.");
                        break;
                    case CrudMenuOptions.ReadAll:
                        dal.ReadAll().ForEach(d => Console.WriteLine(d));
                        break;
                    case CrudMenuOptions.DeleteAll:
                        dal.DeleteAll();
                        Console.WriteLine("All deliveries deleted.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n--- DELIVERY ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != CrudMenuOptions.Return);
    }

    /// <summary>
    /// Displays the Configuration menu for managing the system clock and variables.
    /// </summary>
    /// <remarks>
    /// Option 2 advances the system clock by a double number of hours (can be fractional).
    /// The clock is used by initialization to compute realistic timestamps for generated data.
    /// </remarks>
    private static void ShowConfigMenu()
    {
        int choice = 0;     
        do
        {
            Console.WriteLine("\n--- Configuration Menu ---");
            Console.WriteLine("0: Return");
            Console.WriteLine($"1: Show Current Clock ({s_dalIConfig.Clock})");
            Console.WriteLine("2: Advance Clock (Manual Input)");
            Console.WriteLine($"3: Show Max Range ({s_dalIConfig.MaxRange})");
            Console.Write("\nEnter choice: ");
            if (!int.TryParse(Console.ReadLine(), out choice)) continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        Console.WriteLine($"Current System Clock: {s_dalIConfig.Clock}");
                        break;
                    case 2:
                        double hours = ReadDouble("Enter hours to advance: ");
                        s_dalIConfig.Clock = s_dalIConfig.Clock.AddHours(hours);
                        Console.WriteLine($"Clock advanced. New time: {s_dalIConfig.Clock}");
                        break;
                    case 3:
                        Console.WriteLine($"Current Max Range: {s_dalIConfig.MaxRange}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n--- CONFIG ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != 0);
    }

    /// <summary>
    /// Interactively prompts the user for all details needed to create a new Courier.
    /// </summary>
    /// <returns>A new <see cref="DO.Courier"/> instance populated from user input.</returns>
    /// <remarks>
    /// This helper validates user input using TryParse loops and returns a populated Courier
    /// using object-initializer syntax so it matches the DO record definition. The routine
    /// intentionally uses the system clock for the courier's start date display only.
    /// </remarks>
    private static DO.Courier InputCourier()
    {
        Console.WriteLine("\n--- Create New Courier ---");

        int id = ReadInt("Enter ID (T\"Z): ");
        string name = ReadString("Enter Name: ");
        string phone = ReadString("Enter Phone (e.g., 0501234567): ");
        string email = ReadString("Enter Email: ");

        DO.VehicleType vehicle = ReadEnum<DO.VehicleType>("Enter Vehicle Type (Car=0, Motorcycle=1, Bicycle=2, Foot=3): ");

        double? maxDistance = ReadDoubleNull("Enter Max Distance (in km, leave blank for null): ");

        bool isActive = ReadBool("Is courier active? (yes/no): ");

        DateTime startDate = s_dalIConfig.Clock;
        Console.WriteLine($"Setting Start Date to current system clock: {startDate}");

        return new Courier()
        {
            Id = id,
            Name = name,
            PhoneNumber = phone,
            Email = email,
            IsActive = isActive,
            Distance = maxDistance,
            VehicleType = vehicle
        };
    }

    /// <summary>
    /// Reads a non-empty string from the console.
    /// </summary>
    /// <param name="prompt">Prompt text to display to the user.</param>
    /// <returns>User input string (never null or whitespace).</returns>
    /// <remarks>
    /// This method keeps prompting until the user submits a non-empty value.
    /// Use for required text fields such as names and emails.
    /// </remarks>
    private static string ReadString(string prompt)
    {
        string? input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
        } while (string.IsNullOrWhiteSpace(input));
        return input!;
    }

    /// <summary>
    /// Reads a valid integer from the console using a TryParse loop.
    /// </summary>
    /// <param name="prompt">Prompt text to display to the user.</param>
    /// <returns>Parsed integer value.</returns>
    /// <remarks>
    /// Continues prompting until TryParse succeeds. Suitable for numeric IDs and menu selection.
    /// </remarks>
    private static int ReadInt(string prompt)
    {
        int result;
        while (!int.TryParse(ReadString(prompt), out result))
        {
            Console.WriteLine("Invalid input. Please enter a valid integer.");
        }
        return result;
    }

    /// <summary>
    /// Reads a valid double from the console using a TryParse loop.
    /// </summary>
    /// <param name="prompt">Prompt text to display to the user.</param>
    /// <returns>Parsed double value.</returns>
    /// <remarks>
    /// Accepts decimal numbers; useful for fractional hours or distances.
    /// </remarks>
    private static double ReadDouble(string prompt)
    {
        double result;
        while (!double.TryParse(ReadString(prompt), out result))
        {
            Console.WriteLine("Invalid input. Please enter a valid number.");
        }
        return result;
    }

    /// <summary>
    /// Reads a valid nullable double from the console. Returns null for empty input.
    /// </summary>
    /// <param name="prompt">Prompt text to display to the user.</param>
    /// <returns>Parsed double value or null when the user provides empty input.</returns>
    /// <remarks>
    /// Useful for optional numeric fields such as MaxDistance where the user may leave blank.
    /// </remarks>
    private static double? ReadDoubleNull(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        if (double.TryParse(input, out double result))
        {
            return result;
        }
        Console.WriteLine("Invalid input. Setting to null.");
        return null;
    }

    /// <summary>
    /// Reads a boolean value from the console (y/yes/true accepted).
    /// </summary>
    /// <param name="prompt">Prompt text to display to the user.</param>
    /// <returns>Parsed boolean value (true for y/yes/true).</returns>
    /// <remarks>
    /// Use for toggles such as IsActive. Input is case-insensitive.
    /// </remarks>
    private static bool ReadBool(string prompt)
    {
        string input = ReadString(prompt).ToLower();
        return input == "y" || input == "yes" || input == "true";
    }

    /// <summary>
    /// Reads a valid Enum value from the console using TryParse and validation.
    /// </summary>
    /// <typeparam name="T">Enum type to parse.</typeparam>
    /// <param name="prompt">Prompt text to display to the user.</param>
    /// <returns>Parsed enum value of type T.</returns>
    /// <remarks>
    /// The method loops until the user enters a valid enum name or numeric value acceptable by Enum.TryParse.
    /// It also checks Enum.IsDefined to avoid out-of-range numeric values.
    /// </remarks>
    private static T ReadEnum<T>(string prompt) where T : struct, Enum
    {
        T result;
        string input = ReadString(prompt);
        while (!Enum.TryParse(input, true, out result) || !Enum.IsDefined(typeof(T), result))
        {
            Console.WriteLine("Invalid type. Please enter one of the listed options.");
            input = ReadString(prompt);
        }
        return result;
    }
}


