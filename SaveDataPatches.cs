using System.IO;

namespace PSW.BinarySave
{
    namespace Patches
    {
        internal static class SaveDataPatches
        {
            public static bool Sequal<T>(this T[] source, T[] comparer)
            {
                bool retval = source[0].Equals(comparer[0]);
                for (int i = 1; i < comparer.Length && i < source.Length; i++)
                {
                    retval = retval && source[i].Equals(comparer[i]);
                }
                return retval;
            }

            public static byte[] MakeByteArrayFromHere(this Stream str)
            {
                byte[] retval = new byte[(str.Length - str.Position)];
                str.Read(retval, 0, retval.Length);
                return retval;
            }
        }
    }
}
