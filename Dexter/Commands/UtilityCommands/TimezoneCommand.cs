﻿using Dexter.Attributes.Methods;
using Dexter.Enums;
using Dexter.Extensions;
using Dexter.Helpers;
using Discord.Commands;
using Humanizer;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Dexter.Commands {
    public partial class UtilityCommands {

        /// <summary>
        /// Gives general information about time zones, or compares them, or searches for one, depending on <paramref name="Action"/>.
        /// </summary>
        /// <param name="Action">The action to take respective to other arguments. Leave empty for general information.</param>
        /// <param name="Argument">Everything that comes after the <paramref name="Action"/> when the command is run, generally a date or time zone.</param>
        /// <returns>A <c>Task</c> object, which can be awaited until the method completes successfully.</returns>

        [Command("timezone")]
        [Summary("Gives information about time zones or searches for a specific one.")]
        [ExtendedSummary("Gives information about time zones or searches for a specific one.\n" +
            "`timezone SEARCH [TERM]` - Looks for similar timezone names and gives information about each one.\n" +
            "`timezone SPAN [TIMEZONE]` - Looks for time zones with similar or exactly the same offsets as given.\n" +
            "`timezone GET [ABBREVIATION]` - Gets information about one specific time zone.\n" +
            "`timezone WHEN (DATE) [TIME] (ZONE)` - Gets the time difference between now and the specified date & time in the given time zone.\n" +
            "`timezone NOW [TIMEZONE]` - Gets the current time in a given time zone.")]
        [BotChannel]

        public async Task TimezoneCommand(string Action = "", [Remainder] string Argument = "") {
            
            if(string.IsNullOrEmpty(Action) || Action.ToLower() == "info") {
                await BuildEmbed(EmojiEnum.Sign)
                    .WithTitle("Time Zone Info")
                    .WithDescription("Time Zones are used to coordinate the times you input with those of other members in different parts of the world.\n" +
                        "Many countries use a mechanism called **Daylight Saving Time** (DST), whereby time gets advanced for 1 hour during the summer half of the year.\n" +
                        "When inputting your time zone, make sure to check whether your local area uses DST or not, and specify the correct time zone.\n" +
                        $"Dexter is running in {TimeZoneData.ToTimeZoneExpression(DateTimeOffset.Now.Offset)}.")
                    .WithCurrentTimestamp()
                    .SendEmbed(Context.Channel);
                    return;
            }

            switch(Action.ToLower()) {
                case "search":
                    if(string.IsNullOrEmpty(Argument)) {
                        await BuildEmbed(EmojiEnum.Annoyed)
                            .WithTitle("Invalid number of arguments!")
                            .WithDescription("You must provide a Time Zone Abbreviation to search for!")
                            .SendEmbed(Context.Channel);
                        return;
                    } 

                    {
                        string[] Results = LanguageHelper.SearchTimeZone(Argument, LanguageConfiguration);
                        string[] ResultsHumanized = new string[Math.Min(10, Results.Length)];

                        for (int i = 0; i < ResultsHumanized.Length; i++)
                            ResultsHumanized[i] = $"{Results[i]}: {LanguageConfiguration.TimeZones[Results[i]]}";

                        await BuildEmbed(EmojiEnum.Love)
                            .WithTitle("Top 10 Results")
                            .WithDescription(string.Join("\n", ResultsHumanized))
                            .SendEmbed(Context.Channel);
                    }
                    return;
                case "span":
                    if(string.IsNullOrEmpty(Argument)) {
                        await BuildEmbed(EmojiEnum.Annoyed)
                            .WithTitle("Invalid number of arguments!")
                            .WithDescription("You must provide a Time Zone Expression to search for!")
                            .SendEmbed(Context.Channel);
                        return;
                    }

                    {
                        TimeZoneData TimeZone = TimeZoneData.Parse(Argument, LanguageConfiguration);

                        string[] Results = LanguageHelper.SearchTimeZone(TimeZone.TimeOffset, LanguageConfiguration, out int Exact);
                        string[] ResultsHumanized = new string[Math.Min(Math.Max(Exact, 10), Results.Length)];

                        for (int i = 0; i < ResultsHumanized.Length; i++)
                            ResultsHumanized[i] = $"{Results[i]}: {LanguageConfiguration.TimeZones[Results[i]]}";

                        await BuildEmbed(EmojiEnum.Love)
                            .WithTitle($"Top {ResultsHumanized.Length} Results similar to {TimeZoneData.ToTimeZoneExpression(TimeZone.Offset)}")
                            .WithDescription(string.Join("\n", ResultsHumanized))
                            .SendEmbed(Context.Channel);
                    }
                    return;
                case "now":
                    if (string.IsNullOrEmpty(Argument)) {
                        await BuildEmbed(EmojiEnum.Annoyed)
                            .WithTitle("Invalid number of arguments!")
                            .WithDescription("You must provide a Time Zone Expression to search for!")
                            .SendEmbed(Context.Channel);
                        return;
                    } 
                    
                    {
                        TimeZoneData TimeZone = TimeZoneData.Parse(Argument, LanguageConfiguration);

                        await Context.Channel.SendMessageAsync($"It is currently **{DateTimeOffset.Now.ToOffset(TimeZone.TimeOffset):dddd',' MMMM d',' hh:mm tt}** in {TimeZoneData.ToTimeZoneExpression(TimeZone.Offset)}.");
                    }
                    return;
                case "get":
                    if (string.IsNullOrEmpty(Argument)) {
                        await BuildEmbed(EmojiEnum.Annoyed)
                                .WithTitle("Invalid number of arguments!")
                                .WithDescription("You must provide a Time Zone Abbreviation to search for!")
                                .SendEmbed(Context.Channel);
                        return;
                    }
                    
                    if(!LanguageConfiguration.TimeZones.ContainsKey(Argument)) {
                        string[] Results = LanguageHelper.SearchTimeZone(Argument, LanguageConfiguration);
                        string[] ResultsHumanized = new string[Math.Min(3, Results.Length)];

                        for (int i = 0; i < ResultsHumanized.Length; i++)
                            ResultsHumanized[i] = $"{Results[i]}: {LanguageConfiguration.TimeZones[Results[i]]}";

                        await BuildEmbed(EmojiEnum.Annoyed)
                            .WithTitle("Unable to find time zone!")
                            .WithDescription($"Did you mean...\n{string.Join("\n", ResultsHumanized)}")
                            .SendEmbed(Context.Channel);
                        return;
                    }

                    await BuildEmbed(EmojiEnum.Love)
                        .WithTitle("Found time zone!")
                        .WithDescription($"{Argument}: {LanguageConfiguration.TimeZones[Argument]}")
                        .SendEmbed(Context.Channel);
                    return;
                case "when":
                case "until":
                case "till":
                    if (string.IsNullOrEmpty(Argument)) {
                        await BuildEmbed(EmojiEnum.Annoyed)
                                .WithTitle("Invalid number of arguments!")
                                .WithDescription("You must provide a Date and Time to compare to!")
                                .SendEmbed(Context.Channel);
                        return;
                    }

                    if (!LanguageHelper.TryParseTime(Argument, CultureInfo.CurrentCulture, LanguageConfiguration, out DateTimeOffset Time)) {
                        await BuildEmbed(EmojiEnum.Annoyed)
                            .WithTitle("Failed to parse date!")
                            .WithDescription($"Make sure you follow the format `Month dd(, yyyy) hh:mm(:ss) (<am/pm>) (TZ)` or `{CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern} hh:mm(:ss) (<am/pm>) (TZ))`.")
                            .SendEmbed(Context.Channel);
                    }

                    await BuildEmbed(EmojiEnum.Love)
                        .WithTitle("Found Time!")
                        .WithDescription($"{Time:MMM dd, yyyy hh:mm tt 'UTC'zzz} {(Time.CompareTo(DateTimeOffset.Now) < 0 ? "happened" : "will happen")} {Time.Humanize()}.")
                        .SendEmbed(Context.Channel);

                    return;
                default:
                    await BuildEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Unrecognized action!")
                        .WithDescription($"Unable to parse action \"`{Action}`\"! \nFor more information on accepted actions, check out the `help timezone` command.")
                        .SendEmbed(Context.Channel);
                    return;
            }
        }
    }
}
