using System;
using System.Drawing;

public class Program {
    public static void Main() {
        try {
            string path = @"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\cryptominer_sprite_1772827344346.png";
            Bitmap bmp = new Bitmap(path);
            Bitmap final = new Bitmap(bmp.Width, bmp.Height);
            
            Color corner = bmp.GetPixel(0, 0);
            
            for(int y = 0; y < bmp.Height; y++) {
                for(int x = 0; x < bmp.Width; x++) {
                    Color c = bmp.GetPixel(x,y);
                    // DALL-E sometimes returns off-white (253,254,255 etc) 
                    float dist = Math.Abs(c.R - 255) + Math.Abs(c.G - 255) + Math.Abs(c.B - 255);
                    if (dist < 30) {
                        final.SetPixel(x, y, Color.Transparent);
                    } else {
                        final.SetPixel(x, y, c);
                    }
                }
            }
            bmp.Dispose();
            final.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            final.Dispose();
            Console.WriteLine("Successfully stripped white background from Cryptominer PNG.");
        } catch (Exception e) {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}
