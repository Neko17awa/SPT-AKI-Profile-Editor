﻿using System.Text.Json.Serialization;

namespace SPT_AKI_Profile_Editor.Core.ServerClasses
{
    public class LocalesGlobalPreset
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
    }
}