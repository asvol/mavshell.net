using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink.V2.Common;

namespace Asv.Mavlink.Shell
{
    public class SmoothFlightCommand:VehicleCommandBase
    {
        public SmoothFlightCommand()
        {
            IsCommand("smooth_flight");
        }

        protected override IVehicle CreateVehicle(VehicleConfig config)
        {
            return new VehiclePx4(config);
        }

        protected override async Task<int> RunAsync(IVehicle vehicle)
        {
            Vehicle.RawStatusText.Subscribe(_ => Console.WriteLine($"DRONE=>{_.Severity:G}:{new string(_.Text)}"));
            while (ConsoleDialogs.AskYesNo("Need arm?"))
            {
                var result = await Vehicle.Arm(CancellationToken.None).ConfigureAwait(false);
                PrintResult(result);
                if (result.Result == MavResult.MavResultAccepted) break;
            }
            while (ConsoleDialogs.AskYesNo("Take off needed?"))
            {
                var meters = ConsoleDialogs.AskDouble("How high (in meters) ?");
                var result = await Vehicle.TakeOff(float.NaN,-1,(float) Vehicle.Home.Value.Latitude,(float) Vehicle.Home.Value.Longitude,(float) (Vehicle.Home.Value.Altitude.Value + meters), CancellationToken.None).ConfigureAwait(false);
                PrintResult(result);
                if (result.Result == MavResult.MavResultAccepted) break;
                
            }
            return 0;
        }

        private void PrintResult(CommandAckPayload result)
        {
            Console.WriteLine($"{result.Command:G}:{result.Result:G} (param2:{result.ResultParam2};progress:{result.Progress})");
        }
    }
}