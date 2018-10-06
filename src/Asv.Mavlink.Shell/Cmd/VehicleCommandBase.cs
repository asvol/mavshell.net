using System.Threading;
using System.Threading.Tasks;
using ManyConsole;

namespace Asv.Mavlink.Shell
{
    public abstract class VehicleCommandBase : ConsoleCommand
    {
        private string _connectionString = "udp://192.168.0.140:14560";
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        protected VehicleCommandBase()
        {
            HasOption("cs=", $"Connection string. Default '{_connectionString}'", _ => _connectionString = _);
        }

        public override int Run(string[] remainingArguments)
        {
            var cfg = new VehicleConfig {ConnectionString = _connectionString};
            Vehicle = CreateVehicle(cfg);
            return RunAsync(Vehicle).Result;
        }

        public IVehicle Vehicle { get; private set; }

        protected abstract IVehicle CreateVehicle(VehicleConfig config);

        protected abstract Task<int> RunAsync(IVehicle vehicle);
        
    }
}