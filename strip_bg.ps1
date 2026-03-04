Add-Type -AssemblyName System.Drawing
$sprites = @("player_sprite.png", "enemy_sprite.png", "coin_sprite.png", "boomerang_sprite.png", "shield_sprite.png")
foreach ($s in $sprites) {
    $path = "c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\$s"
    if (Test-Path $path) {
        $img = [System.Drawing.Image]::FromFile($path)
        $bmp = new-object System.Drawing.Bitmap($img)
        $img.Dispose()
        $color = $bmp.GetPixel(0, $bmp.Height - 1)
        $bmp.MakeTransparent($color)
        $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        $bmp.Dispose()
        Write-Host "Processed $s"
    }
}
