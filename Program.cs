Console.WriteLine("Enter password:");
var password = Console.ReadLine()?.ToString() ?? "0";
Console.WriteLine("Password to guess is " + password);

var checker = new Check();
var result = checker.RunChecks(password).Result;

if(result == string.Empty)
{
    Console.WriteLine("");
    Console.WriteLine($"Password not found!");
}
