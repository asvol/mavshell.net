using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Asv.Mavlink.V2.Common;
using ManyConsole;

namespace Asv.Mavlink.Shell
{
    public class Command:ConsoleCommand
    {
        private string _connectionString = "udp://0.0.0.0:14560";
        private readonly Subject<IPacketV2<IPayload>> _input = new Subject<IPacketV2<IPayload>>();

        public Command()
        {
            IsCommand("cmd", "Send MAVLink command");
        }

        public override int Run(string[] remainingArguments)
        {
            var conn = new MavlinkV2Connection(_connectionString, _=>_.RegisterCommonDialect());


            conn.DeserizliaePackageErrors.Subscribe(_ => OnError(_));
            conn.Port.Error.Subscribe(_ => Console.WriteLine($"Connection error:{_.Message}"));
            conn.Where(_ => _.MessageId == StatustextPacket.PacketMessageId).Cast<StatustextPacket>()
                .Select(_ => new string(_.Payload.Text)).Subscribe(_ => Console.WriteLine(_));

            conn.Where(_ => _.MessageId == HeartbeatPacket.PacketMessageId).Cast<HeartbeatPacket>()
                .Subscribe(_ => Console.WriteLine($"BaseMode:{_.Payload.BaseMode:F} CustomMode:{_.Payload.CustomMode} "));

            //BaseMode: MavModeFlagCustomModeEnabled, MavModeFlagStabilizeEnabled, MavModeFlagManualInputEnabled, MavModeFlagSafetyArmed CustomMode:458752
            var a = conn.Take(1).ToTask().Result;
            Console.ReadLine();
            var cmd = new MavlinkCommandProtocol(conn);



            cmd.Execute(0, 255, 255, MavCmd.MavCmdDoSetMode, 0, CancellationToken.None,
                (float) (MavModeFlag.MavModeFlagCustomModeEnabled | MavModeFlag.MavModeFlagStabilizeEnabled | MavModeFlag.MavModeFlagManualInputEnabled | MavModeFlag.MavModeFlagSafetyArmed),
                458752f
                , 0f, 0f, 0f, 0f).Wait();

            //cmd.Execute(0, 255, 255, MavCmd.MavCmdComponentArmDisarm, 0, CancellationToken.None, 1, 0, 0, 0,0,0).Wait();
            
            //cmd.Execute(0,255,255, MavCmd.MavCmdNavTakeoff, 0, CancellationToken.None, float.NaN, float.NaN, float.NaN, 55.146524f, 61.406014f,50f).Wait();
            return 0;
        }

        private void OnError(DeserizliaePackageException deserizliaePackageException)
        {
            // Console.WriteLine(deserizliaePackageException);
        }
    }
}