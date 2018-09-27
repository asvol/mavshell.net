using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Asv.Mavlink.Decoder;
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
            var strm = RemoteStreamFactory.CreateStream(_connectionString);
            var decoder = new PacketV2Decoder();
            var encoder = new PacketV2Encoder();
            decoder.OutError.Subscribe(_ => OnError(_));
            encoder.Subscribe(strm);

            decoder.RegisterCommonDialect();
            strm.SelectMany(_ => _).Subscribe(decoder);
            strm.Start(CancellationToken.None);
            var a = decoder.Take(1).ToTask().Result;
            
            var cmd = new MavlinkCommandProtocol(decoder, encoder);
            //cmd.Execute(0,1,1, MavCmd.MavCmdDoSetMode, 0, CancellationToken.None, (float) MavMode.MavModeGuidedArmed, 0f,0f, 0f, 0f, 0f).Wait();
            //cmd.Execute(0, 255, 255, MavCmd.MavCmdComponentArmDisarm, 1, CancellationToken.None, 1, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN).Wait();
            cmd.Execute(0,1,1, MavCmd.MavCmdNavTakeoff, 0, CancellationToken.None, float.NaN, float.NaN, float.NaN, 55.146524f, 61.406014f,50f).Wait();
            return 0;
        }

        private void OnError(DeserizliaePackageException deserizliaePackageException)
        {
            // Console.WriteLine(deserizliaePackageException);
        }
    }
}