using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink.Decoder;
using Asv.Mavlink.V2.Common;
using ManyConsole;

namespace Asv.Mavlink.Shell
{
    public class ListenCommand:ConsoleCommand
    {
        private int _port;
        readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private IPEndPoint _from;
        private UdpClient _recv;
        private ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();
        private List<DisplayRow> _items = new List<DisplayRow>();
        private IPEndPoint _sendTo;
        private DateTime _lastUpdate;

        public ListenCommand()
        {
            IsCommand("listen", "Listen MAVLink packages and print statistic");
            HasOption("p|port=", $"UDP port. Default '{_port}'", (int _) => _port = _);
        }

        public override int Run(string[] remainingArguments)
        {
            var listenerIp = new IPEndPoint(IPAddress.Any, 14560);
            _recv = new UdpClient(listenerIp);

            Task.Factory.StartNew(AsyncListen,_cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Task.Factory.StartNew(KeyListen, _cancel.Token,TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _lastUpdate = DateTime.Now;
            while (!_cancel.IsCancellationRequested)
            {
                Redraw();
                Thread.Sleep(3000);
            }
            return 0;
        }

        private void Redraw()
        {
            
            DisplayRow[] items;
            try
            {
                _rw.EnterReadLock();
                items = _items.ToArray();
            }
            finally
            {
                _rw.ExitReadLock();
            }
            var time = DateTime.Now - _lastUpdate;
            foreach (var item in items)
            {
                item.Freq = (item.Count/ time.TotalSeconds).ToString("0.0 Hz");
                item.Count = 0;
            }
            _lastUpdate = DateTime.Now;
            Console.Clear();
            TextTable.PrintTableFromObject(Console.WriteLine,new  DoubleTextTableBorder(),1,int.MaxValue,items,_=>_.Msg,_=>_.Message,_=>_.Freq);

            Console.WriteLine("Press Q for exit");
        }

        private void KeyListen()
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

        private void AsyncListen()
        {
            var decoder = new PacketV2Decoder();
            decoder.RegisterCommonDialect();
            decoder.Subscribe(OnPacket);
            while (true)
            {
                _from = new IPEndPoint(IPAddress.Any, 0);
                var a = _recv.Receive(ref _from);
                _sendTo = _from;
                foreach (var b in a)
                {
                    decoder.OnNext(b);
                }
            }
        }

        public void OnPacket(IPacketV2<IPayload> packet)
        {
            try
            {
                _rw.EnterWriteLock();
                var exist = _items.FirstOrDefault(_ => packet.MessageId == _.Msg);
                if (exist == null)
                {
                    _items.Add(new DisplayRow {Msg = packet.MessageId, Message = packet.Name});
                }
                else
                {
                    exist.Count++;
                }
            }
            finally
            {
                _rw.ExitWriteLock();
            }
        }
    }

    internal class DisplayRow
    {
        public string Message { get; set; }
        public int Count { get; set; }
        public string Freq { get; set; }
        public int Msg { get; set; }
    }
}