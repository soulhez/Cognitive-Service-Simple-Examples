﻿using ImageSharp;
using ImageSharp.Drawing.Brushes;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveFunctions
{
    public class BlurFaces
    {
        private static string _faceAPIKey = "1d6e182e79c74fa6ac7817c1303a3e69";
        // The name of the source image.
        const string _sourceImage = @"C:\Users\adamt\Pictures\LA Hacks Pics\DSC02450.JPG";

        // The name of the destination image
        const string _destinationImage = @"C:\Users\adamt\Pictures\LA Hacks Pics\DSC02450Blurred.JPG";
        private static List<string> _emoticons = new List<string>() {
            @"C:\Users\adamt\Pictures\LA Hacks Pics\smileemoji.png",
            @"C:\Users\adamt\Pictures\LA Hacks Pics\1afawkes.png",
            @"C:\Users\adamt\Pictures\LA Hacks Pics\crying happy.png",
            @"C:\Users\adamt\Pictures\LA Hacks Pics\1apileOfPoo.png"};
        public static void Process()
        {

            // Detect the faces in the source file
            DetectFaces(_sourceImage, _faceAPIKey)
                .ContinueWith((task) =>
                {
                    // Save the result of the detection
                    var faceRects = task.Result;

                    Console.WriteLine($"Detected {faceRects.Length} faces");

                    // Blur the detected faces and save in another file
                    BlurThemFaces(faceRects, _sourceImage, _destinationImage);

                    Console.WriteLine($"Done!!!");
                });

            Console.ReadLine();
        }

        /// <summary>
        /// Blur the detected faces from de source image.
        /// </summary>
        /// <param name="faceRects">The detected faces rectangles</param>
        /// <param name="sourceImage">The source image</param>
        /// <param name="destinationImage">The destination image</param>
        private static void BlurThemFaces(FaceRectangle[] faceRects, string sourceImage, string destinationImage)
        {
            if (File.Exists(destinationImage))
            {
                File.Delete(destinationImage);
            }

            if (faceRects.Length > 0)
            {
                using (FileStream stream = File.OpenRead(sourceImage))
                {
                    using (FileStream output = File.OpenWrite(destinationImage))
                    {
                        List<Image> emoticons = new List<Image>();
                        _emoticons.ForEach(o => emoticons.Add(new Image(o)));

                        var image = new Image<Color>(stream);
                        Random random = new Random(DateTime.Now.Millisecond);
                        // Blur every detected face
                        foreach (var faceRect in faceRects)
                        {
                            var rectangle = new Rectangle(
                                faceRect.Left,
                                faceRect.Top,
                                faceRect.Width,
                                faceRect.Height);

                            //image = image.BoxBlur(20, rectangle);

                            var randomEmoticon = emoticons[random.Next(emoticons.Count)];
                            image = image.DrawImage(randomEmoticon, 100, rectangle.Size, new Point(rectangle.X, rectangle.Y));
                        }

                        image.SaveAsJpeg(output);

                    }
                }
            }

        }

        /// <summary>
        /// Detect faces calling the Face API
        /// </summary>
        /// <param name="imageFilePath">ource image</param>
        /// <param name="apiKey">Azure Face API Key</param>
        /// <returns>Detected faces rectangles</returns>
        private static async Task<FaceRectangle[]> DetectFaces(string imageFilePath, string apiKey)
        {
            var faceServiceClient = new FaceServiceClient(apiKey);

            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    return faceRects.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new FaceRectangle[0];
            }
        }
    }
}
