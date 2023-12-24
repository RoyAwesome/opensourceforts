using Godot;
using System;

using static OSSForts.Globals.SettingsUtils;

[GlobalClass]
[Tool]
public partial class GameSettings : Node
{
    public const string MapListPath = "Gamemode/Maps_Modes/Map_List";
    public override void _Ready()
    {
        AddCustomProjectSetting(MapListPath, new string[] { }, PropertyHint.ArrayType);
    }
}
