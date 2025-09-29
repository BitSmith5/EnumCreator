using System;
using System.Collections.Generic;
using System.Reflection;

namespace EnumEditor
{
    [Serializable]
    public class EnumInfo
    {
        public string enumName;
        public string namespaceName;
        public string filePath;
        public Type enumType;
        public bool isFlags;
        public List<EnumValueInfo> values;
        
        public EnumInfo()
        {
            values = new List<EnumValueInfo>();
        }
        
        public EnumInfo(string name, string ns, string path, Type type, bool flags = false)
        {
            enumName = name;
            namespaceName = ns;
            filePath = path;
            enumType = type;
            isFlags = flags;
            values = new List<EnumValueInfo>();
            
            if (type != null)
            {
                LoadValuesFromType();
            }
        }
        
        private void LoadValuesFromType()
        {
            if (enumType == null || !enumType.IsEnum) return;
            
            // Check if it's a flags enum
            isFlags = enumType.GetCustomAttribute<FlagsAttribute>() != null;
            
            // Get all enum values
            var enumValues = Enum.GetValues(enumType);
            var enumNames = Enum.GetNames(enumType);
            
            for (int i = 0; i < enumValues.Length; i++)
            {
                var value = enumValues.GetValue(i);
                var name = enumNames[i];
                var numericValue = Convert.ToInt32(value);
                
                values.Add(new EnumValueInfo
                {
                    name = name,
                    value = numericValue,
                    isObsolete = IsValueObsolete(name)
                });
            }
        }
        
        private bool IsValueObsolete(string valueName)
        {
            if (enumType == null) return false;
            
            var field = enumType.GetField(valueName);
            if (field == null) return false;
            
            return field.GetCustomAttribute<ObsoleteAttribute>() != null;
        }
        
        public int GetNextValue()
        {
            if (values.Count == 0) return 0;
            
            if (isFlags)
            {
                // For flags, find the next power of 2
                int maxValue = 0;
                foreach (var value in values)
                {
                    if (!value.isObsolete && value.value > maxValue)
                    {
                        maxValue = value.value;
                    }
                }
                
                // Find the next power of 2
                int nextPower = 1;
                while (nextPower <= maxValue)
                {
                    nextPower <<= 1;
                }
                
                return nextPower;
            }
            else
            {
                // For regular enums, find the next sequential value
                int maxValue = -1;
                foreach (var value in values)
                {
                    if (!value.isObsolete && value.value > maxValue)
                    {
                        maxValue = value.value;
                    }
                }
                
                return maxValue + 1;
            }
        }
        
        public bool HasValue(string valueName)
        {
            foreach (var value in values)
            {
                if (value.name.Equals(valueName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool IsValidValueName(string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName)) return false;
            
            // Check C# identifier rules
            if (!char.IsLetter(valueName[0]) && valueName[0] != '_') return false;
            
            for (int i = 1; i < valueName.Length; i++)
            {
                if (!char.IsLetterOrDigit(valueName[i]) && valueName[i] != '_') return false;
            }
            
            return true;
        }
    }
    
    [Serializable]
    public class EnumValueInfo
    {
        public string name;
        public int value;
        public bool isObsolete;
        
        public EnumValueInfo()
        {
        }
        
        public EnumValueInfo(string name, int value, bool isObsolete = false)
        {
            this.name = name;
            this.value = value;
            this.isObsolete = isObsolete;
        }
    }
}
