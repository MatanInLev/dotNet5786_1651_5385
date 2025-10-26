partial class Program
{
    private static void Main(string[] args)
    {
        Welcome1651();
        Welcome5385();
        System.Console.ReadKey();
    }

    static partial void Welcome5385();
    private static void Welcome1651()
    {
        System.Console.Write("Enter your name: ");
        string name = Console.ReadLine();
        System.Console.WriteLine("{0}, welcome to my first console application", name);
    }
}