using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OSSForts.Globals;

public static class SettingsUtils
{
    public static void AddCustomProjectSetting<T>(string name, T defaultValue, PropertyHint hint = PropertyHint.None, string hint_string = "")
    {
        if (ProjectSettings.HasSetting(name))
        {
            return;
        }

        Variant value = Variant.From(defaultValue);


        ProjectSettings.SetSetting(name, value);
        ProjectSettings.AddPropertyInfo(new Godot.Collections.Dictionary()
            {
                { "name", name },
                { "type", (int)value.VariantType },
                { "hint", (int)hint },
                { "hint_string", hint_string },
            });
        ProjectSettings.SetInitialValue(name, value);
    }
}

