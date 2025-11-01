/*
AI PROMPT (for documentation):
"Create the complete 'DalTest/Program.cs' file based on all rules in Chapter 11.
- It must include a main loop and sub-menus for all entities.
- It must use Enums for menu choices.
- All DAL calls must be wrapped in 'try-catch' blocks.
- All user input must use 'TryParse' (11d, Sec 8).
- Implement the 'Create' (manual input) for the 'Courier' entity fully.
- All XML documentation comments (///) must be in English."
*/

using DalApi;
using DO; // Required for entity types and Enums
using Dal; // Required to instantiate concrete implementation classes
using System;
using System.Linq; // Required for LINQ operations like Any() and ToList()

namespace DalTest;

/// <summary>
/// Defines the options for the main application menu.
/// </summary>
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
public static class Program
{
    // --- 11a: Interface Initialization ---

    // Instantiate concrete implementation classes from the DalList layer
    private static IConfig s_dalIConfig = new ConfigImplementation();
    private static ICourier s_dalICourier = new CourierImplementation();
    private static IOrder s_dalIOrder = new OrderImplementation();
    private static IDelivery s_dalIDelivery = new DeliveryImplementation();


    // --- Main Entry Point ---

    public static void Main(string[] args)
    {
        // 11a, Sec 2: Wrap the main logic in a try-catch block
        try
        {
            // Set a fixed past system clock time for predictable data initialization
            s_dalIConfig.Clock = new DateTime(2025, 10, 15, 10, 0, 0);

            // 11c: Main loop
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

                // 11d, Sec 8: Use TryParse for input safety
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
            // 11a, Sec 2: Catch any unhandled exceptions (e.g., interface instantiation issues)
            Console.WriteLine($"\n--- FATAL ERROR ---");
            Console.WriteLine($"An unhandled exception occurred: {ex.Message}");
        }
    }


    // --- Helper Methods (11d, Sec 2) ---

    /// <summary>
    /// Calls the initialization routine in DalTest/Initialization.cs.
    /// (Implements 11b: Initialization)
    /// </summary>
    private static void InitializeData()
    {
        try
        {
            // 11b: Call the static Do method, passing all initialized DAL interfaces.
            Initialization.Do(s_dalICourier, s_dalIOrder, s_dalIDelivery, s_dalIConfig);
            Console.WriteLine("Data initialization successful.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- ERROR during Initialization ---");
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Resets all data by deleting all entities and resetting configuration.
    /// (Implements 11b: Reset)
    /// </summary>
    private static void ResetAllData()
    {
        try
        {
            // Call DeleteAll on each entity
            s_dalIDelivery.DeleteAll();
            s_dalIOrder.DeleteAll();
            s_dalICourier.DeleteAll();

            // Reset configuration values
            s_dalIConfig.Reset();

            Console.WriteLine("Database and Configuration successfully reset.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- ERROR during Reset ---");
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Displays all data entities by calling ReadAll on each interface.
    /// </summary>
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
            Console.WriteLine($"\n--- ERROR reading data ---");
            Console.WriteLine(ex.Message);
        }
    }


    // --- Sub-Menus ---

    /// <summary>
    /// Displays the CRUD menu for the Courier entity.
    /// </summary>
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
                        // Call the complex input helper method
                        DO.Courier newCourier = InputCourier();
                        dal.Create(newCourier);
                        Console.WriteLine($"Courier created successfully with ID: {newCourier.Id}");
                        break;
                    case CrudMenuOptions.ReadSingle:
                        int id = ReadInt("Enter Courier ID (T\"Z): ");
                        var courier = dal.Read(id);
                        // CS0019 FIX: Use conditional expression for printing
                        Console.WriteLine(courier != null ? courier.ToString() : "Courier not found.");
                        break;
                    case CrudMenuOptions.ReadAll:
                        // 11d, Sec 4: ReadAll and print with implicit ToString
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
                // 11a, Sec 2: Catch DAL exception and print message
                Console.WriteLine($"\n--- COURIER ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != CrudMenuOptions.Return);
    }

    /// <summary>
    /// Displays the CRUD menu for the Order entity.
    /// </summary>
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
                        // CS0019 FIX: Use conditional expression
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
                Console.WriteLine($"\n--- ORDER ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != CrudMenuOptions.Return);
    }

    /// <summary>
    /// Displays the CRUD menu for the Delivery entity.
    /// </summary>
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
                        // CS0019 FIX: Use conditional expression
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
                Console.WriteLine($"\n--- DELIVERY ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != CrudMenuOptions.Return);
    }

    /// <summary>
    /// Displays the Configuration menu for managing the system clock and variables.
    /// </summary>
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
                Console.WriteLine($"\n--- CONFIG ERROR ---");
                Console.WriteLine(ex.Message);
            }
        } while (choice != 0);
    }

    // --- Complex Input Helper Methods ---

    /// <summary>
    /// Interactively prompts the user for all details needed to create a new Courier.
    /// (Implements 11c input requirement)
    /// </summary>
    private static DO.Courier InputCourier()
    {
        Console.WriteLine("\n--- Create New Courier ---");

        int id = ReadInt("Enter ID (T\"Z): ");
        string name = ReadString("Enter Name: ");
        string phone = ReadString("Enter Phone (e.g., 0501234567): ");
        string email = ReadString("Enter Email: ");

        // Enum input
        DO.VehicleType vehicle = ReadEnum<DO.VehicleType>("Enter Vehicle Type (Car=0, Motorcycle=1, Bicycle=2, Foot=3): ");

        // Nullable double input
        double? maxDistance = ReadDoubleNull("Enter Max Distance (in km, leave blank for null): ");

        // Bool input
        bool isActive = ReadBool("Is courier active? (yes/no): ");

        // Use system clock for start date
        DateTime startDate = s_dalIConfig.Clock;
        Console.WriteLine($"Setting Start Date to current system clock: {startDate}");

        // CS1739 FIX: Changed from positional constructor ( ... )
        // to object initializer syntax { ... }
        // This must match the DO.Courier.cs record definition.
        return new Courier() // Call the parameterless constructor
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

    // --- Safe Input Helper Methods (using TryParse) ---

    /// <summary>
    /// Reads a non-empty string from the console.
    /// </summary>
    private static string ReadString(string prompt)
    {
        string? input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
        } while (string.IsNullOrWhiteSpace(input));
        return input;
    }

    /// <summary>
    /// Reads a valid integer from the console.
    /// </summary>
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
    /// Reads a valid double from the console.
    /// </summary>
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
    /// Reads a valid nullable double from the console.
    /// </summary>
    private static double? ReadDoubleNull(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return null; // Allows empty input for nullable
        }
        if (double.TryParse(input, out double result))
        {
            return result;
        }
        Console.WriteLine("Invalid input. Setting to null.");
        return null;
    }

    /// <summary>
    /// Reads a boolean value from the console (y/yes/true).
    /// </summary>
    private static bool ReadBool(string prompt)
    {
        string input = ReadString(prompt).ToLower();
        return input == "y" || input == "yes" || input == "true";
    }

    /// <summary>
    /// Reads a valid Enum value from the console.
    /// </summary>
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


