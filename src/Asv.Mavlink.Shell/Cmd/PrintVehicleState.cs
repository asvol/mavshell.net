using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink.V2.Common;

namespace Asv.Mavlink.Shell
{
    public class PrintVehicleState:VehicleCommandBase
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        public PrintVehicleState()
        {
            IsCommand("info", "Print vehicle info");
        }

        public override int Run(string[] remainingArguments)
        {
            Task.Factory.StartNew(KeyListen, TaskCreationOptions.LongRunning, _cancel.Token);
            return base.Run(remainingArguments);
        }

        protected override async Task<int> RunAsync(IVehicle vehicle)
        {
            while (!_cancel.IsCancellationRequested)
            {
                Print(vehicle);
                await Task.Delay(1000, _cancel.Token).ConfigureAwait(false);
            }
            return 0;
        }

        protected override IVehicle CreateVehicle(VehicleConfig config)
        {
            return new Vehicle(config);
        }

        protected virtual void GetAddidtionslParams(IVehicle vehicle, IDictionary<string, string> paramsToPrint)
        {
            
        }

        private void Print(IVehicle vehicle)
        {
            Console.Clear();

            var dict = new Dictionary<string,string>
            {
                {nameof(Vehicle.Link),vehicle.Link.Value.ToString() },
                {nameof(Vehicle.PacketRateHz),vehicle.PacketRateHz.Value.ToString("0 Hz") },
                {nameof(HeartbeatPayload.SystemStatus),vehicle.Heartbeat.Value?.SystemStatus.ToString() ?? string.Empty},
                {nameof(HeartbeatPayload.Type),vehicle.Heartbeat.Value?.Type.ToString()?? string.Empty },
                {nameof(HeartbeatPayload.Autopilot),vehicle.Heartbeat.Value?.Autopilot.ToString()?? string.Empty},
                {nameof(HeartbeatPayload.BaseMode),vehicle.Heartbeat.Value?.BaseMode.ToString("F")?? string.Empty},
                {nameof(HeartbeatPayload.CustomMode),vehicle.Heartbeat.Value?.CustomMode.ToString() ?? string.Empty},
                {nameof(HeartbeatPayload.MavlinkVersion),vehicle.Heartbeat.Value?.MavlinkVersion.ToString() ?? string.Empty},
            };
            GetAddidtionslParams(vehicle, dict);
            TextTable.PrintKeyValue(Console.WriteLine, new DoubleTextTableBorder(), 20,20, "Vehicle", dict);
        }

        private void KeyListen(object a)
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        _cancel.Cancel(false);
                        break;
                }
            }
        }
    }
}