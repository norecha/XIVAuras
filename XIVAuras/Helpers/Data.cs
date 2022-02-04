using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using XIVAuras.Config;

namespace XIVAuras.Helpers
{
    [StructLayout(LayoutKind.Explicit, Size = 0x8)]
    public struct Combo {
        [FieldOffset(0x00)] public float Timer;
        [FieldOffset(0x04)] public uint Action;
    }

    public struct TriggerData
    {
        public string Name;
        public uint Id;
        public ushort Icon;
        public byte MaxStacks;
        public uint[] ComboId;

        public TriggerData(string name, uint id, ushort icon, byte maxStacks = 0, uint[]? comboId = null)
        {
            Name = name;
            Id = id;
            Icon = icon;
            MaxStacks = maxStacks;
            ComboId = comboId ?? new uint[0];
        }
    }

    public struct RecastInfo
    {
        public float RecastTime;
        public float RecastTimeElapsed;
        public ushort MaxCharges;

        public RecastInfo(float recastTime, float recastTimeElapsed, ushort maxCharges)
        {
            RecastTime = recastTime;
            RecastTimeElapsed = recastTimeElapsed;
            MaxCharges = maxCharges;
        }
    }
    
    // AuraGroup settings that applies to all children auras
    public struct GroupOverrides
    {
        public Vector2 Position;
        public Vector2? IconSize = null;

        public GroupOverrides(Vector2 position)
        {
            this.Position = position;
        }

        public void Merge(GroupConfig groupConfig)
        {
            // position offsets are appended for each group.
            this.Position += groupConfig.Position;

            // icon sizes are entirely overwritten, if they exist
            if (groupConfig.IconSize.X > 0 && groupConfig.IconSize.Y > 0)
            {
                this.IconSize = groupConfig.IconSize;
            }
        }
    }
    
    public class DataSource
    {
        [JsonIgnore]
        private static readonly Dictionary<string, FieldInfo> _fields = 
            typeof(DataSource).GetFields().ToDictionary((x) => x.Name.ToLower());

        [JsonIgnore] public static readonly string[] TextTags = new string[]
        {
            "Text tags available for Cooldown/Status triggers:",
            "[value]    =>  The value tracked by the trigger, represents cooldown/duration.",
            "[maxvalue]",
            "[stacks]",
            "[maxstacks]",
            "",
            "Text tags available for CharacterState triggers:",
            "[name]",
            "[name_first]",
            "[name_last]",
            "[job]",
            "[jobname]",
            "[hp]",
            "[maxhp]",
            "[mp]",
            "[maxmp]",
            "[gp]",
            "[maxgp]",
            "[cp]",
            "[maxcp]",
        };

        public string GetFormattedString(string format, string numberFormat, int rounding)
        {
            return TextTagFormatter.TextTagRegex.Replace(
                format,
                new TextTagFormatter(this, numberFormat, rounding, _fields).Evaluate);
        }

        public DataSource()
        {
            this.Name_First = new LazyString<string?>(() => this.Name, LazyStringConverters.FirstName);
            this.Name_Last = new LazyString<string?>(() => this.Name, LazyStringConverters.LastName);
            this.JobName = new LazyString<Job>(() => this.Job, LazyStringConverters.JobName);
        }
                
        public float GetDataForSourceType(TriggerDataSource sourcetype) => sourcetype switch
        {
            TriggerDataSource.Value     => this.Value,
            TriggerDataSource.Stacks    => this.Stacks,
            TriggerDataSource.MaxStacks => this.MaxStacks,
            TriggerDataSource.HP        => this.Hp,
            TriggerDataSource.MP        => this.Mp,
            TriggerDataSource.CP        => this.Cp,
            TriggerDataSource.GP        => this.Gp,
            TriggerDataSource.Level     => this.Level,
            _                           => 0
        };

        public uint Id;
        public float Value;
        public float MaxValue;
        public int Stacks;
        public int MaxStacks;
        public ushort Icon;
        
        public string Name = string.Empty;
        public LazyString<string?>? Name_First;
        public LazyString<string?>? Name_Last;
        public Job Job;
        public LazyString<Job>? JobName;

        public uint Level;
        public float Hp;
        public float MaxHp;
        public float Mp;
        public float MaxMp;
        public float Gp;
        public float MaxGp;
        public float Cp;
        public float MaxCp;
        public bool HasPet;
    }
}
