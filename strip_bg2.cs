using System;
using System.Drawing;

public class Program2 {
    public static void Main() {
        try {
            string path = @"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\cryptominer_sprite_1772827344346.png";
            Bitmap bmp = new Bitmap(path);
            Bitmap final = new Bitmap(bmp.Width, bmp.Height);
            
            for(int y = 0; y < bmp.Height; y++) {
                for(int x = 0; x < bmp.Width; x++) {
                    Color c = bmp.GetPixel(x,y);
                    // Clear white/near-white pixels
                    if (c.R > 220 && c.G > 220 && c.B > 220) {
                        final.SetPixel(x, y, Color.Transparent);
                    } else {
                        final.SetPixel(x, y, c);
                    }
                }
            }
            bmp.Dispose();
            final.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            final.Dispose();
            Console.WriteLine("SUCCESS: Stripped white background from Cryptominer PNG.");
        } catch (Exception e) {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}
