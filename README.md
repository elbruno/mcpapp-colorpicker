# 🎨 Color Picker MCP App

[![Build](https://github.com/elbruno/mcpapp-colorpicker/actions/workflows/build.yml/badge.svg)](https://github.com/elbruno/mcpapp-colorpicker/actions/workflows/build.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![MCP](https://img.shields.io/badge/MCP-Apps-blue)](https://modelcontextprotocol.github.io/ext-apps/api/)

A **Model Context Protocol (MCP) App** built with .NET that provides an interactive color picker with a rich UI. This is a sample implementation demonstrating how to build MCP Apps using C# and the `ModelContextProtocol.AspNetCore` package.

> **Note:** MCP Apps are a new extension to MCP that allows servers to provide interactive UI components. This sample demonstrates how to implement one in .NET/C#.

## ✨ Features

- **Interactive Color Picker UI** - Visual color selection with real-time preview
- **Multiple Color Formats** - HEX, RGB, and HSL with one-click copy
- **Quick Color Palette** - 24 preset colors for fast selection
- **Random Color Generator** - Generate random colors instantly
- **VS Code Theme Integration** - Automatically adapts to your VS Code theme
- **HTTP Transport** - Easy integration with MCP clients

## 🛠️ MCP Tool

### `ColorPicker`

Opens an interactive color picker UI to visually select a color.

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `initialColor` | string (optional) | Initial color in hex format (e.g., `#FF5733`). Default: `#3498DB` |

**Returns:** A `ColorPickerResult` with the UI resource URI for rendering the interactive picker.

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Visual Studio Code](https://code.visualstudio.com/) with MCP extension

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/elbruno/mcpapp-colorpicker.git
cd mcpapp-colorpicker
```

### 2. Build and run

```bash
dotnet restore
dotnet run
```

The server will start on `http://localhost:3001/mcp`.

### 3. Configure VS Code

Add the following to your VS Code MCP configuration (`.vscode/mcp.json`):

```json
{
  "servers": {
    "color-picker": {
      "type": "http",
      "url": "http://localhost:3001/mcp"
    }
  }
}
```

### 4. Use it

In VS Code with the MCP extension, invoke the `ColorPicker` tool to open the interactive color picker UI.

## 🏗️ Project Structure

```
mcpapp-colorpicker/
├── ColorPickerMcp.csproj   # Project file with MCP SDK reference
├── Program.cs              # MCP server implementation with embedded UI
├── README.md               # This file
├── LICENSE                 # MIT License
└── .github/
    └── workflows/
        └── build.yml       # GitHub Actions CI
```

## 🔧 Technical Details

This MCP App uses:

- **[ModelContextProtocol.AspNetCore](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore)** (`0.2.0-preview.1`) - Official .NET SDK for MCP
- **HTTP Transport** - Exposes MCP endpoint at `/mcp`
- **Embedded HTML UI** - Color picker interface served at `/ui/color-picker`
- **Attribute-based Tool Definition** - Uses `[McpServerToolType]` and `[McpServerTool]` attributes

### Endpoints

| Endpoint | Description |
|----------|-------------|
| `/mcp` | MCP protocol endpoint for client connections |
| `/ui/color-picker` | Interactive color picker HTML UI |
| `/mcp/resources/ui/color-picker` | UI as MCP resource |

## 📚 Resources

Learn more about MCP Apps:

- 📺 [VS Code MCP Apps Video](https://www.youtube.com/watch?v=HWmC3T5Wwqw)
- 📖 [MCP Apps - Bringing UI](https://modelcontextprotocol.github.io/ext-apps/api/)
- 📝 [MCP Apps Announcement Blog](https://blog.modelcontextprotocol.io/posts/2026-01-26-mcp-apps/)
- 🚀 [MCP Apps QuickStart](https://modelcontextprotocol.io/docs/extensions/apps)
- 💻 [Official Samples Repository](https://github.com/modelcontextprotocol/ext-apps)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

## 👤 Author

**El Bruno** - [@elbruno](https://github.com/elbruno)
