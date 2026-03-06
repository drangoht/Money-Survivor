using System;
using System.Drawing;
using System.IO;

public class ProgramTransparentAggressive2 {
    public static void ProcessImage(string path) {
        try {
            Bitmap bmp = new Bitmap(path);
            Bitmap final = new Bitmap(bmp.Width, bmp.Height);
            
            // We previously painted the edges orbColor, so the corner is orbColor
            Color corner = bmp.GetPixel(0, 0);

            for(int y = 0; y < bmp.Height; y++) {
                for(int x = 0; x < bmp.Width; x++) {
                    Color c = bmp.GetPixel(x,y);
                    
                    bool isCorner = (Math.Abs(c.R - corner.R) + Math.Abs(c.G - corner.G) + Math.Abs(c.B - corner.B) < 40);
                    // aggressively kill any remaining pure white background islands
                    bool isWhiteBg = (c.R > 240 && c.G > 240 && c.B > 240);
                    // kill any leftover magenta from our earlier experiment
                    bool isMagenta = (c.R > 240 && c.G < 20 && c.B > 240);
                    
                    if (isCorner || isWhiteBg || isMagenta) {
                        final.SetPixel(x, y, Color.Transparent); // Force completely invisible
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
            Console.WriteLine("Global transparency applied: " + path);
        } catch (Exception e) {
            Console.WriteLine("Error on " + path + ": " + e.Message);
        }
    }

    public static void Main() {
        try {
            ProcessImage(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\cryptominer_sprite_1772827344346.png");
            ProcessImage(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\stock_options_sprite_1772827497492.png");
        } catch (Exception e) {
             Console.WriteLine("Main Error: " + e.Message);
        }
    }
}
