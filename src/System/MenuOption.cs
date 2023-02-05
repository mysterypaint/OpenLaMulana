using System;

namespace OpenLaMulana.System
{
    internal partial class Menu
    {
        public class MenuOption
        {
            public string Name;
            public MenuTypes ElementType;
            public Func<int[], int> Function;
            public OptionsMenuPages Page;
            public float Value;
            public float CommittedValue;
            public float DefaultValue;
            public int[] Args;
            public string[] Strings;

            public MenuOption(string name, MenuTypes elementType, Func<int[], int> function, OptionsMenuPages page, float sliderDefaultValue, int[] sliderRange, string[] strings)
            {
                Name = name;
                ElementType = elementType;
                Function = function;
                Page = page;
                Value = sliderDefaultValue;
                CommittedValue = sliderDefaultValue;
                DefaultValue = sliderDefaultValue;
                Args = sliderRange;
                Strings = strings;
            }
        }
    }
}
