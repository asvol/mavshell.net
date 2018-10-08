using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink.V2.Common;

namespace Asv.Mavlink.Shell
{
    public class SmoothFlightCommand:VehicleCommandBase
    {
        public const int ToleranceMeter = 1;

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

            try
            {
                while (ConsoleDialogs.AskYesNo("Need arm?"))
                {
                    var result = await Vehicle.Arm(CancellationToken.None).ConfigureAwait(false);
                    PrintResult(result);
                    if (result.Result == MavResult.MavResultAccepted) break;
                }
                while (ConsoleDialogs.AskYesNo("Take off needed?"))
                {
                    var meters = ConsoleDialogs.AskDouble("How high (in meters) ?");
                    var result = await Vehicle.TakeOff(float.NaN, -1, (float) Vehicle.Home.Value.Latitude,
                        (float) Vehicle.Home.Value.Longitude, (float) (Vehicle.Home.Value.Altitude.Value + meters),
                        CancellationToken.None).ConfigureAwait(false);
                    PrintResult(result);
                    if (result.Result == MavResult.MavResultAccepted) break;
                }
                while (true)
                {
                    try
                    {
                        var azimuth = ConsoleDialogs.AskDouble("Azimuth (in deg) ?");
                        var dist = ConsoleDialogs.AskDouble("Distance (in meter) ?");
                        var alt = ConsoleDialogs.AskDouble("Altitude (in meter, by home)?");
                        var target = GeoMath.RadialPoint(Vehicle.Gps.Value, dist, azimuth);
                        target = new GeoPoint(target.Latitude,target.Longitude, Vehicle.Home.Value.Altitude.Value + alt);
                        await vehicle.GoToSmooth(target,20,1,1000,CancellationToken.None, new Progress<double>(_=>Console.WriteLine(TextRender.Progress(_,30))));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }

        

        private void PrintResult(CommandAckPayload result)
        {
            Console.WriteLine($"{result.Command:G}:{result.Result:G} (param2:{result.ResultParam2};progress:{result.Progress})");
        }
    }
}