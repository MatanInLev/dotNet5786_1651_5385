using BlApi;
using BO;
using System;
using System.Collections.Generic;

namespace BLTest;

internal static class Program
{
    // Access the BL layer via the Factory (Simple Factory Pattern)
    static readonly IBl s_bl = BlApi.Factory.Get();

    // Simulated logged-in user ID (Admin) for testing purposes
    private const int c_adminId = 12345678;

    static void Main(string[] args)
    {
        Console.WriteLine("=== BL Test Console Application ===");

        int choice;
        do
        {
            // Main Menu
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("0: Exit");
            Console.WriteLine("1: Courier Management");
            Console.WriteLine("2: Order Management");
            Console.WriteLine("3: Admin / System Config");

            choice = ReadInt("Enter your choice: ");

            try
            {
                switch (choice)
                {
                    case 0:
                        Console.WriteLine("Goodbye!");
                        break;
                    case 1:
                        CourierMenu();
                        break;
                    case 2:
                        OrderMenu();
                        break;
                    case 3:
                        AdminMenu();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Global exception handler for unexpected crashes
                PrintException(ex);
            }

        } while (choice != 0);
    }

    #region Sub-Menus

    /// <summary>
    /// Sub-menu for Courier entity operations (CRUD + List)
    /// </summary>
    private static void CourierMenu()
    {
        int choice;
        do
        {
            Console.WriteLine("\n--- Courier Management ---");
            Console.WriteLine("0: Return to Main Menu");
            Console.WriteLine("1: Add Courier");
            Console.WriteLine("2: Get Courier by ID");
            Console.WriteLine("3: Get All Couriers");
            Console.WriteLine("4: Update Courier");
            Console.WriteLine("5: Delete Courier");

            choice = ReadInt("Enter your choice: ");

            try
            {
                switch (choice)
                {
                    case 0: break;

                    case 1: // Add
                        Console.WriteLine("Enter details for new Courier:");
                        BO.Courier newCourier = new BO.Courier
                        {
                            Id = ReadInt("ID: "),
                            Name = ReadString("Name: "),
                            Email = ReadString("Email: "),
                            Phone = ReadString("Phone: "),
                            Vehicle = ReadEnum<BO.Vehicle>("Vehicle Type (0-Bike, 1-Car...): "),
                            MaxDistance = ReadDouble("Max Distance (km): "),
                            IsActive = true
                        };
                        s_bl.Courier.Add(c_adminId, newCourier);
                        Console.WriteLine("Courier added successfully.");
                        break;

                    case 2: // Get Single
                        int getId = ReadInt("Enter Courier ID: ");
                        Console.WriteLine(s_bl.Courier.Get(c_adminId, getId));
                        break;

                    case 3: // Get List
                        Console.WriteLine("List of Couriers:");
                        foreach (var item in s_bl.Courier.GetList(c_adminId))
                        {
                            Console.WriteLine(item);
                        }
                        break;

                    case 4: // Update
                        int upId = ReadInt("Enter Courier ID to update: ");
                        // Fetch existing data first to allow partial updates
                        BO.Courier upCourier = s_bl.Courier.Get(c_adminId, upId);
                        Console.WriteLine($"Updating Courier {upId}. Leave fields empty to keep current value.");

                        string name = ReadString($"Name [{upCourier.Name}]: ", allowEmpty: true);
                        if (!string.IsNullOrEmpty(name)) upCourier.Name = name;

                        string phone = ReadString($"Phone [{upCourier.Phone}]: ", allowEmpty: true);
                        if (!string.IsNullOrEmpty(phone)) upCourier.Phone = phone;

                        // Pass the updated object back to BL
                        s_bl.Courier.Update(c_adminId, upCourier);
                        Console.WriteLine("Courier updated successfully.");
                        break;

                    case 5: // Delete
                        int delId = ReadInt("Enter Courier ID to delete: ");
                        s_bl.Courier.Delete(c_adminId, delId);
                        Console.WriteLine("Courier deleted successfully.");
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }

        } while (choice != 0);
    }

    /// <summary>
    /// Sub-menu for Order entity operations and Business Processes
    /// </summary>
    private static void OrderMenu()
    {
        int choice;
        do
        {
            Console.WriteLine("\n--- Order Management ---");
            Console.WriteLine("0: Return to Main Menu");
            Console.WriteLine("1: Add Order");
            Console.WriteLine("2: Get Order by ID");
            Console.WriteLine("3: Get All Orders");
            Console.WriteLine("4: Update Order");
            Console.WriteLine("5: Delete Order (Note: Should throw exception)");
            Console.WriteLine("--- Business Actions ---");
            Console.WriteLine("6: Assign Order to Courier");
            Console.WriteLine("7: Complete Order Delivery");
            Console.WriteLine("8: Cancel Order");
            Console.WriteLine("9: Show Orders Status Count");

            choice = ReadInt("Enter your choice: ");

            try
            {
                switch (choice)
                {
                    case 0: break;

                    case 1: // Add
                        BO.Order newOrder = new BO.Order
                        {
                            Id = 0, // Generated by DAL
                            CustomerName = ReadString("Customer Name: "),
                            CustomerAddress = ReadString("Address: "),
                            CustomerPhone = ReadString("Phone: "),
                            Type = ReadEnum<BO.OrderType>("Order Type (0-Food, 1-Electronics...): ")
                        };
                        s_bl.Order.Add(c_adminId, newOrder);
                        Console.WriteLine("Order added successfully.");
                        break;

                    case 2: // Get
                        int id = ReadInt("Enter Order ID: ");
                        Console.WriteLine(s_bl.Order.Get(c_adminId, id));
                        break;

                    case 3: // Get List
                        foreach (var item in s_bl.Order.GetList(c_adminId, null, null, null))
                            Console.WriteLine(item);
                        break;

                    case 4: // Update
                        int upId = ReadInt("Enter Order ID to update: ");
                        BO.Order upOrder = s_bl.Order.Get(c_adminId, upId);
                        Console.WriteLine($"Updating Order {upId}.");

                        string name = ReadString($"Customer Name [{upOrder.CustomerName}]: ", true);
                        if (!string.IsNullOrEmpty(name)) upOrder.CustomerName = name;

                        string addr = ReadString($"Address [{upOrder.CustomerAddress}]: ", true);
                        if (!string.IsNullOrEmpty(addr)) upOrder.CustomerAddress = addr;

                        s_bl.Order.Update(c_adminId, upOrder);
                        Console.WriteLine("Order updated successfully.");
                        break;

                    case 5: // Delete
                        int delId = ReadInt("Enter Order ID to delete: ");
                        s_bl.Order.Delete(c_adminId, delId);
                        break;

                    case 6: // Assign (Courier picks up order)
                        int ordId = ReadInt("Enter Order ID: ");
                        int courId = ReadInt("Enter Courier ID: ");
                        s_bl.Order.AssignOrder(c_adminId, ordId, courId);
                        Console.WriteLine("Order assigned to courier successfully.");
                        break;

                    case 7: // Complete Delivery
                        int delivId = ReadInt("Enter Delivery ID (found inside Order details): ");
                        BO.DeliveryStatus status = ReadEnum<BO.DeliveryStatus>("Status (0-Delivered...): ");
                        s_bl.Order.CompleteOrderDelivery(c_adminId, delivId, status);
                        Console.WriteLine("Order delivery completed.");
                        break;

                    case 8: // Cancel
                        int cancelId = ReadInt("Enter Order ID to cancel: ");
                        s_bl.Order.Cancel(c_adminId, cancelId);
                        Console.WriteLine("Order cancelled successfully.");
                        break;

                    case 9: // Status Statistics
                        var stats = s_bl.Order.GetOrdersStatusCount(c_adminId);
                        Console.WriteLine("Orders Status Count:");
                        foreach (var kvp in stats)
                        {
                            Console.WriteLine($" - {kvp.Key}: {kvp.Value}");
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }

        } while (choice != 0);
    }

    /// <summary>
    /// Sub-menu for Admin and System Configuration
    /// </summary>
    private static void AdminMenu()
    {
        int choice;
        do
        {
            Console.WriteLine("\n--- Admin & Config ---");
            Console.WriteLine("0: Return to Main Menu");
            Console.WriteLine("1: Initialize Database (Test Data)");
            Console.WriteLine("2: Reset Database (Clear All)");
            Console.WriteLine("3: Show System Clock");
            Console.WriteLine("4: Forward Clock");

            choice = ReadInt("Enter choice: ");
            try
            {
                switch (choice)
                {
                    case 0: break;

                    case 1: // Initialize
                        Console.Write("Initializing DB... ");
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine("Done.");
                        break;

                    case 2: // Reset
                        Console.Write("Resetting DB... ");
                        s_bl.Admin.ResetDB();
                        Console.WriteLine("Done.");
                        break;

                    case 3: // Clock
                        Console.WriteLine($"Current System Clock: {s_bl.Admin.GetClock()}");
                        break;

                    case 4: // Forward Clock
                        // Simulating time passage
                        int amount = ReadInt("Enter amount to forward: ");
                        BO.TimeUnit unit = ReadEnum<BO.TimeUnit>("Unit (0-Minutes, 1-Hours, 2-Days 3-Months 4-Years): ");

                        // Loop to simulate stepping if needed, or just call logic
                        // Since IAdmin interface accepts unit, we loop 'amount' times
                        for (int i = 0; i < amount; i++)
                        {
                            s_bl.Admin.ForwardClock(unit);
                        }
                        Console.WriteLine($"Clock forwarded. New time: {s_bl.Admin.GetClock()}");
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }
        } while (choice != 0);
    }

    #endregion

    #region Input Helpers

    // Helper to print exceptions with inner details
    static void PrintException(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {ex.GetType().Name}");
        Console.WriteLine($"Message: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
        Console.ResetColor();
    }

    // Helper to read an integer with validation
    static int ReadInt(string prompt)
    {
        Console.Write(prompt);
        int val;
        while (!int.TryParse(Console.ReadLine(), out val))
        {
            Console.WriteLine("Invalid input. Please enter a number.");
            Console.Write(prompt);
        }
        return val;
    }

    // Helper to read a double with validation
    static double ReadDouble(string prompt)
    {
        Console.Write(prompt);
        double val;
        while (!double.TryParse(Console.ReadLine(), out val))
        {
            Console.WriteLine("Invalid input. Please enter a number.");
            Console.Write(prompt);
        }
        return val;
    }

    // Helper to read a string, optionally allowing empty input
    static string ReadString(string prompt, bool allowEmpty = false)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        while (!allowEmpty && string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Input cannot be empty.");
            Console.Write(prompt);
            input = Console.ReadLine();
        }
        return input ?? "";
    }

    // Generic helper to read Enums
    static T ReadEnum<T>(string prompt) where T : struct, Enum
    {
        Console.Write(prompt);
        T result;
        while (!Enum.TryParse(Console.ReadLine(), out result) || !Enum.IsDefined(typeof(T), result))
        {
            Console.WriteLine("Invalid value. Please try again.");
            Console.Write(prompt);
        }
        return result;
    }

    #endregion
}