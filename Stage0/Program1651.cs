partial class Program
{
    private static void Main(string[] args)
    {
        Welcome1651();
        Welcome7440();
        System.Console.ReadKey();
    }

    static partial void Welcome7440();
    private static void Welcome1651()
    {
        System.Console.Write("Enter your name: ");
        string name = Console.ReadLine();
        System.Console.WriteLine("{0}, welcome to my first console application", name);
    }
}