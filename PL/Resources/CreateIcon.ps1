Add-Type -AssemblyName System.Drawing

$bmp = New-Object System.Drawing.Bitmap(64, 64)
$graphics = [System.Drawing.Graphics]::FromImage($bmp)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)

# Draw truck body (orange)
$orangeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 87, 34))
$graphics.FillRectangle($orangeBrush, 15, 25, 35, 22)

# Draw cabin (darker orange)
$darkOrangeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(216, 67, 21))
$cabinPoints = @(
    [System.Drawing.Point]::new(15, 25),
    [System.Drawing.Point]::new(15, 15),
    [System.Drawing.Point]::new(28, 15),
    [System.Drawing.Point]::new(32, 25)
)
$graphics.FillPolygon($darkOrangeBrush, $cabinPoints)

# Draw window (light blue)
$windowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(227, 242, 253))
$graphics.FillRectangle($windowBrush, 18, 18, 8, 5)

# Draw wheels (dark gray)
$wheelBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(66, 66, 66))
$graphics.FillEllipse($wheelBrush, 20, 42, 10, 10)
$graphics.FillEllipse($wheelBrush, 40, 42, 10, 10)

# Draw wheel centers (white)
$whiteBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$graphics.FillEllipse($whiteBrush, 23, 45, 4, 4)
$graphics.FillEllipse($whiteBrush, 43, 45, 4, 4)

# Draw package (yellow)
$packageBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 213, 79))
$graphics.FillRectangle($packageBrush, 35, 28, 10, 10)

# Draw package tape (orange)
$tapeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 167, 38))
$graphics.FillRectangle($tapeBrush, 39, 28, 2, 10)

# Save as PNG
$bmp.Save("$PSScriptRoot\delivery_icon.png", [System.Drawing.Imaging.ImageFormat]::Png)

# Convert to ICO
$icon = [System.Drawing.Icon]::FromHandle($bmp.GetHicon())
$stream = New-Object System.IO.FileStream("$PSScriptRoot\delivery_icon.ico", [System.IO.FileMode]::Create)
$icon.Save($stream)
$stream.Close()

$graphics.Dispose()
$bmp.Dispose()
$icon.Dispose()

Write-Host "Icon files created successfully at $PSScriptRoot"
