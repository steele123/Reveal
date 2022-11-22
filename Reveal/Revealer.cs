using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LCUSharp;
using LCUSharp.Websocket;
using Spectre.Console;

namespace Reveal
{
    public class Revealer
    {
        private LeagueClientApi _api;

        private ChampSelect? _champSelect;

        private CancellationTokenSource _cts;

        public async Task Connect()
        {
            await AnsiConsole.Status()
                .SpinnerStyle(new Style().Foreground(Color.Yellow))
                .StartAsync("Connecting to League Client...", async ctx =>
                {
                    _api = await LeagueClientApi.ConnectAsync();
                    ctx.Status("Connected!");
                    ctx.SpinnerStyle(new Style().Foreground(Color.Green));
                    await Task.Delay(3000);
                });
            
            // Hook Events
            _api.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", OnGameFlowChanged);
            _api.Disconnected += OnDisconnected;

            var source = new CancellationTokenSource();
            _cts = source;
            await Task.Run(() => LiveTable(source.Token), source.Token);
            AnsiConsole.Clear();
        }

        public async Task LiveTable(CancellationToken ct)
        {
            var table = new Table().Centered();
            table.AddColumn("Ally 1");
            table.AddColumn("Ally 2");
            table.AddColumn("Ally 3");
            table.AddColumn("Ally 4");
            table.AddColumn("Ally 5");

            await AnsiConsole.Live(table)
                .StartAsync(async ctx =>
                {
                    var tableTitle = new TableTitle("No Active Lobby", Style.Plain);
                    table.Title = tableTitle;
                    
                    ctx.Refresh();
                    
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                        
                        if (_champSelect == null)
                        {
                            if (table.Rows.Count > 0 && table.Title.Text != "No Active Lobby")
                            {
                                var title = new TableTitle("No Active Lobby", new Style().Foreground(Color.Grey));
                                table.Title = title;
                                
                                table.Rows.Clear();
                                
                                ctx.Refresh();
                            }

                            await Task.Delay(1000);
                            continue;
                        }
                        
                        if (table.Rows.Count == 0)
                        {
                            var title = new TableTitle("In Lobby", Style.Plain.Foreground(Color.Green));
                            var names = _champSelect.Participants.Select(participant => participant.Name).ToList();
                            var captionStyle = new Style(Color.Blue, link: $"https://www.op.gg/multisearch/na?summoners={string.Join(",", names)}");
                            
                            table.Title = title;
                            table.Caption("CTRL + CLICK ME TO OPEN OP.GG", captionStyle);
                            table.AddRow(names.ToArray());
                            ctx.Refresh();
                        }

                        await Task.Delay(1000);
                    }
                });
        }

        private async void OnGameFlowChanged(object? sender, LeagueEvent e)
        {
            var result = e.Data.ToString();

            if (result != "ChampSelect")
            {
                _champSelect = null;
                return;
            }
            
            switch (result)
            {
                case "None":
                    break;
                case "Lobby":
                    break;
                case "ChampSelect":
                    await Task.Delay(3000);
                    
                    var champSelect =
                        await _api.RequestHandler.GetResponseAsync<ChampSelect>(HttpMethod.Get,
                            "/chat/v5/participants/champ-select");

                    _champSelect = champSelect;
                    break;
                case "GameStart":
                    break;
                case "InProgress":
                    break;
                case "WaitingForStats":
                    break;
                default:
                    break;
            }
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            _cts.Cancel();
        }
    }
}