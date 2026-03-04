Add-Type -AssemblyName System.Drawing
$files = Get-ChildItem "c:\CODE\IA\Unity\Money-Survivor\Assets\ReadmeImages\*.png"
foreach ($f in $files) {
    if ($f.Name -match "splash" -or $f.Name -match "scrolling_background") { continue }
    $img = [System.Drawing.Image]::FromFile($f.FullName)
    $bmp = new-object System.Drawing.Bitmap($img)
    $img.Dispose()
    $color = $bmp.GetPixel(0, $bmp.Height - 1)
    $bmp.MakeTransparent($color)
    $bmp.Save($f.FullName, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Processed $($f.Name)"
}
