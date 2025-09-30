# EnumEditor

**A Unity Editor tool for adding new values to existing enums directly from the Inspector.**

## Overview

Enum Editor is a powerful Unity Editor extension designed to boost designer productivity by allowing new enum values to be added directly from the Unity Inspector. When you have a public enum field exposed in the Inspector, Enum Editor automatically includes an add button next to each enum. This eliminates the need for designers to ask engineers for help when they need new enum values, as Unity by default only allows custom values for primitive types like int and string.

## Features

- **Inspector Integration**: Automatically adds add buttons to enum fields in the Inspector
- **Enum Detection**: Automatically finds and locates existing enums in your C# scripts
- **Smart Value Assignment**: Respects enum numbering (sequential for regular enums, powers of 2 for flags)
- **Validation**: Validates enum value names according to C# identifier rules
- **Confirmation Dialogs**: Shows confirmation dialogs before making changes
- **Settings Management**: Configurable settings for customization
- **Error Handling**: Comprehensive error handling and user feedback

## Quick Start

1. **Install EnumEditor** in your Unity project
2. **Create or use existing enums** in your C# scripts
3. **Expose enum fields** in MonoBehaviour or ScriptableObject classes
4. **Select objects** with enum fields in the Inspector
5. **Click the + button** next to enum fields to add new values
6. **Enter the new value name** and confirm the addition

## Usage

### Adding New Enum Values

1. Select a GameObject or ScriptableObject with enum fields in the Inspector
2. Find an enum field - you'll see a "+" button next to it
3. Click the "+" button to expand the add field
4. Enter the new enum value name
5. Click "Add Value" to add it to the enum
6. Confirm the addition in the dialog (if enabled)

### Settings

Access settings through `Tools > EnumEditor > Settings Window` to configure:

- **Enable/Disable**: Turn EnumEditor on or off
- **Confirmation Dialogs**: Show confirmation before adding values
- **Search Settings**: Configure how enums are found
- **UI Settings**: Customize button text and labels
- **Validation**: Control validation rules

## Requirements

- **Unity Version**: 2021.3 or newer
- **Platform**: Windows, macOS, Linux
- **Dependencies**: None (self-contained package)

## How It Works

1. **Enum Detection**: EnumEditor scans your project for enum definitions in C# scripts
2. **Inspector Integration**: Custom property drawers add "+" buttons to enum fields
3. **Value Addition**: When you add a new value, EnumEditor:
   - Validates the value name
   - Finds the enum definition file
   - Calculates the next appropriate value
   - Modifies the source file
   - Refreshes Unity's asset database

## Enum Types Supported

- **Regular Enums**: Sequential numbering (0, 1, 2, 3...)
- **Flags Enums**: Powers of 2 numbering (1, 2, 4, 8...)
- **Namespaced Enums**: Full namespace support
- **Custom Enums**: Any C# enum definition

## Validation Rules

- **C# Identifier Rules**: Value names must follow C# identifier rules
- **No Duplicates**: Prevents adding duplicate enum values
- **Empty Name Check**: Validates against empty or whitespace names
- **File Existence**: Ensures enum definition files exist

## Error Handling

EnumEditor provides comprehensive error handling:

- **File Not Found**: Clear error messages when enum files can't be located
- **Invalid Names**: Validation errors for invalid enum value names
- **Duplicate Values**: Warnings when trying to add existing values
- **Permission Issues**: File system permission error handling

## Settings Reference

### General Settings
- **Enable EnumEditor**: Master switch for the tool
- **Show Confirmation Dialog**: Show confirmation before adding values
- **Auto Refresh Assets**: Automatically refresh Unity's asset database

### Search Settings
- **Search Depth**: How deep to search for enum definitions
- **Include Generated Enums**: Whether to include generated enum folders

### UI Settings
- **Show Add Button for All Enums**: Enable/disable the add button
- **Add Button Text**: Customize the button text
- **New Value Field Label**: Customize the input field label

### Validation Settings
- **Validate Enum Names**: Enable C# identifier validation
- **Prevent Duplicates**: Prevent duplicate enum values
- **Allow Empty Names**: Allow empty enum value names

## Troubleshooting

### Common Issues

**"Could not find enum definition"**
- Ensure the enum is defined in a C# script
- Check that the enum is public or internal
- Verify the file is in the Assets folder

**"Invalid enum value name"**
- Use only letters, numbers, and underscores
- Start with a letter or underscore
- Avoid special characters

**"Value already exists"**
- Check if the value name already exists in the enum
- Use a different name for the new value

### Debug Information

Enable debug logging through Unity's Console to see detailed information about enum detection and modification processes.

## License

This tool is licensed under the MIT License. See LICENSE file for details.

## Support

For support, feature requests, or bug reports:
- **Email**: gallo.nicholas@gmail.com
- **Portfolio**: https://gallonicholas.myportfolio.com/

## Changelog

### Version 1.0.0
- Initial release
- Basic enum value addition functionality
- Inspector integration
- Settings management
- Validation and error handling

---

**Created by Nicholas R. Gallo**  
**Based on EnumCreator Pro**
