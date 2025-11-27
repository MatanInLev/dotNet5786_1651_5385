using BlApi;
using BO;

namespace BLTest;

internal static class Program
{
    static readonly IBl s_bl = BlApi.Factory.Get();

    static void Main(string[] args)
    {
        int choice;
        do
        {
            Console.WriteLine("\n--- BL Test Menu ---");
            Console.WriteLine("0: Exit");
            Console.WriteLine("1: List all orders");
            Console.WriteLine("2: Read order by ID");
            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Invalid input.");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1:
                        foreach (var order in s_bl.Order.GetList(0, null, null, null))
                            Console.WriteLine(order);
                        break;

                    case 2:
                        int id = ReadInt("Enter order ID: ");
                        var result = s_bl.Order.Get(0, id);
                        Console.WriteLine(result);
                        break;

                    case 0:
                        Console.WriteLine("Goodbye!");
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (BlBaseException ex)
            {
                Console.WriteLine($"BL Error: {ex.Message}");
            }

        } while (choice != 0);
    }

    static int ReadInt(string prompt)
    {
        Console.Write(prompt);
        int value;
        while (!int.TryParse(Console.ReadLine(), out value))
        {
            Console.WriteLine("Invalid number. Try again.");
            Console.Write(prompt);
        }
        return value;
    }
}
