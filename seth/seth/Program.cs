using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace seth
{
    class Program
    {
        private static DiscordSocketConfig config = new DiscordSocketConfig(){
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.All

        };
        private static DiscordSocketClient client;
        private static Random rand = new Random();
        private static System.Timers.Timer color_timer = new System.Timers.Timer(rand.Next(2*60*60*1000));
        private static System.Timers.Timer color_timeout_timer = new System.Timers.Timer(8*60*60*1000);
        private static System.Timers.Timer save_timer = new System.Timers.Timer(30*60*1000);
        private static Dictionary<ulong, Player> players= new Dictionary<ulong, Player>();
        public static void Main(string[] args) =>
            MainAsync(args).GetAwaiter().GetResult();
        public static async Task MainAsync(string[] args)
        {
            var token = "super_secret_discord_token"; //real token not enclosed in this file

            client = new DiscordSocketClient(config);

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            client.Log += LogAsync;

            save_timer.Elapsed += Save;
            save_timer.AutoReset = true;
            save_timer.Enabled = true;

            color_timer.AutoReset = true;
            color_timer.Enabled = true;
            color_timer.Elapsed += DropColor;
            color_timeout_timer.Elapsed += ColorTryAgain;

            client.SlashCommandExecuted += SlashCommandExecuted;
            client.ButtonExecuted += ButtonHandler;
            client.Ready += Load;
            client.Ready += RegisterSlashCommands;
            
            await Task.Delay(-1);
        }
        private static async Task RegisterSlashCommands()
        {
            SlashCommandBuilder heartCommand = new SlashCommandBuilder()
                .WithName("hearts")
                .WithDescription("Learn more about collecting Colorful Hearts.");
            await client.CreateGlobalApplicationCommandAsync(heartCommand.Build());
            SlashCommandBuilder heartCheckCommand = new SlashCommandBuilder()
                .WithName("heartcheck")
                .WithDescription("See which hearts you need.");
            await client.CreateGlobalApplicationCommandAsync(heartCheckCommand.Build());
            SlashCommandBuilder balanceCheckCommand = new SlashCommandBuilder()
                .WithName("bal")
                .WithDescription("See your pebble balance.");
            await client.CreateGlobalApplicationCommandAsync(balanceCheckCommand.Build());
            SlashCommandBuilder saveCommand = new SlashCommandBuilder()
                .WithName("save")
                .WithDescription("Marcie command for Marcie purposes.");
            await client.CreateGlobalApplicationCommandAsync(saveCommand.Build());
            SlashCommandBuilder heartstormCommand = new SlashCommandBuilder()
                .WithName("heartstorm")
                .WithDescription("Coming soon...?");
            await client.CreateGlobalApplicationCommandAsync(heartstormCommand.Build());
        }

        private static async Task SlashCommandExecuted(SocketSlashCommand command)
        {
            if (!players.ContainsKey(command.User.Id))
            {
                players.Add(command.User.Id, new Player(command.User));
            }
            Player player= players[command.User.Id];
            switch(command.Data.Name)
            {
                case "hearts":
                    string output1 = "Catch Colorful Hearts by pressing the \"Grab\" button on a heart when it appears. You can't grab a heart you already have. Once you have all six, your hearts reset and you are rewarded with 100 pebbles. Pebbles don't have a use yet, but we're working on it ;3";
                    await command.RespondAsync(output1);
                    break;
                case "heartcheck":
                    string output2 = "You need the following hearts to complete your set:\n";
                    foreach (Heart heart in Enum.GetValues(typeof(Heart)))
                    {
                        if (!player.Hearts.Contains(heart))
                        {
                            output2 += ":"+heart.ToString().ToLower()+"_heart: ";
                        }
                    }
                    await command.RespondAsync(output2);
                    break;
                case "bal":
                    await command.RespondAsync($"You have {player.pebbles} pebbles.");
                    break;
                case "heartstorm":
                    if (command.User.Id == 198869784656347137 || command.User.Id == 525893656302059522)
                    {
                        await command.RespondAsync("It looks like a heart storm is coming!");
                        int times = rand.Next(3, 7);
                        for (int i = 0; i < times; i++){
                            DropColor();
                        }
                    }
                    break;
                case "save":
                    Save();
                    await command.RespondAsync("Data saved.");
                    break;
            }
        }

        private static async Task Load()
        {
            JArray memory = JArray.Parse(File.ReadAllText("memory.json"));
            foreach (JToken playerObj in memory.Children()){
                Player player = new Player(client.GetUser(playerObj.Value<ulong>("id")));
                var pebbles = playerObj.Value<int>("pebbles");
                player.pebbles = pebbles;
                var heartsMem = playerObj.Value<JToken>("hearts");
                foreach(JToken heartObj in heartsMem.Children())
                {
                    Enum.TryParse<Heart>(heartObj.ToString(), true, out var heart);
                    player.Hearts.Add(heart);
                }
                players.Add(player.user.Id, player);
            }
        }
        private static void Save(object? sender, EventArgs e)
        {
            Save();
            Console.WriteLine("Saving files...");
        }
        private static void Save()
        {
            JsonArray memory = new JsonArray();
            foreach (var player in players.Values)
            {
                memory.Add(player.Save());
            }
            File.WriteAllText("memory.json", memory.ToString());
        }
        private static void DropColor(object? sender, ElapsedEventArgs e)
        {
            DropColor();
        }
        private static void DropColor(){
            if (DateTime.Now.CompareTo(DateTime.Parse("08:00:00")) > 0)
            {
                var df_general = client.GetChannel(1088963139384660080) as SocketTextChannel; //real
                //var df_general = client.GetChannel(1345123646544543855) as SocketTextChannel; //test
                var drop_message = "**A Colorful Heart appears!**\n*Collect all six colors to earn 100 pebbles. Do /hearts for more info.*\n";
                ComponentBuilder button;
                var chance = rand.Next(6);
                switch (chance)
                {
                    case 0:
                        button = new ComponentBuilder()
                            .WithButton("Grab", "grab_red");
                        df_general.SendMessageAsync(drop_message+":heart:", components:button.Build());
                        break;
                    case 1:
                        button = new ComponentBuilder()
                            .WithButton("Grab", "grab_orange");
                        df_general.SendMessageAsync(drop_message+":orange_heart:", components:button.Build());
                        break;
                    case 2:
                        button = new ComponentBuilder()
                            .WithButton("Grab", "grab_yellow");
                        df_general.SendMessageAsync(drop_message+":yellow_heart:", components:button.Build());
                        break;
                    case 3:
                        button = new ComponentBuilder()
                            .WithButton("Grab", "grab_green");
                        df_general.SendMessageAsync(drop_message+":green_heart:", components:button.Build());
                        break;
                    case 4:
                        button = new ComponentBuilder()
                            .WithButton("Grab", "grab_blue");
                        df_general.SendMessageAsync(drop_message+":blue_heart:", components:button.Build());
                        break;
                    case 5:
                        button = new ComponentBuilder()
                            .WithButton("Grab", "grab_purple");
                        df_general.SendMessageAsync(drop_message+":purple_heart:", components:button.Build());
                        break;
                }
                color_timer.Enabled = false;
                color_timeout_timer.Enabled = true;
                color_timeout_timer.Start();
            }
        }
        private static async Task ButtonHandler(SocketMessageComponent component)
        {
            if (component.Data.CustomId.StartsWith("grab_"))
            {
                await component.DeferAsync();
                await component.Channel.SendMessageAsync(GrabHeart(component));
            }
        }
        private static string GrabHeart(SocketMessageComponent component)
        {
            if (!players.ContainsKey(component.User.Id))
            {
                players.Add(component.User.Id, new Player(component.User));
            }
            Player player= players[component.User.Id];
            Enum.TryParse<Heart>(component.Data.CustomId.Substring(5).ToUpper(), true, out var heart);
            if (player.Hearts.Contains(heart))
            {
                return "You've already got that heart, you can't pick it up.";
            }
            else {
                player.Hearts.Add(heart);
                component.Message.DeleteAsync();
                color_timer.Interval = rand.Next(2*60*60*1000);
                color_timer.Enabled = true;
                var output = $"Heart grabbed by {component.User.Username}!";
                if (player.Hearts.Count > 5)
                {
                    player.Hearts.Clear();
                    player.pebbles += 100;
                    output += "\nThat was the last heart you needed! Your hearts have been reset and you have earned 100 pebbles.";
                }
                return output;
            }
        }
        private static void ColorTryAgain(object? sender, ElapsedEventArgs e)
        {
            color_timeout_timer.Enabled = false;
            color_timer.Enabled = true;
        }
        private static Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"+ $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else 
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}

