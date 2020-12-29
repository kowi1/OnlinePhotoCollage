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


        public int Resize(String imageName,string message)
        {
            
            using (Image image = Image.Load(Path.Combine(Directory.GetCurrentDirectory(), @"uploads", imageName)))
            {
                image.Mutate(x => x
                     .Resize(image.Width / 2, image.Height / 2));
                     //.Grayscale());

                image.Save(Path.Combine(Directory.GetCurrentDirectory(), @"uploads","OnlinePhotoCollage"+ message+imageName)); // Automatic encoder selected based on extension.
            }
            return 0;
        }


        public string Stitch(List<string> imagelist,string message,Tuple<int, int, int, int, string, string> settings)
        {  
            Image<Rgba32> img1;
            Image<Rgba32> img2;
            Image<Rgba32> outputImage;


            int width=150;
            int height=180;
            int border=settings.Item1;

            int horizontalWidth= (width+2*border)*imagelist.Count;
            int horizontalHeight= height+2*border;
            int verticalWidth= width+2*border;
            int verticalHeight= (height+2*border)*imagelist.Count;

            if(settings.Item5=="horizontal") //orientation is horizontal
            {
                 outputImage = new Image<Rgba32>(horizontalWidth,horizontalHeight );
            }
            else ////orientation is vertical
            {
                 outputImage = new Image<Rgba32>(verticalWidth,verticalHeight);
            }
            
            //Boarder Color
            Rgba32 colorRGB= new Rgba32(Convert.ToByte(Convert.ToUInt32(settings.Item2)),Convert.ToByte(Convert.ToUInt32(settings.Item3)),Convert.ToByte(Convert.ToUInt32(settings.Item4)));
            Color color=new Color(colorRGB);

            //Fill background Color   
            outputImage.Mutate(i => i.Fill(color));
            
            //Sequentially add all images in list to the output image by shifting them to the right position.
            for (int i=0;i<imagelist.Count;i++)
            {
                 img1= outputImage;
                 img2 = Image.Load<Rgba32>(Path.Combine(Directory.GetCurrentDirectory(), @"uploads", imagelist[i]));
              
                 // create output image of the correct dimensions
                 {
                      // reduce source images to correct dimensions
                      // skip if already correct size
                      // if you need to use source images else where use Clone and take the result instead
                      //img1.Mutate(o => o.Resize(new Size(100, 150))); 
                      img2.Mutate(o => o.Resize(new Size(width, height)));
                      // take the 2 source images and draw them onto the image
                      if(settings.Item5=="horizontal") //orientation is horizontal
                      {
                             outputImage.Mutate(o => o
                                        .DrawImage(img1, new Point(0, 0), 1f) // draw the first one top left
                                        .DrawImage(img2, new Point((width*i)+border*(i+1), border), 1f) // draw the second next to it
                                        );
                       }
                       else //orientation is vertical
                       {
                             outputImage.Mutate(o => o
                                         .DrawImage(img1, new Point(0, 0), 1f) // draw the first one top left
                                         .DrawImage(img2, new Point(border,(height*i)+border*(i+1)), 1f) // draw the second below it
                                        );
                        }
                 }
             }
              var outputFile = Path.Combine(Directory.GetCurrentDirectory(), @"uploads","OnlinePhotoCollage"+ message+".png");
             outputImage.Save(outputFile);
             return outputFile;
        }
    }
}