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
    // --- 11a: Initialization of Interfaces ---

    // Instantiate concrete implementation classes. Assumes they exist in the 'Dal' namespace.
    private static IConfig s_dalIConfig = new ConfigImplementation();
    private static ICourier s_dalICourier = new CourierImplementation();
    private static IOrder s_dalIOrder = new OrderImplementation();
    private static IDelivery s_dalIDelivery = new DeliveryImplementation();


    // --- Main Entry Point ---

    public static void Main(string[] args)
    {
        // 11a, Sec 2: Wrap the main program logic in a try-catch block to handle exceptions
        try
        {
            // Set the Configuration clock to a specific past time for predictable data initialization
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
                Console.WriteLine("5: Initialize All Data");
                Console.WriteLine("6: Show All Entities");
                Console.WriteLine("7: Reset All Data");

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
            // 11a, Sec 2: Catch any unhandled exceptions (e.g., initialization issues)
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

    private static void ShowCourierMenu(ICourier dal)
    {
        CrudMenuOptions choice = CrudMenuOptions.Return;
        do
        {
            Console.WriteLine("\n--- Courier CRUD Menu ---");
            Console.WriteLine("0: Return");
            Console.WriteLine("1: Create (Requires complex manual input, skipping here)");
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
                    case CrudMenuOptions.ReadSingle:
                        Console.Write("Enter Courier ID (T\"Z): ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            var courier = dal.Read(id);
                            // CS0019 FIX: Use conditional expression to print the object or the string
                            Console.WriteLine(courier != null ? courier.ToString() : "Courier not found.");
                        }
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

    private static void ShowOrderMenu(IOrder dal)
    {
        CrudMenuOptions choice = CrudMenuOptions.Return;
        do
        {
            Console.WriteLine("\n--- Order CRUD Menu ---");
            Console.WriteLine("0: Return | 2: Read Single (by ID) | 3: Read All | 6: Delete All");
            Console.Write("Enter choice: ");
            if (!int.TryParse(Console.ReadLine(), out int numChoice)) continue;
            choice = (CrudMenuOptions)numChoice;

            try
            {
                switch (choice)
                {
                    case CrudMenuOptions.ReadSingle:
                        Console.Write("Enter Order ID: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            var order = dal.Read(id);
                            // CS0019 FIX: Use conditional expression to print the object or the string
                            Console.WriteLine(order != null ? order.ToString() : "Order not found.");
                        }
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

    private static void ShowDeliveryMenu(IDelivery dal)
    {
        CrudMenuOptions choice = CrudMenuOptions.Return;
        do
        {
            Console.WriteLine("\n--- Delivery CRUD Menu ---");
            Console.WriteLine("0: Return | 2: Read Single (by ID) | 3: Read All | 6: Delete All");
            Console.Write("Enter choice: ");
            if (!int.TryParse(Console.ReadLine(), out int numChoice)) continue;
            choice = (CrudMenuOptions)numChoice;

            try
            {
                switch (choice)
                {
                    case CrudMenuOptions.ReadSingle:
                        Console.Write("Enter Delivery ID: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            var delivery = dal.Read(id);
                            // CS0019 FIX: Use conditional expression to print the object or the string
                            Console.WriteLine(delivery != null ? delivery.ToString() : "Delivery not found.");
                        }
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
                        Console.Write("Enter hours to advance: ");
                        if (double.TryParse(Console.ReadLine(), out double hours))
                        {
                            s_dalIConfig.Clock = s_dalIConfig.Clock.AddHours(hours);
                            Console.WriteLine($"Clock advanced. New time: {s_dalIConfig.Clock}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid hour input.");
                        }
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
}
