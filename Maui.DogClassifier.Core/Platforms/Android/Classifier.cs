using Java.IO;
using Java.Nio;
using Java.Nio.Channels;

namespace Maui.DogClassifier.Core
{
    // All the code in this file is only included on Android.
    public class Classifier
    {
        private MappedByteBuffer model;

        public Classifier()
        {
            model = LoadModel();
            
        }

        public void GetLabel()
        { 
        }

        private MappedByteBuffer LoadModel()
        {
            var assets = Android.App.Application.Context.Assets;

            var assetDescriptor = assets.OpenFd("model.tflite");
            var inputStream = new FileInputStream(assetDescriptor.FileDescriptor);

            var mappedByteBuffer = inputStream.Channel.Map(FileChannel.MapMode.ReadOnly, assetDescriptor.StartOffset, assetDescriptor.DeclaredLength);

            return mappedByteBuffer;
        }
    }
}