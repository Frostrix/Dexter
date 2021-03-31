﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexter.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dexter.Databases.Games {

    /// <summary>
    /// Holds all relevant data for the Dexter Games subsystem.
    /// </summary>

    public class GamesDB : Database {

        /// <summary>
        /// Holds game-specific data, a set of GameInstances (or sessions).
        /// </summary>

        public DbSet<GameInstance> Games { get; set; }

        /// <summary>
        /// Holds player-specific data, like what game they're playing or their score.
        /// </summary>

        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// Gets a player from their unique user <paramref name="ID"/>.
        /// </summary>
        /// <param name="ID">The ID of the player to fetch.</param>
        /// <returns>A Player whose ID matches <paramref name="ID"/>, or a new one matching it.</returns>

        public Player GetOrCreatePlayer(ulong ID) {
            
            Player p = Players.Find(ID);
            if (p is null) {
                p = new() {
                    UserID = ID,
                    Playing = -1,
                    Data = "",
                    Lives = 0,
                    Score = 0
                };
                Console.Out.WriteLine($"Creating new player: #{ID}");
                Players.Add(p);
            }
            return p;
        }

        private int Count = 1;

        internal int GenerateGameToken() {
            while (Games.Find(Count) is not null) Count++;

            return Count;
        }
    }
}
