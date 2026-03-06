using System;
using System.Drawing;
using System.IO;

public class ProgramBgFillAll {
    public static void ProcessImage(string path, Color targetBgColor) {
        try {
            Bitmap bmp = new Bitmap(path);
            Bitmap final = new Bitmap(bmp.Width, bmp.Height);

            for(int y = 0; y < bmp.Height; y++) {
                for(int x = 0; x < bmp.Width; x++) {
                    Color c = bmp.GetPixel(x,y);
                    
                    // If it is fully transparent (what we did in the last script)
                    // or if it matches the current top-left corner
                    if (c.A < 10) {
                        final.SetPixel(x, y, targetBgColor);
                    } else {
                        final.SetPixel(x, y, c);
                    }
                }
            }
            
            bmp.Dispose();
            string tempPath = path + ".tmp";
            final.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            final.Dispose();
            
            File.Delete(path);
            File.Move(tempPath, path);
            Console.WriteLine("Filled transparent pixels with orb bg color: " + path);
        } catch (Exception e) {
            Console.WriteLine("Error on " + path + ": " + e.Message);
        }
    }

    public static void Main() {
        try {
            // First, sample the exact background color from xp_orb_sprite.png
            string orbPath = @"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\xp_orb_sprite.png";
            Bitmap orbBmp = new Bitmap(orbPath);
            Color exactBgColor = orbBmp.GetPixel(0, 0); 
            orbBmp.Dispose();
            Console.WriteLine("Sampled XP Orb Background Color: " + exactBgColor);

             ProcessImage(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\cryptominer_sprite_1772827344346.png", exactBgColor);
             ProcessImage(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\stock_options_sprite_1772827497492.png", exactBgColor);
        } catch (Exception e) {
             Console.WriteLine("Main Error: " + e.Message);
        }
    }
}
