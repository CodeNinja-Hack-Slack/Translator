Add-Type -AssemblyName System.Drawing

$sizes = @(32, 16)
$images = @{}

foreach ($size in $sizes) {
    $bitmap = [System.Drawing.Bitmap]::new($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = 'HighQuality'
    $g.TextRenderingHint = 'AntiAlias'
    $g.InterpolationMode = 'HighQualityBicubic'

    $s = [float]$size
    $r = $s * 0.22

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $path.AddArc(0, 0, $r*2, $r*2, 180, 90)
    $path.AddArc($s-$r*2, 0, $r*2, $r*2, 270, 90)
    $path.AddArc($s-$r*2, $s-$r*2, $r*2, $r*2, 0, 90)
    $path.AddArc(0, $s-$r*2, $r*2, $r*2, 90, 90)
    $path.CloseFigure()

    $bgBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.PointF]::new(0, 0), [System.Drawing.PointF]::new($s*0.7, $s),
        [System.Drawing.Color]::FromArgb(0, 130, 230),
        [System.Drawing.Color]::FromArgb(0, 60, 160))
    $g.FillPath($bgBrush, $path)
    $bgBrush.Dispose()

    $hlBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.PointF]::new(0, 0), [System.Drawing.PointF]::new(0, $s*0.45),
        [System.Drawing.Color]::FromArgb(50, 255, 255, 255),
        [System.Drawing.Color]::FromArgb(0, 255, 255, 255))
    $g.SetClip($path)
    $g.FillRectangle($hlBrush, 0, 0, $s, $s*0.45)
    $g.ResetClip()
    $hlBrush.Dispose()
    $path.Dispose()

    $fontSize = $s * 0.58
    $font = [System.Drawing.Font]::new("Microsoft YaHei", $fontSize, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $textBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
    $format = [System.Drawing.StringFormat]::new()
    $format.Alignment = 'Center'
    $format.LineAlignment = 'Center'
    $g.DrawString("译", $font, $textBrush, [System.Drawing.RectangleF]::new(-0.5, 0.5, $s, $s), $format)
    $font.Dispose()
    $textBrush.Dispose()
    $format.Dispose()
    $g.Dispose()
    $images[$size] = $bitmap
}

$outputPath = Join-Path $PSScriptRoot "TranslatorApp\app.ico"
$fs = [System.IO.File]::Open($outputPath, [System.IO.FileMode]::Create)
$writer = [System.IO.BinaryWriter]::new($fs)

$writer.Write([UInt16]0)        # reserved
$writer.Write([UInt16]1)        # ICO type
$writer.Write([UInt16]$sizes.Count)  # image count

$dirEntries = @()
$imageData = @()
$offset = 6 + $sizes.Count * 16

foreach ($size in $sizes) {
    $bitmap = $images[$size]
    $rect = [System.Drawing.Rectangle]::new(0, 0, $size, $size)
    $bmpData = $bitmap.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $stride = $bmpData.Stride
    $scan0 = $bmpData.Scan0
    $pixelBytes = [byte[]]::new($stride * $size)
    [System.Runtime.InteropServices.Marshal]::Copy($scan0, $pixelBytes, 0, $pixelBytes.Length)
    $bitmap.UnlockBits($bmpData)

    # Convert BGRA to bottom-up order for ICO
    $pixels = [byte[]]::new($size * $size * 4)
    for ($y = 0; $y -lt $size; $y++) {
        for ($x = 0; $x -lt $size; $x++) {
            $srcIdx = $y * $stride + $x * 4
            $dstIdx = ($size - 1 - $y) * $size * 4 + $x * 4
            $pixels[$dstIdx]   = $pixelBytes[$srcIdx]     # B
            $pixels[$dstIdx+1] = $pixelBytes[$srcIdx+1]   # G
            $pixels[$dstIdx+2] = $pixelBytes[$srcIdx+2]   # R
            $pixels[$dstIdx+3] = $pixelBytes[$srcIdx+3]   # A
        }
    }

    $bitmap.Dispose()

    $andMaskSize = [int]($size * $size / 8)
    $andMask = [byte[]]::new($andMaskSize)
    $orMask  = [byte[]]::new($andMaskSize)

    $bmpInfoHeaderStream = [System.IO.MemoryStream]::new()
    $bmpWriter = [System.IO.BinaryWriter]::new($bmpInfoHeaderStream)
    $bmpWriter.Write([UInt32]40)               # biSize
    $bmpWriter.Write([Int32]$size)             # biWidth
    $bmpWriter.Write([Int32]($size * 2))       # biHeight (XOR + AND)
    $bmpWriter.Write([UInt16]1)                # biPlanes
    $bmpWriter.Write([UInt16]32)               # biBitCount
    $bmpWriter.Write([UInt32]0)                # biCompression (BI_RGB)
    $bmpWriter.Write([UInt32]0)                # biSizeImage
    $bmpWriter.Write([Int32]0)                 # biXPelsPerMeter
    $bmpWriter.Write([Int32]0)                 # biYPelsPerMeter
    $bmpWriter.Write([UInt32]0)                # biClrUsed
    $bmpWriter.Write([UInt32]0)                # biClrImportant
    $bmpWriter.Flush()

    $bmpInfoHeader = $bmpInfoHeaderStream.ToArray()
    $bmpWriter.Dispose()
    $bmpInfoHeaderStream.Dispose()

    $imageStream = [System.IO.MemoryStream]::new()
    $imageStream.Write($bmpInfoHeader, 0, $bmpInfoHeader.Length)
    $imageStream.Write($pixels, 0, $pixels.Length)
    $imageStream.Write($andMask, 0, $andMask.Length)
    $imageStream.Write($orMask, 0, $orMask.Length)
    $imageBytes = $imageStream.ToArray()
    $imageStream.Dispose()

    $imageData += @{data = $imageBytes; offset = $offset}
    $offset += $imageBytes.Length

    $dirEntryStream = [System.IO.MemoryStream]::new()
    $dirWriter = [System.IO.BinaryWriter]::new($dirEntryStream)
    $w = if ($size -eq 256) { 0 } else { $size }
    $h = if ($size -eq 256) { 0 } else { $size }
    $dirWriter.Write([byte]$w)               # Width
    $dirWriter.Write([byte]$h)               # Height
    $dirWriter.Write([byte]0)                # ColorCount
    $dirWriter.Write([byte]0)                # Reserved
    $dirWriter.Write([UInt16]1)              # Planes
    $dirWriter.Write([UInt16]32)             # BitCount
    $dirWriter.Write([UInt32]$imageBytes.Length)  # ImageSize
    $dirWriter.Write([UInt32]$offset - $imageBytes.Length)  # ImageOffset
    $dirWriter.Flush()

    $dirEntries += $dirEntryStream.ToArray()
    $dirWriter.Dispose()
    $dirEntryStream.Dispose()
}

# Write directory entries
foreach ($entry in $dirEntries) {
    $writer.Write($entry, 0, $entry.Length)
}

# Write image data
foreach ($img in $imageData) {
    $writer.Write($img.data, 0, $img.data.Length)
}

$writer.Flush()
$totalBytes = $fs.Length
$writer.Dispose()
$fs.Dispose()

Write-Host "app.ico generated: $totalBytes bytes ($($sizes.Count) images)"
