using System;
class Logger
{
    //static string path=@"";
    public static void Log(string s)
    {
        Console.WriteLine(DateTime.Now.ToString()+" : "+s);
    }
}