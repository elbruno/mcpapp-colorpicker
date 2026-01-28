using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:3001");

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly();

var app = builder.Build();

// MCP endpoint - map to /mcp path
app.MapMcp("/mcp");

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

/// <summary>
/// HTML content provider for the color picker UI
/// </summary>
public static class ColorPickerHtmlProvider
{
  public static Task<string> GetHtml()
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
    .container { max-width: 800px; margin: 0 auto; }
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
      position: relative;
      overflow: hidden;
    }
    .preview-info {
      position: relative;
      z-index: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 4px;
    }
    .preview-hex { font-size: 18px; font-weight: 700; }
    .preview-rgb { font-size: 11px; opacity: 0.9; }
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
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .filter-info {
      font-size: 11px;
      font-weight: 400;
      color: var(--vscode-descriptionForeground, #666);
      font-style: italic;
    }
    
    /* Gradient Palette Styles */
    .gradient-palette-container {
      margin-bottom: 20px;
      position: relative;
    }
    .gradient-palette {
      display: grid;
      grid-template-columns: repeat(20, 1fr);
      gap: 3px;
      margin-bottom: 16px;
    }
    .gradient-color {
      aspect-ratio: 1;
      border-radius: 3px;
      cursor: crosshair;
      transition: transform 0.1s, box-shadow 0.1s;
      border: 1px solid rgba(255,255,255,0.1);
    }
    .gradient-color:hover {
      transform: scale(1.3);
      box-shadow: 0 4px 12px rgba(0,0,0,0.5);
      z-index: 10;
      border: 2px solid var(--vscode-focusBorder, #007fd4);
    }
    .gradient-color.selected {
      border: 2px solid #fff;
      box-shadow: 0 0 8px rgba(255,255,255,0.8);
    }
    
    /* Hue Strip */
    .hue-strip {
      height: 30px;
      border-radius: 6px;
      background: linear-gradient(to right, 
        #ff0000 0%, #ffff00 17%, #00ff00 33%, 
        #00ffff 50%, #0000ff 67%, #ff00ff 83%, #ff0000 100%);
      cursor: crosshair;
      position: relative;
      margin-bottom: 16px;
      border: 2px solid var(--vscode-input-border, #3c3c3c);
    }
    .hue-indicator {
      position: absolute;
      top: -4px;
      width: 4px;
      height: calc(100% + 8px);
      background: white;
      border: 2px solid black;
      pointer-events: none;
      transform: translateX(-50%);
    }
    
    .palette { display: grid; grid-template-columns: repeat(10, 1fr); gap: 6px; }
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
    <h1>🎨 Advanced Color Picker</h1>
    
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
    
    <div class="preview" id="preview">
      <div class="preview-info">
        <span class="preview-hex" id="preview-hex">#3498DB</span>
        <span class="preview-rgb" id="preview-rgb">rgb(52, 152, 219)</span>
      </div>
    </div>
    
    <div class="actions">
      <button class="btn btn-secondary" id="random-btn">🎲 Random</button>
      <button class="btn btn-primary" id="select-btn">✓ Use This Color</button>
    </div>
    
    <div class="section-title">Hue Selector <span class="filter-info">- Click to select base hue</span></div>
    <div class="hue-strip" id="hue-strip">
      <div class="hue-indicator" id="hue-indicator"></div>
    </div>
    
    <div class="section-title">Gradient Palette <span class="filter-info">- Hover to preview, click to select</span></div>
    <div class="gradient-palette-container">
      <div class="gradient-palette" id="gradient-palette"></div>
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
    
    function rgbToHex(r, g, b) {
      return '#' + [r, g, b].map(x => {
        const hex = x.toString(16);
        return hex.length === 1 ? '0' + hex : hex;
      }).join('').toUpperCase();
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
    
    function hslToRgb(h, s, l) {
      h /= 360;
      s /= 100;
      l /= 100;
      let r, g, b;
      if (s === 0) {
        r = g = b = l;
      } else {
        const hue2rgb = (p, q, t) => {
          if (t < 0) t += 1;
          if (t > 1) t -= 1;
          if (t < 1/6) return p + (q - p) * 6 * t;
          if (t < 1/2) return q;
          if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
          return p;
        };
        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;
        r = hue2rgb(p, q, h + 1/3);
        g = hue2rgb(p, q, h);
        b = hue2rgb(p, q, h - 1/3);
      }
      return {
        r: Math.round(r * 255),
        g: Math.round(g * 255),
        b: Math.round(b * 255)
      };
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
    const previewHex = document.getElementById('preview-hex');
    const previewRgb = document.getElementById('preview-rgb');
    const gradientPalette = document.getElementById('gradient-palette');
    const hueStrip = document.getElementById('hue-strip');
    const hueIndicator = document.getElementById('hue-indicator');
    const palette = document.getElementById('palette');
    const status = document.getElementById('status');
    const randomBtn = document.getElementById('random-btn');
    const selectBtn = document.getElementById('select-btn');
    
    // Current state
    let currentHue = 210; // Start with blue hue
    
    // Build gradient palette based on current hue
    function buildGradientPalette(hue) {
      gradientPalette.innerHTML = '';
      
      // Create a 20x10 grid (200 colors)
      // Rows: Lightness from 95% to 5%
      // Columns: Saturation from 0% to 100%
      for (let row = 0; row < 10; row++) {
        for (let col = 0; col < 20; col++) {
          const lightness = 95 - (row * 10);
          const saturation = col * 5;
          
          const { r, g, b } = hslToRgb(hue, saturation, lightness);
          const hexColor = rgbToHex(r, g, b);
          
          const div = document.createElement('div');
          div.className = 'gradient-color';
          div.style.backgroundColor = hexColor;
          div.dataset.color = hexColor;
          
          // Hover to preview
          div.addEventListener('mouseenter', () => {
            updateColor(hexColor, false);
          });
          
          // Click to select
          div.addEventListener('click', () => {
            updateColor(hexColor, true);
            document.querySelectorAll('.gradient-color').forEach(el => el.classList.remove('selected'));
            div.classList.add('selected');
            status.textContent = `✓ Selected from gradient: ${hexColor}`;
            status.className = 'status success';
          });
          
          gradientPalette.appendChild(div);
        }
      }
    }
    
    // Hue strip interaction
    hueStrip.addEventListener('click', (e) => {
      const rect = hueStrip.getBoundingClientRect();
      const x = e.clientX - rect.left;
      const percentage = x / rect.width;
      currentHue = Math.round(percentage * 360);
      
      updateHueIndicator(percentage);
      buildGradientPalette(currentHue);
    });
    
    hueStrip.addEventListener('mousemove', (e) => {
      if (e.buttons === 1) { // If mouse button is pressed
        const rect = hueStrip.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const percentage = Math.max(0, Math.min(1, x / rect.width));
        currentHue = Math.round(percentage * 360);
        
        updateHueIndicator(percentage);
        buildGradientPalette(currentHue);
      }
    });
    
    function updateHueIndicator(percentage) {
      hueIndicator.style.left = `${percentage * 100}%`;
    }
    
    // Quick palette colors
    const paletteColors = [
      '#FF0000', '#FF4500', '#FF8C00', '#FFD700', '#FFFF00',
      '#9ACD32', '#00FF00', '#00FA9A', '#00CED1', '#00BFFF',
      '#0000FF', '#4169E1', '#8A2BE2', '#9370DB', '#FF00FF',
      '#FF1493', '#FF69B4', '#FFC0CB', '#FFFFFF', '#D3D3D3',
      '#A9A9A9', '#808080', '#696969', '#000000', '#8B4513'
    ];
    
    // Build quick palette
    paletteColors.forEach(color => {
      const div = document.createElement('div');
      div.className = 'palette-color';
      div.style.backgroundColor = color;
      div.onclick = () => {
        updateColor(color, true);
        status.textContent = `✓ Selected: ${color}`;
        status.className = 'status success';
      };
      palette.appendChild(div);
    });
    
    function updateColor(hex, permanent = true) {
      hex = hex.toUpperCase();
      
      if (permanent) {
        colorInput.value = hex;
        hexValue.value = hex;
      }
      
      const { r, g, b } = hexToRgb(hex);
      const rgbStr = `rgb(${r}, ${g}, ${b})`;
      
      if (permanent) {
        rgbValue.value = rgbStr;
      }
      
      const { h, s, l } = rgbToHsl(r, g, b);
      const hslStr = `hsl(${h}, ${s}%, ${l}%)`;
      
      if (permanent) {
        hslValue.value = hslStr;
      }
      
      preview.style.backgroundColor = hex;
      const contrastColor = getContrastColor(hex);
      preview.style.color = contrastColor;
      previewHex.textContent = hex;
      previewRgb.textContent = rgbStr;
      
      if (!permanent) {
        status.textContent = `Hovering: ${hex}`;
        status.className = 'status';
      }
    }
    
    // Event listeners
    colorInput.addEventListener('input', (e) => {
      updateColor(e.target.value, true);
      status.textContent = '';
      status.className = 'status';
    });
    
    hexValue.addEventListener('input', (e) => {
      let val = e.target.value;
      if (/^#[0-9A-Fa-f]{6}$/.test(val)) {
        updateColor(val, true);
      }
    });
    
    randomBtn.addEventListener('click', () => {
      const newColor = randomColor();
      updateColor(newColor, true);
      status.textContent = `🎲 Random color: ${newColor}`;
      status.className = 'status success';
    });
    
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
    updateColor('#3498DB', true);
    buildGradientPalette(currentHue);
    updateHueIndicator(currentHue / 360);
    
    // Listen for initial color from host
    window.addEventListener('message', (event) => {
      if (event.data?.initialColor) {
        updateColor(event.data.initialColor, true);
      }
    });
  </script>
</body>
</html>
""";
    return Task.FromResult(html);
  }
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
  [McpServerTool]
  [Description("Open an interactive color picker to select a color visually. Returns an HTML UI that allows the user to pick colors interactively.")]
  [McpMeta("ui", JsonValue = """{ "resourceUri": "ui://color-picker/app.html" }""")]
  public static ColorPickerResult ColorPicker(
      [Description("Initial color to display (hex format like #FF5733). Default: #3498DB")]
        string? initialColor = "#3498DB")
  {
    return new ColorPickerResult
    {
      InitialColor = initialColor ?? "#3498DB",
      Message = "Opening color picker UI..."
    };
  }
}

/// <summary>
/// Color Picker MCP Resources
/// </summary>
[McpServerResourceType]
public static class ColorPickerResources
{
  /// <summary>
  /// Provides the HTML UI for the color picker app
  /// </summary>
  [McpServerResource(
      UriTemplate = "ui://color-picker/app.html",
      MimeType = "text/html",
      Title = "Color Picker UI")]
  [Description("Interactive color picker UI")]
  public static async Task<string> GetColorPickerUI()
  {
    return await ColorPickerHtmlProvider.GetHtml();
  }
}

public class ColorPickerResult
{
  public string InitialColor { get; set; } = "#3498DB";
  public string Message { get; set; } = "";

  public override string ToString() =>
      $"Color Picker ready. Initial color: {InitialColor}";
}
