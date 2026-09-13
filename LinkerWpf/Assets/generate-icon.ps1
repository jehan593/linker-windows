Add-Type -AssemblyName System.Drawing

# Nord0 background, nord9 ring glyph — matches app/src/main/res/drawable/ic_launcher_foreground.xml
$bgColor = [System.Drawing.Color]::FromArgb(255, 0x2E, 0x34, 0x40)   # nord0
$fgColor = [System.Drawing.Color]::FromArgb(255, 0x81, 0xA1, 0xC1)  # nord9

function New-RoundedRectPath([single]$x, [single]$y, [single]$w, [single]$h, [single]$radius) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $radius * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-IconBitmap([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    # Rounded-square background (~22% corner radius, matching dnsw's squircle icon) instead of a
    # hard-cornered square — the canvas outside it stays fully transparent so the taskbar/Explorer's
    # own background shows through the corners.
    $bgBrush = New-Object System.Drawing.SolidBrush $bgColor
    $bgPath = New-RoundedRectPath 0 0 $size $size ($size * 0.22)
    $g.FillPath($bgBrush, $bgPath)
    $bgBrush.Dispose()
    $bgPath.Dispose()

    # Source viewport is 108x108 (Android adaptive icon foreground), two interlocking rings:
    # ring 1 center (44,54) outer r=15 inner r=9 ; ring 2 center (64,54) outer r=15 inner r=9
    $scale = $size / 108.0
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.FillMode = [System.Drawing.Drawing2D.FillMode]::Alternate

    function Add-Ring($path, $cx, $cy, $r, $scale) {
        $d = $r * 2 * $scale
        $x = ($cx - $r) * $scale
        $y = ($cy - $r) * $scale
        $path.AddEllipse($x, $y, $d, $d)
    }

    Add-Ring $path 44 54 15 $scale
    Add-Ring $path 44 54 9 $scale
    Add-Ring $path 64 54 15 $scale
    Add-Ring $path 64 54 9 $scale

    $brush = New-Object System.Drawing.SolidBrush $fgColor
    $g.FillPath($brush, $path)
    $g.Dispose()
    return $bmp
}

$sizes = @(16, 32, 48, 256)
$pngBytesBySize = @{}
foreach ($s in $sizes) {
    $bmp = New-IconBitmap $s
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytesBySize[$s] = $ms.ToArray()
    $bmp.Dispose()
}

# Also save a standalone 256px PNG for use as WPF app branding / about screen
[System.IO.File]::WriteAllBytes("$PSScriptRoot\icon-256.png", $pngBytesBySize[256])

# Hand-roll an .ico container (ICONDIR + ICONDIRENTRY[] + PNG payloads), all sizes PNG-compressed
# (supported by Windows Vista+ for any declared size, not just 256).
$out = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter $out

$bw.Write([UInt16]0)      # reserved
$bw.Write([UInt16]1)      # type = icon
$bw.Write([UInt16]$sizes.Count)

$headerSize = 6 + (16 * $sizes.Count)
$offset = $headerSize
foreach ($s in $sizes) {
    $data = $pngBytesBySize[$s]
    $dim = if ($s -ge 256) { 0 } else { $s }  # 0 means 256 in ICO format
    $bw.Write([Byte]$dim)      # width
    $bw.Write([Byte]$dim)      # height
    $bw.Write([Byte]0)         # color palette
    $bw.Write([Byte]0)         # reserved
    $bw.Write([UInt16]1)       # color planes
    $bw.Write([UInt16]32)      # bits per pixel
    $bw.Write([UInt32]$data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($s in $sizes) {
    $bw.Write($pngBytesBySize[$s])
}
$bw.Flush()
[System.IO.File]::WriteAllBytes("$PSScriptRoot\app.ico", $out.ToArray())
Write-Output "Wrote $PSScriptRoot\app.ico and icon-256.png"
