using ManyConsole;

namespace Asv.Mavlink.Shell
{
    public class SitlCommand:ConsoleCommand
    {
        private string _connectionString = "udp://192.168.0.140:14550";

        public SitlCommand()
        {
            IsCommand("sitl", "Emulate drone");
            HasOption("cs=", $"Connection string. Default '{_connectionString}'", _ => _connectionString = _);
        }

        public override int Run(string[] remainingArguments)
        {
            return 0;
        }
    }
}