using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:3001");

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// MCP endpoint - map to /mcp path
app.MapMcp("/mcp");

// Serve the color picker UI HTML for the MCP App View
app.MapGet("/ui/color-picker", async () =>
{
    var html = await GetColorPickerHtml();
    return Results.Content(html, "text/html");
});

// API endpoint to get the color picker UI as MCP resource
app.MapGet("/mcp/resources/ui/color-picker", async () =>
{
    var html = await GetColorPickerHtml();
    return Results.Json(new
    {
        contents = new[]
        {
            new
            {
                uri = "ui://color-picker/app.html",
                mimeType = "text/html",
                text = html
            }
        }
    });
});

Console.WriteLine();
Console.WriteLine("🎨 Color Picker MCP App (C#)");
Console.WriteLine("============================");
Console.WriteLine("MCP server listening on http://localhost:3001/mcp");
Console.WriteLine();
Console.WriteLine("Add to your VS Code MCP config:");
Console.WriteLine("  \"url\": \"http://localhost:3001/mcp\"");
Console.WriteLine("  \"type\": \"http\"");
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to stop the server");
Console.WriteLine();

await app.RunAsync();

static Task<string> GetColorPickerHtml()
{
    var html = """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Color Picker</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      padding: 16px;
      min-height: 100vh;
      background: var(--vscode-editor-background, #1e1e1e);
      color: var(--vscode-editor-foreground, #cccccc);
    }
    .container { max-width: 400px; margin: 0 auto; }
    h1 { font-size: 18px; margin-bottom: 16px; text-align: center; }
    .picker-row { display: flex; gap: 12px; align-items: center; margin-bottom: 16px; }
    #color-input {
      width: 80px; height: 80px; border: none; border-radius: 8px;
      cursor: pointer; background: none; padding: 0;
    }
    #color-input::-webkit-color-swatch-wrapper { padding: 0; }
    #color-input::-webkit-color-swatch {
      border: 2px solid var(--vscode-input-border, #3c3c3c);
      border-radius: 8px;
    }
    .color-values { flex: 1; display: flex; flex-direction: column; gap: 8px; }
    .value-row { display: flex; align-items: center; gap: 8px; }
    .value-label {
      font-size: 12px; font-weight: 600; width: 35px;
      color: var(--vscode-descriptionForeground, #888);
    }
    .value-input {
      flex: 1; padding: 6px 10px;
      border: 1px solid var(--vscode-input-border, #3c3c3c);
      border-radius: 4px;
      background: var(--vscode-input-background, #3c3c3c);
      color: var(--vscode-input-foreground, #cccccc);
      font-family: 'Consolas', 'Monaco', monospace; font-size: 13px;
    }
    .value-input:focus {
      outline: none;
      border-color: var(--vscode-focusBorder, #007fd4);
    }
    .copy-btn {
      padding: 6px 10px; border: none; border-radius: 4px;
      background: var(--vscode-button-secondaryBackground, #3c3c3c);
      color: var(--vscode-button-secondaryForeground, #cccccc);
      cursor: pointer; font-size: 12px; transition: background 0.2s;
    }
    .copy-btn:hover {
      background: var(--vscode-button-secondaryHoverBackground, #505050);
    }
    .preview {
      height: 60px; border-radius: 8px; margin-bottom: 16px;
      border: 2px solid var(--vscode-input-border, #3c3c3c);
      display: flex; align-items: center; justify-content: center;
      font-size: 14px; font-weight: 600;
      text-shadow: 0 1px 2px rgba(0,0,0,0.5);
    }
    .actions { display: flex; gap: 8px; margin-bottom: 16px; }
    .btn {
      flex: 1; padding: 10px 16px; border: none; border-radius: 6px;
      font-size: 14px; font-weight: 500; cursor: pointer; transition: background 0.2s;
    }
    .btn-primary {
      background: var(--vscode-button-background, #0e639c);
      color: var(--vscode-button-foreground, #ffffff);
    }
    .btn-primary:hover {
      background: var(--vscode-button-hoverBackground, #1177bb);
    }
    .btn-secondary {
      background: var(--vscode-button-secondaryBackground, #3c3c3c);
      color: var(--vscode-button-secondaryForeground, #cccccc);
    }
    .btn-secondary:hover {
      background: var(--vscode-button-secondaryHoverBackground, #505050);
    }
    .section-title {
      font-size: 12px; font-weight: 600; margin-bottom: 8px;
      color: var(--vscode-descriptionForeground, #888);
    }
    .palette { display: grid; grid-template-columns: repeat(8, 1fr); gap: 6px; }
    .palette-color {
      aspect-ratio: 1; border-radius: 4px; border: 2px solid transparent;
      cursor: pointer; transition: transform 0.1s, border-color 0.2s;
    }
    .palette-color:hover {
      transform: scale(1.15);
      border-color: var(--vscode-focusBorder, #007fd4);
    }
    .status {
      text-align: center; font-size: 12px; margin-top: 12px; min-height: 18px;
      color: var(--vscode-descriptionForeground, #888);
    }
    .status.success { color: var(--vscode-terminal-ansiGreen, #4ec94e); }
  </style>
</head>
<body>
  <div class="container">
    <h1>🎨 Color Picker</h1>
    
    <div class="picker-row">
      <input type="color" id="color-input" value="#3498DB" />
      <div class="color-values">
        <div class="value-row">
          <span class="value-label">HEX</span>
          <input type="text" id="hex-value" class="value-input" value="#3498DB" />
          <button class="copy-btn" data-format="hex">Copy</button>
        </div>
        <div class="value-row">
          <span class="value-label">RGB</span>
          <input type="text" id="rgb-value" class="value-input" readonly />
          <button class="copy-btn" data-format="rgb">Copy</button>
        </div>
        <div class="value-row">
          <span class="value-label">HSL</span>
          <input type="text" id="hsl-value" class="value-input" readonly />
          <button class="copy-btn" data-format="hsl">Copy</button>
        </div>
      </div>
    </div>
    
    <div class="preview" id="preview">Selected Color</div>
    
    <div class="actions">
      <button class="btn btn-secondary" id="random-btn">🎲 Random</button>
      <button class="btn btn-primary" id="select-btn">✓ Use This Color</button>
    </div>
    
    <div class="section-title">Quick Colors</div>
    <div class="palette" id="palette"></div>
    
    <div class="status" id="status"></div>
  </div>
  
  <script type="module">
    // Color conversion utilities
    function hexToRgb(hex) {
      const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
      return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
      } : { r: 0, g: 0, b: 0 };
    }
    
    function rgbToHsl(r, g, b) {
      r /= 255; g /= 255; b /= 255;
      const max = Math.max(r, g, b), min = Math.min(r, g, b);
      let h, s, l = (max + min) / 2;
      if (max === min) {
        h = s = 0;
      } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
          case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
          case g: h = ((b - r) / d + 2) / 6; break;
          case b: h = ((r - g) / d + 4) / 6; break;
        }
      }
      return { h: Math.round(h * 360), s: Math.round(s * 100), l: Math.round(l * 100) };
    }
    
    function getContrastColor(hex) {
      const { r, g, b } = hexToRgb(hex);
      const brightness = (r * 299 + g * 587 + b * 114) / 1000;
      return brightness > 128 ? '#000000' : '#ffffff';
    }
    
    function randomColor() {
      return '#' + Math.floor(Math.random()*16777215).toString(16).padStart(6, '0');
    }
    
    // Elements
    const colorInput = document.getElementById('color-input');
    const hexValue = document.getElementById('hex-value');
    const rgbValue = document.getElementById('rgb-value');
    const hslValue = document.getElementById('hsl-value');
    const preview = document.getElementById('preview');
    const palette = document.getElementById('palette');
    const status = document.getElementById('status');
    const randomBtn = document.getElementById('random-btn');
    const selectBtn = document.getElementById('select-btn');
    
    // Palette colors
    const paletteColors = [
      '#FF6B6B', '#FF8E72', '#FFA94D', '#FFD43B', '#A9E34B', '#69DB7C', '#38D9A9', '#3BC9DB',
      '#4DABF7', '#748FFC', '#9775FA', '#DA77F2', '#F783AC', '#E64980', '#C92A2A', '#E67700',
      '#2F9E44', '#1971C2', '#6741D9', '#9C36B5', '#FFFFFF', '#ADB5BD', '#868E96', '#495057'
    ];
    
    // Build palette
    paletteColors.forEach(color => {
      const div = document.createElement('div');
      div.className = 'palette-color';
      div.style.backgroundColor = color;
      div.onclick = () => updateColor(color);
      palette.appendChild(div);
    });
    
    function updateColor(hex) {
      hex = hex.toUpperCase();
      colorInput.value = hex;
      hexValue.value = hex;
      
      const { r, g, b } = hexToRgb(hex);
      rgbValue.value = `rgb(${r}, ${g}, ${b})`;
      
      const { h, s, l } = rgbToHsl(r, g, b);
      hslValue.value = `hsl(${h}, ${s}%, ${l}%)`;
      
      preview.style.backgroundColor = hex;
      preview.style.color = getContrastColor(hex);
      preview.textContent = hex;
      
      status.textContent = '';
      status.className = 'status';
    }
    
    // Event listeners
    colorInput.addEventListener('input', (e) => updateColor(e.target.value));
    
    hexValue.addEventListener('input', (e) => {
      let val = e.target.value;
      if (/^#[0-9A-Fa-f]{6}$/.test(val)) {
        updateColor(val);
      }
    });
    
    randomBtn.addEventListener('click', () => updateColor(randomColor()));
    
    selectBtn.addEventListener('click', () => {
      const color = hexValue.value;
      // Send the selected color back to the host
      if (window.parent !== window) {
        window.parent.postMessage({ type: 'mcp-app-result', color: color }, '*');
      }
      status.textContent = `✓ Selected: ${color}`;
      status.className = 'status success';
      
      // Copy to clipboard
      navigator.clipboard.writeText(color).catch(() => {});
    });
    
    // Copy buttons
    document.querySelectorAll('.copy-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        const format = btn.dataset.format;
        const value = document.getElementById(`${format}-value`).value;
        navigator.clipboard.writeText(value).then(() => {
          status.textContent = `✓ Copied: ${value}`;
          status.className = 'status success';
        });
      });
    });
    
    // Initialize
    updateColor('#3498DB');
    
    // Listen for initial color from host
    window.addEventListener('message', (event) => {
      if (event.data?.initialColor) {
        updateColor(event.data.initialColor);
      }
    });
  </script>
</body>
</html>
""";
    return Task.FromResult(html);
}

/// <summary>
/// Color Picker MCP Tools
/// </summary>
[McpServerToolType]
public static class ColorPickerTools
{
    /// <summary>
    /// Opens an interactive color picker UI to visually select a color.
    /// </summary>
    [McpServerTool, Description("Open an interactive color picker to select a color visually. Returns an HTML UI that allows the user to pick colors interactively.")]
    public static ColorPickerResult ColorPicker(
        [Description("Initial color to display (hex format like #FF5733). Default: #3498DB")]
        string? initialColor = "#3498DB")
    {
        return new ColorPickerResult
        {
            InitialColor = initialColor ?? "#3498DB",
            UiResourceUri = "ui://color-picker/app.html",
            Message = "Opening color picker UI..."
        };
    }
}

public class ColorPickerResult
{
    public string InitialColor { get; set; } = "#3498DB";
    public string UiResourceUri { get; set; } = "";
    public string Message { get; set; } = "";

    public override string ToString() =>
        $"Color Picker ready. Initial color: {InitialColor}\nUI Resource: {UiResourceUri}";
}
