﻿using Dexter.Configurations;
using Dexter.Abstractions;
using Discord.WebSocket;
using Dexter.Databases.CommunityEvents;
using Dexter.Databases.EventTimers;
using System.Linq;

namespace Dexter.Commands {

    /// <summary>
    /// The class containing all commands and utilities within the Community module.
    /// </summary>

    public partial class CommunityCommands : DiscordModule {

        /// <summary>
        /// Works as an interface between the configuration files attached to the Community module and its commands.
        /// </summary>

        public CommunityConfiguration CommunityConfiguration { get; set; }

        /// <summary>
        /// Loads the database containing events for the <c>~event</c> command.
        /// </summary>

        public CommunityEventsDB CommunityEventsDB { get; set; }
   
    }

}