using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            const int percentWidth = 30;
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
                {nameof(SysStatusPayload.BatteryRemaining),vehicle.SysStatus.Value?.BatteryRemaining.ToString() ?? string.Empty},
                {nameof(SysStatusPayload.CurrentBattery),vehicle.SysStatus.Value?.CurrentBattery.ToString() ?? string.Empty},
                {nameof(SysStatusPayload.DropRateComm), TextRender.Progress(((vehicle.SysStatus.Value?.DropRateComm) ?? 0) / 10000.0, percentWidth )},
                {nameof(SysStatusPayload.ErrorsComm),vehicle.SysStatus.Value?.ErrorsComm.ToString() ?? string.Empty},
                {nameof(SysStatusPayload.Load),TextRender.Progress((vehicle.SysStatus.Value?.Load ?? 0) / 1000.0, percentWidth )},
                {nameof(SysStatusPayload.VoltageBattery),vehicle.SysStatus.Value?.VoltageBattery.ToString() ?? string.Empty},
                {nameof(GpsRawIntPayload.Alt),((vehicle.GpsRawInt.Value?.Alt ?? double.NaN) / 1000.0).ToString("F1")},
                
            };
            GetAddidtionslParams(vehicle, dict);
            TextTable.PrintKeyValue(Console.WriteLine, new DoubleTextTableBorder(), dict.Max(_=>_.Key.Length),dict.Max(_=>_.Value.Length), "Vehicle", dict);
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
                    case ConsoleKey.T:
                        Vehicle.TakeOff(0,float.NaN, 55.146524f, 61.406014f, Vehicle.GpsRawInt.Value.Alt + 50f, CancellationToken.None).Wait();
                        break;
                    case ConsoleKey.A:
                        Vehicle.Arm(CancellationToken.None).Wait();
                        break;
                    case ConsoleKey.D:
                        Vehicle.Disarm(CancellationToken.None).Wait();
                        break;
                }
            }
        }
    }
}