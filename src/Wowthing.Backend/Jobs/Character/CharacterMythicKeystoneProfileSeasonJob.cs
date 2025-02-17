﻿using Wowthing.Backend.Models.API.Character;
using Wowthing.Lib.Models.Player;
using Wowthing.Lib.Models.Query;

namespace Wowthing.Backend.Jobs.Character
{
    public class CharacterMythicKeystoneProfileSeasonJob : JobBase
    {
        private const string ApiPath = "profile/wow/character/{0}/{1}/mythic-keystone-profile/season/{2}";

        public override async Task Run(params string[] data)
        {
            var query = JsonConvert.DeserializeObject<SchedulerCharacterQuery>(data[0]) ??
                        throw new InvalidJsonException(data[0]);
            var seasonId = int.Parse(data[1]);
            using var shrug = CharacterLog(query);

            var uri = GenerateUri(query, ApiPath, data[1]);
            var result = await GetJson<ApiCharacterMythicKeystoneProfileSeason>(uri);
            if (result.NotModified)
            {
                LogNotModified();
                return;
            }

            // Fetch character data
            var seasonMap = await Context.PlayerCharacterMythicPlusSeason
                .Where(mps => mps.CharacterId == query.CharacterId)
                .ToDictionaryAsync(k => k.Season);

            if (result.Data.BestRuns != null)
            {
                if (!seasonMap.TryGetValue(seasonId, out PlayerCharacterMythicPlusSeason season))
                {
                    season = new PlayerCharacterMythicPlusSeason
                    {
                        CharacterId = query.CharacterId,
                        Season = seasonId,
                    };
                    Context.PlayerCharacterMythicPlusSeason.Add(season);
                }

                season.Runs = result.Data.BestRuns
                    .EmptyIfNull()
                    .Select(run => new PlayerCharacterMythicPlusRun()
                    {
                        Affixes = run.Affixes.EmptyIfNull().Select(a => a.Id).ToList(),
                        Completed = run.CompletedTimestamp.AsUtcTimestamp(),
                        DungeonId = run.Dungeon.Id,
                        Duration = run.Duration,
                        KeystoneLevel = run.KeystoneLevel,
                        Members = run.Members.EmptyIfNull().Select(member => new PlayerCharacterMythicPlusRunMember
                        {
                            ItemLevel = member.ItemLevel,
                            Name = member.Character.Name,
                            RealmId = member.Character.Realm.Id,
                            SpecializationId = member.Specialization.Id,
                        }).ToList(),
                        Timed = run.Timed,
                    })
                    .ToList();
            }

            await Context.SaveChangesAsync();
        }
    }
}
