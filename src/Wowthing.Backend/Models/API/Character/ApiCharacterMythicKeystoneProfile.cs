﻿namespace Wowthing.Backend.Models.API.Character
{
    public class ApiCharacterMythicKeystoneProfile
    {
        [JsonProperty("current_period")]
        public ApiCharacterMythicKeystonePeriod CurrentPeriod { get; set; }

        public List<ApiObnoxiousObject> Seasons { get; set; }
    }

    public class ApiCharacterMythicKeystonePeriod
    {
        public ApiObnoxiousObject Period { get; set; }

        [JsonProperty("best_runs")]
        public List<ApiCharacterMythicKeystoneBestRun> BestRuns { get; set; }
    }

    public class ApiCharacterMythicKeystoneBestRun
    {
        public ApiObnoxiousObject Dungeon { get; set; }
        public int Duration { get; set; }
        public List<ApiCharacterMythicKeystoneMember> Members { get; set; }

        [JsonProperty("keystone_affixes")]
        public List<ApiObnoxiousObject> Affixes { get; set; }

        [JsonProperty("completed_timestamp")]
        public long CompletedTimestamp { get; set; }

        [JsonProperty("keystone_level")]
        public int KeystoneLevel { get; set; }

        [JsonProperty("is_completed_within_time")]
        public bool Timed { get; set; }
    }

    public class ApiCharacterMythicKeystoneMember
    {
        public ApiCharacterMythicKeystoneMemberCharacter Character { get; set; }
        public ApiObnoxiousObject Specialization { get; set; }

        [JsonProperty("equipped_item_level")]
        public int ItemLevel { get; set; }
    }

    public class ApiCharacterMythicKeystoneMemberCharacter
    {
        public string Name { get; set; }
        public ApiObnoxiousObject Realm { get; set; }
    }
}
