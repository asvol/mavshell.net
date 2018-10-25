﻿using System;
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
        private int _deltaXY = 10;
        private int _deltaZ = 5;
        private string _lastCommand = string.Empty;

        public PrintVehicleState()
        {
            IsCommand("info", "Print vehicle info");
            HasOption("dxy=", $"Delta XY distance (meters) for key move commands (default={_deltaXY})", (int _) => _deltaXY = _);
            HasOption("dz=", $"Delta altitude (meters) for key move commands (default={_deltaZ})", (int _) => _deltaZ = _);
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
                {nameof(HeartbeatPayload.SystemStatus),vehicle.RawHeartbeat.Value?.SystemStatus.ToString() ?? string.Empty},
                {nameof(HeartbeatPayload.Type),vehicle.RawHeartbeat.Value?.Type.ToString()?? string.Empty },
                {nameof(HeartbeatPayload.Autopilot),vehicle.RawHeartbeat.Value?.Autopilot.ToString()?? string.Empty},
                {nameof(HeartbeatPayload.BaseMode),vehicle.RawHeartbeat.Value?.BaseMode.ToString("F")?? string.Empty},
                {nameof(HeartbeatPayload.CustomMode),vehicle.RawHeartbeat.Value?.CustomMode.ToString() ?? string.Empty},
                {nameof(HeartbeatPayload.MavlinkVersion),vehicle.RawHeartbeat.Value?.MavlinkVersion.ToString() ?? string.Empty},
                {nameof(SysStatusPayload.BatteryRemaining),vehicle.RawSysStatus.Value?.BatteryRemaining.ToString() ?? string.Empty},
                {nameof(SysStatusPayload.CurrentBattery),vehicle.RawSysStatus.Value?.CurrentBattery.ToString() ?? string.Empty},
                {nameof(SysStatusPayload.DropRateComm), TextRender.Progress(((vehicle.RawSysStatus.Value?.DropRateComm) ?? 0) / 10000.0, percentWidth )},
                {nameof(SysStatusPayload.ErrorsComm),vehicle.RawSysStatus.Value?.ErrorsComm.ToString() ?? string.Empty},
                {nameof(SysStatusPayload.Load),TextRender.Progress((vehicle.RawSysStatus.Value?.Load ?? 0) / 1000.0, percentWidth )},
                {nameof(SysStatusPayload.VoltageBattery),vehicle.RawSysStatus.Value?.VoltageBattery.ToString() ?? string.Empty},
                {nameof(Vehicle.Gps),vehicle.Gps.Value.ToString() },
                {nameof(Vehicle.Home),vehicle.Home.Value.ToString() },
                {nameof(GpsRawIntPayload.Alt),((vehicle.RawGpsRawInt.Value?.Alt ?? double.NaN) / 1000.0).ToString("F1")},
                {"LastCommand",_lastCommand }
                
            };
            GetAddidtionslParams(vehicle, dict);
            
            TextTable.PrintKeyValue(Console.WriteLine, new DoubleTextTableBorder(), dict.Max(_=>_.Key.Length),dict.Max(_=>_.Value.Length), "Vehicle", dict);

            var help = new Dictionary<string, string>
            {
                {"Arrows","Move XY" },
                {"U and D","Up/down altitude" },
                {"PageUp and PageDown","Up / Down max speed (MPC_XY_VEL_MAX)" },
                {"A","Arm" },
                {"T","Take off" },
                {"Q","Exit" },
            };
            TextTable.PrintKeyValue(Console.WriteLine, new DoubleTextTableBorder(), help.Max(_ => _.Key.Length), help.Max(_ => _.Value.Length), "Commands", help);

        }

        private void KeyListen(object a)
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.RightArrow:
                        _lastCommand = $"GOTO: delta={_deltaXY} Azimuth=90";
                        var newPoint = GeoMath.RadialPoint(Vehicle.Gps.Value, _deltaXY, 90);
                        Vehicle.GoTo(20,true,float.NaN,(float)newPoint.Latitude, (float)newPoint.Longitude, (float)newPoint.Altitude,_cancel.Token).Wait();
                        break;
                    case ConsoleKey.LeftArrow:
                        _lastCommand = $"GOTO: delta={_deltaXY} Azimuth=270";
                        var newPoint1 = GeoMath.RadialPoint(Vehicle.Gps.Value, _deltaXY, 270);
                        Vehicle.GoTo(20, true, float.NaN, (float)newPoint1.Latitude, (float)newPoint1.Longitude, (float)newPoint1.Altitude, _cancel.Token).Wait();
                        break;
                    case ConsoleKey.UpArrow:
                        _lastCommand = $"GOTO: delta={_deltaXY} Azimuth=0";
                        var newPoint2 = GeoMath.RadialPoint(Vehicle.Gps.Value, _deltaXY, 0);
                        Vehicle.GoTo(20, true, float.NaN, (float)newPoint2.Latitude, (float)newPoint2.Longitude, (float)newPoint2.Altitude, _cancel.Token).Wait();
                        break;
                    case ConsoleKey.DownArrow:
                        _lastCommand = $"GOTO: delta={_deltaXY} Azimuth=180";
                        var newPoint3 = GeoMath.RadialPoint(Vehicle.Gps.Value, _deltaXY, 180);
                        Vehicle.GoTo(20, true, float.NaN, (float)newPoint3.Latitude, (float)newPoint3.Longitude, (float)newPoint3.Altitude, _cancel.Token).Wait();
                        break;
                    case ConsoleKey.U:
                        _lastCommand = $"Down: H={Vehicle.Gps.Value.Altitude:F1} D=+{_deltaZ}";
                        var newPoint4 = Vehicle.Gps.Value.AddAltitude(_deltaZ);
                        Vehicle.GoTo(20, true, float.NaN, (float)newPoint4.Latitude, (float)newPoint4.Longitude, (float)newPoint4.Altitude, _cancel.Token).Wait();
                        break;
                    case ConsoleKey.D:
                        var newPoint5 = Vehicle.Gps.Value.AddAltitude(-_deltaZ);
                        _lastCommand = $"Down: H={Vehicle.Gps.Value.Altitude:F1} D=-{_deltaZ}";
                        Vehicle.GoTo(20, true, float.NaN, (float)newPoint5.Latitude, (float)newPoint5.Longitude, (float)newPoint5.Altitude, _cancel.Token).Wait();
                        break;
                    case ConsoleKey.PageUp:
                        var p = Vehicle.ReadParam("MPC_XY_VEL_MAX", CancellationToken.None).Result;
                        Vehicle.WriteParam("MPC_XY_VEL_MAX", p.RealValue.Value + 1.0f, _cancel.Token).Wait();
                        _lastCommand = $"MPC_XY_VEL_MAX: {p.RealValue.Value} D=+1";

                        break;
                    case ConsoleKey.PageDown:
                        var p2 = Vehicle.ReadParam("MPC_XY_VEL_MAX", CancellationToken.None).Result;
                        Vehicle.WriteParam("MPC_XY_VEL_MAX", p2.RealValue.Value - 1.0f, _cancel.Token).Wait();
                        _lastCommand = $"MPC_XY_VEL_MAX: {p2.RealValue.Value} D=-1";
                        break;
                    case ConsoleKey.Q:
                        _cancel.Cancel(false);
                        break;
                    case ConsoleKey.T:
                        _lastCommand = $"Takeoff";
                        Vehicle.TakeOff(0,90, (float) Vehicle.Gps.Value.Latitude, (float) Vehicle.Gps.Value.Longitude, (float) (Vehicle.Gps.Value.Altitude.Value + 50f), CancellationToken.None).Wait();
                        break;
                    case ConsoleKey.A:
                        _lastCommand = $"Armed";
                        Vehicle.Arm(CancellationToken.None).Wait();
                        break;
                    
                }
            }
        }
    }
}