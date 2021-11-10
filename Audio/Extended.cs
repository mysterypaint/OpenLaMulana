using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenLaMulana.Audio
{
    //Class that provides language extensions made by Maki
    public static class Extended
    {
        public enum Languages
        {
            en,
            fr,
            de,
            es,
            it,
            jp
        }
        public static bool Save_As_PNG(Texture2D texture, string path, int width, int height)
        {
            var return_value = false;
            try
            {
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                texture.SaveAsPng(fs, width, height);
                return_value = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return return_value;
        }

        //https://stackoverflow.com/a/2887/4509036
        public static T ByteArrayToClass<T>(byte[] bytes) where T : class
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
        //https://stackoverflow.com/a/2887/4509036
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

    }
}
