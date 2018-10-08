using System;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Asv.Mavlink.Shell
{
    public static class ConsoleDialogs
    {
        public static string AskString(string question)
        {
            Console.Write(question + @" : ");
            return Console.ReadLine();
        }

        public static bool AskYesNo(string question)
        {
            bool? res = null;
            while (!res.HasValue)
            {
                Console.Write(question);
                var result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.Y:
                        Console.WriteLine(" => YES");
                        res = true;
                        break;
                    case ConsoleKey.N:
                        Console.WriteLine(" => NO");
                        res = false;
                        break;
                    default:
                        Console.WriteLine(RS.DialogsApi_ask_yes_no);
                        break;
                }
                
            }
            
            return res.Value;
        }

        public static DateTime AskDateTime(string question)
        {
            while (true)
            {
                DateTime resDigit;
                Console.Write(question + ": ");
                var result = Console.ReadLine();
                if (result != null && DateTime.TryParse(result, out resDigit))
                {
                    return resDigit;
                }
                Console.WriteLine(RS.DialogsApi_ask_date_time_Please_input_date_time_value);
            }
        }

        public static string AskPassword(string question)
        {
            while (true)
            {
                Console.Write(question + ": ");

                var sb = new StringBuilder();
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return sb.ToString();
                    }
                    sb.Append(key.KeyChar);
                    Console.Write('*');
                }
            }

        }
        
        public static long AskInteger(string question)
        {
            while (true)
            {
                long resDigit;
                Console.Write(question + ": ");
                var result = Console.ReadLine();
                if (result != null && long.TryParse(result, out resDigit))
                {
                    return resDigit;
                }
                Console.WriteLine(RS.DialogsApi_ask_integer_Please_input_integer_value);
            }
        }
        
        public static double AskDouble(string question)
        {
            while (true)
            {
                Console.Write(question + ": ");
                var result = Console.ReadLine();
                double resDigit;
                if (result != null && double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out resDigit))
                {
                    return resDigit;
                }
                Console.WriteLine(RS.DialogsApi_ask_double_Please_input_double_value);
            }
        }




    }
}