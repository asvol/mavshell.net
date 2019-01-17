using System;
using System.Collections.Generic;

namespace Asv.Mavlink.Shell
{
    public class PrintPx4VehicleState : PrintVehicleState
    {
        public PrintPx4VehicleState()
        {
            IsCommand("px4info", "Print PX4 vehicle info");
        }

        protected override IVehicle CreateVehicle(VehicleConfig config)
        {
            return new VehiclePx4(config);
        }

        protected override void GetAddidtionslParams(IVehicle vehicle, IDictionary<string, string> paramsToPrint)
        {
            paramsToPrint.Add("PX4CustomMode", ((IVehiclePx4)vehicle).Mode.Value?.Px4Mode.ToString() ?? String.Empty);
        }
    }
}