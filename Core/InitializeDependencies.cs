﻿using Dexter.Core.Abstractions;
using Dexter.Core.DiscordApp;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dexter.Core {
    public static class InitializeDependencies {
        private static ServiceProvider Services;

        public static async Task Main() {
            ServiceCollection ServiceCollection = new ServiceCollection();

            ServiceCollection.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 1000 }));

            ServiceCollection.AddSingleton<CommandService>();

            Assembly.GetExecutingAssembly().GetTypes().Where(Type => Type.IsSubclassOf(typeof(JSONConfiguration)) && !Type.IsAbstract).ToList().ForEach(Type => {
                if (!File.Exists($"{Type.Name}.json")) {
                    File.WriteAllText($"{Type.Name}.json", JsonSerializer.Serialize(Activator.CreateInstance(Type), new JsonSerializerOptions() { WriteIndented = true }));
                    ServiceCollection.AddSingleton(Type);
                } else
                    ServiceCollection.AddSingleton(Type, JsonSerializer.Deserialize(File.ReadAllText($"{Type.Name}.json"), Type, new JsonSerializerOptions() { WriteIndented = true }));
            });

            Assembly.GetExecutingAssembly().GetTypes().Where(Type => Type.IsSubclassOf(typeof(InitializableModule)) && !Type.IsAbstract).ToList().ForEach(Type => ServiceCollection.AddSingleton(Type));
            
            Services = ServiceCollection.BuildServiceProvider();

            Assembly.GetExecutingAssembly().GetTypes().Where(Type => Type.IsSubclassOf(typeof(InitializableModule)) && !Type.IsAbstract).ToList().ForEach(Type => (Services.GetService(Type) as InitializableModule).AddDelegates());
            
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnProcessExit);

            await Services.GetRequiredService<FrontendConsole>().RunAsync();
        }

        public static void OnProcessExit(object Sender, EventArgs Arguments) {
            Assembly.GetExecutingAssembly().GetTypes().Where(Type => Type.IsSubclassOf(typeof(JSONConfiguration)) && !Type.IsAbstract).ToList().ForEach(Type => {
                File.WriteAllText($"{Type.Name}.json", JsonSerializer.Serialize(Services.GetService(Type), new JsonSerializerOptions() { WriteIndented = true }));
            });
        }
    }
}
