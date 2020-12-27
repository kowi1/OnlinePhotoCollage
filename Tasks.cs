using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;


namespace OnlinePhotoCollage
{
    public class Tasks
    {
         public Tasks()
        {
            
        }
         public int Resize(String imageName,int count)
        {
            
            using (Image image = Image.Load(Path.Combine(Directory.GetCurrentDirectory(), @"uploads", imageName)))
            {
                image.Mutate(x => x
                     .Resize(image.Width / 2, image.Height / 2)
                     .Grayscale());

                image.Save(Path.Combine(Directory.GetCurrentDirectory(), @"uploads",(count++).ToString()+imageName)); // Automatic encoder selected based on extension.
            }
            return 0;
        }
         public int Stitch(List<string> images,int count,Tuple<int, int, int, int, string, int> OptionsArg)
        {  
            Image<Rgba32> img1;
            Image<Rgba32> img2;
            Image<Rgba32> outputImage;
            int width=150;
            int height=180;
            int border=OptionsArg.Item1;
           if(OptionsArg.Item5=="horizontal") 
           {
               outputImage = new Image<Rgba32>((width+2*border)*images.Count, height+2*border);
           }else{
               outputImage = new Image<Rgba32>(width+2*border,(height+2*border)*images.Count);
           }
           Rgba32 colorRGB= new Rgba32(Convert.ToByte(Convert.ToUInt32(OptionsArg.Item2)),Convert.ToByte(Convert.ToUInt32(OptionsArg.Item3)),Convert.ToByte(Convert.ToUInt32(OptionsArg.Item4)));
            Color color=new Color(colorRGB);
            Console.WriteLine(Convert.ToUInt32(OptionsArg.Item2)+" "+Convert.ToUInt32(OptionsArg.Item3)+" "+Convert.ToUInt32(OptionsArg.Item4));
                         
           outputImage.Mutate(i => i.Fill(color));
            
            for (int i=0;i<images.Count;i++){
                 
      
                   img1= outputImage;
                   img2 = Image.Load<Rgba32>(Path.Combine(Directory.GetCurrentDirectory(), @"uploads", images[i]));
              
 // create output image of the correct dimensions
{
    // reduce source images to correct dimensions
    // skip if already correct size
    // if you need to use source images else where use Clone and take the result instead
    //img1.Mutate(o => o.Resize(new Size(100, 150))); 
    img2.Mutate(o => o.Resize(new Size(width, height)));

    // take the 2 source images and draw them onto the image
     if(OptionsArg.Item5=="horizontal") 
           {
    outputImage.Mutate(o => o
        .DrawImage(img1, new Point(0, 0), 1f) // draw the first one top left
        .DrawImage(img2, new Point((width*i)+border*(i+1), border), 1f) // draw the second next to it
    );
           }else
{
    outputImage.Mutate(o => o
        .DrawImage(img1, new Point(0, 0), 1f) // draw the first one top left
        .DrawImage(img2, new Point(border,(height*i)+border*(i+1)), 1f) // draw the second next to it
    );
}    
    outputImage.Save(Path.Combine(Directory.GetCurrentDirectory(), @"uploads", (count++).ToString()+"cu.png"));
}
            }
            
            return 0;
        }
    }
}