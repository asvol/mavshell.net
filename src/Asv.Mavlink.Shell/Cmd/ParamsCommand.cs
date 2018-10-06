﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink.V2.Common;
using ManyConsole;

namespace Asv.Mavlink.Shell
{
    public class ParamsCommand : VehicleCommandBase
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly Subject<ConsoleKeyInfo> _userInput = new Subject<ConsoleKeyInfo>();
        private int _pageSize = 30;
        private string _search;
        private int _skip;

        public ParamsCommand()
        {
            IsCommand("params", "Read all params from Vehicle");
            HasOption("p|pageSize=", $"max size of rows in table. Default={_pageSize}", (int p) => _pageSize = p);
            Task.Factory.StartNew(_=>KeyListen(), _cancel.Token);
            Console.CancelKeyPress += Console_CancelKeyPress;
            _userInput.Where(_=>_.Key == ConsoleKey.Backspace && !string.IsNullOrEmpty(_search)).Subscribe(_=>
            {
                _skip = 0;
                _search = _search.Substring(0, _search.Length - 1);
                Redraw();
            });
            _userInput.Where(_ => char.IsLetterOrDigit(_.KeyChar) || _.KeyChar == '_').Subscribe(_ =>
            {
                _skip = 0;
                _search += _.KeyChar;
                Redraw();
            });
            _userInput.Where(_ => _.Key == ConsoleKey.RightArrow).Subscribe(_ =>
            {
                _skip += _pageSize;
                Redraw();
            });
            _userInput.Where(_ => _.Key == ConsoleKey.LeftArrow).Subscribe(_ =>
            {
                _skip -= _pageSize;
                if (_skip < 0) _skip = 0; 
                Redraw();
            });
        }

        private void KeyListen()
        {
            while (!_cancel.IsCancellationRequested)
            {
                var key = Console.ReadKey(true);
                _userInput.OnNext(key);
            }
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.Cancel) _cancel.Cancel(false);
        }

        protected override IVehicle CreateVehicle(VehicleConfig config)
        {
            return new Vehicle(config);
        }

        protected override async Task<int> RunAsync(IVehicle vehicle)
        {
            await vehicle.ReadAllParams(CancellationToken.None, new Progress<double>(_ => Console.WriteLine("Read params progress:" + TextRender.Progress(_, 20))));
            
            while (!_cancel.IsCancellationRequested)
            {
                Redraw();
                await Task.Delay(1000).ConfigureAwait(false);
            }
            return 0;
        }

        private void Redraw()
        {
            Console.Clear();
            Console.WriteLine("Use Left/Right arrows for page navigation (<-|->) and type for search");
            Console.Write("Search:"+_search);
            var top = Console.CursorTop;
            var left = Console.CursorLeft;
            Console.WriteLine();

            var items =Vehicle.Params.Values
                .Where(_ => _search.IsNullOrWhiteSpace() ||
                            _.Name.Contains(_search, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(_ => _.Name).ToArray();
            Console.WriteLine($"Show [{_skip} - {_skip + _pageSize}] of {items.Length}. All {Vehicle.Params.Count} items: ");
            TextTable.PrintTableFromObject(Console.WriteLine, new DoubleTextTableBorder(), 1, int.MaxValue, items.Skip(_skip).Take(_pageSize) );
            Console.SetCursorPosition(left,top);
        }

    }
}