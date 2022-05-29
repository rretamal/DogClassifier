using Android.Graphics;
using Android.Util;
using Java.IO;
using Java.Nio;
using Java.Nio.Channels;

namespace Maui.DogClassifier.Core
{
    // All the code in this file is only included on Android.
    public class Classifier
    {
        private MappedByteBuffer model;
        private List<string> labels;
        private Org.Tensorflow.Lite.Interpreter interpreter;
        private int floatSize = 4;
        private int pixelSize = 3;

        public Classifier()
        {
            model = LoadModel();
            labels = LoadLabels();

            interpreter = new Org.Tensorflow.Lite.Interpreter(model);
        }

        public ImageResult ClassifyImage(byte[] image)
        {
            var tensor = interpreter.GetInputTensor(0);
            var shape = tensor.Shape();

            var width = shape[1];
            var height = shape[2];

            var byteBuffer = GetPhotoAsByteBuffer(image, width, height);

            var outputLocations = new float[1][] { new float[labels.Count] };
            var outputs = Java.Lang.Object.FromArray(outputLocations);

            interpreter.Run(byteBuffer, outputs);
            var classificationResult = outputs.ToArray<float[]>();

            //Map the classificationResult to the labels and sort the result to find which label has the highest probability
            var classificationModelList = new List<ImageResult>();

            for (var i = 0; i < labels.Count; i++)
            {
                var label = labels[i]; classificationModelList.Add(new ImageResult()
                {
                    Name = label,
                    Percentage = classificationResult[0][i]
                });
            }

            var top = classificationModelList.OrderByDescending(c => c.Percentage).FirstOrDefault();

            return top;
        }

        private MappedByteBuffer LoadModel()
        {
            var assets = Android.App.Application.Context.Assets;

            var assetDescriptor = assets.OpenFd("model.tflite");
            var inputStream = new FileInputStream(assetDescriptor.FileDescriptor);

            var mappedByteBuffer = inputStream.Channel.Map(FileChannel.MapMode.ReadOnly, assetDescriptor.StartOffset, assetDescriptor.DeclaredLength);

            return mappedByteBuffer;
        }

        private List<string> LoadLabels()
        {
            var assets = Android.App.Application.Context.Assets;

            var streamReader = new StreamReader(assets.Open("labels.txt"));

            //Transform labels.txt into List<string>
            var labels = streamReader.ReadToEnd().Split('\n').ToList();

            return labels;
        }

        private ByteBuffer GetPhotoAsByteBuffer(byte[] image, int width, int height)
        {
            var bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
            var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, true);

            var modelInputSize = floatSize * height * width * pixelSize;
            var byteBuffer = ByteBuffer.AllocateDirect(modelInputSize);
            byteBuffer.Order(ByteOrder.NativeOrder());

            var pixels = new int[width * height];
            resizedBitmap.GetPixels(pixels, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

            var pixel = 0;

            MemoryStream byteArrayOutputStream = new MemoryStream();
            resizedBitmap.Compress(Bitmap.CompressFormat.Png, 100, byteArrayOutputStream);
            byte[] byteArray = byteArrayOutputStream.ToArray();
            String encoded = Base64.EncodeToString(byteArray, Base64Flags.Default);

            //Loop through each pixels to create a Java.Nio.ByteBuffer
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var pixelVal = pixels[pixel++];

                    byteBuffer.PutFloat(pixelVal >> 16 & 0xFF);
                    byteBuffer.PutFloat(pixelVal >> 8 & 0xFF);
                    byteBuffer.PutFloat(pixelVal & 0xFF);
                }
            }

            bitmap.Recycle();

            return byteBuffer;
        }
    }
}