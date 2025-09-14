# EnumCreator Pro

**Professional Enum Management Tool for Unity**

A powerful Unity Editor tool that simplifies the creation, management, and generation of C# enums with a professional, user-friendly interface.

## ✨ Features

- **Visual Enum Editor**: Create and edit enums through an intuitive Inspector interface
- **Automatic Code Generation**: Generates clean, professional C# enum files automatically
- **Namespace Management**: Organize your enums with custom namespaces
- **Flags Support**: Built-in support for flag enums with automatic powers-of-2 numbering
- **Tooltip Integration**: Add tooltips to enum values for better documentation
- **Value Protection**: Prevent accidental changes to existing enum values
- **Professional UI**: Clean, modern interface designed for Asset Store quality

## 🚀 Quick Start

1. **Create an Enum Definition**:
   - Right-click in your Project window
   - Navigate to `Create > Enum Creator > Enum Definition`
   - Name your enum definition asset

2. **Configure Your Enum**:
   - Select the enum definition asset
   - Set the namespace and flags options
   - Add your enum values with optional tooltips

3. **Generate Code**:
   - Click "Regenerate" or "Apply Changes" in the Inspector
   - Your enum code will be automatically generated in the specified folder

## ⚙️ Settings

Access the settings through `Tools > Enum Creator > Settings` to configure:

- **Default Namespace**: Set the default namespace for new enums
- **Generated Enums Path**: Choose where generated files are saved
- **Default Flags**: Set whether new enums use flags by default
- **Tooltip Integration**: Enable/disable tooltip generation
- **Auto-Generated Headers**: Control header comments in generated files

## 📁 File Structure

```
Assets/
├── EnumCreator/
│   ├── Editor/           # Editor scripts
│   ├── Runtime/          # Runtime scripts
│   └── Settings/         # Settings assets
└── GeneratedEnums/       # Generated enum files
    ├── MyEnum.cs
    └── AnotherEnum.cs
```

## 🎯 Use Cases

- **Game States**: Define game states, UI states, or player states
- **Item Types**: Create item categories, weapon types, or building types
- **Flags**: Use for permissions, abilities, or multi-select options
- **Constants**: Define named constants with better IntelliSense support

## 🔧 Advanced Features

### Flags Enums
When "Use as Flags" is enabled, the tool automatically:
- Adds `[System.Flags]` attribute
- Uses powers-of-2 numbering (1, 2, 4, 8...)
- Generates proper bitwise operation support

### Value Protection
Enable "Prevent Value Name Changes" to:
- Protect existing enum values from accidental modification
- Allow new values to be added freely
- Maintain backward compatibility

### Tooltip Integration
Add tooltips to enum values for:
- Better IntelliSense documentation
- Self-documenting code
- Improved developer experience

## 📖 Documentation

For detailed documentation, examples, and advanced usage, visit our [GitHub repository](https://github.com/yourusername/enumcreator).

## 🐛 Support

- **Bug Reports**: [GitHub Issues](https://github.com/yourusername/enumcreator/issues)
- **Feature Requests**: [GitHub Discussions](https://github.com/yourusername/enumcreator/discussions)
- **Documentation**: [Wiki](https://github.com/yourusername/enumcreator/wiki)

## 📄 License

This tool is licensed under the MIT License. See LICENSE file for details.

## 🙏 Acknowledgments

Built with ❤️ for the Unity community. Special thanks to all contributors and users who help improve this tool.

---

**Version**: 1.0.0  
**Unity Version**: 2021.3+  
**Platform**: Windows, macOS, Linux
