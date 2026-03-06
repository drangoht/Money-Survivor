using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

public class ProgramBgFixerNew {
    public static void ProcessImage(string path, Color targetBgColor) {
        try {
            Bitmap bmp = new Bitmap(path);
            Bitmap final = new Bitmap(bmp.Width, bmp.Height);
            
            Color bgKey = bmp.GetPixel(0, 0); // Corner pixel
            float thresh = 80f; 

            bool[,] visited = new bool[bmp.Width, bmp.Height];
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(0, 0));
            q.Enqueue(new Point(bmp.Width - 1, 0));
            q.Enqueue(new Point(0, bmp.Height - 1));
            q.Enqueue(new Point(bmp.Width - 1, bmp.Height - 1));

            // Copy over the actual non-background pixels
            for(int y = 0; y < bmp.Height; y++) {
                for(int x = 0; x < bmp.Width; x++) {
                    final.SetPixel(x, y, bmp.GetPixel(x,y));
                }
            }

            // Flood fill the new target background color
            while(q.Count > 0) {
                Point p = q.Dequeue();
                if (p.X < 0 || p.X >= bmp.Width || p.Y < 0 || p.Y >= bmp.Height) continue;
                if (visited[p.X, p.Y]) continue;
                
                Color c = bmp.GetPixel(p.X, p.Y);
                bool isBg = c.A < 10 || 
                           (Math.Abs(c.R - bgKey.R) + Math.Abs(c.G - bgKey.G) + Math.Abs(c.B - bgKey.B) < thresh);

                if (isBg) {
                    visited[p.X, p.Y] = true;
                    final.SetPixel(p.X, p.Y, targetBgColor); // Apply the exact XP Orb background color
                    
                    q.Enqueue(new Point(p.X - 1, p.Y));
                    q.Enqueue(new Point(p.X + 1, p.Y));
                    q.Enqueue(new Point(p.X, p.Y - 1));
                    q.Enqueue(new Point(p.X, p.Y + 1));
                }
            }
            
            // Dispose file lock and save via temp file
            bmp.Dispose();
            string tempPath = path + ".tmp";
            final.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            final.Dispose();
            
            File.Delete(path);
            File.Move(tempPath, path);
            Console.WriteLine("Painted matching background on: " + path);
        } catch (Exception e) {
            Console.WriteLine("Error on " + path + ": " + e.Message);
        }
    }

    public static void Main() {
        try {
            string orbPath = @"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\xp_orb_sprite.png";
            Bitmap orbBmp = new Bitmap(orbPath);
            Color exactBgColor = orbBmp.GetPixel(0, 0); 
            orbBmp.Dispose();
            Console.WriteLine("Sampled XP Orb Background Color: " + exactBgColor);

            ProcessImage(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\stock_options_sprite_1772827497492.png", exactBgColor);
        } catch (Exception e) {
             Console.WriteLine("Main Error: " + e.Message);
        }
    }
}
