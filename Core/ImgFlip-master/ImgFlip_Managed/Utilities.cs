using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ImgFlip
{
    /// <summary>
    /// A few utilities that make using ImgFlip easier
    /// </summary>
    public abstract class Utilities
    {
        /// <summary>
        /// Makes a secure string from a string
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>a SecureString version of the input string</returns>
        public static SecureString MakeSecureStringFromString(string input)
        {
            SecureString secureString = new SecureString();

            foreach (char character in input.ToCharArray())
            {
                secureString.AppendChar(character);
            }

            return secureString;
        }

        /// <summary>
        /// Gets the underlying string for a SecureString
        /// </summary>
        /// <param name="secureString">the secure string</param>
        /// <returns>the secure string contents</returns>
        public static string MakeStringFromSecureString(SecureString secureString)
        {
            IntPtr unmanagedPointer = IntPtr.Zero;

            try
            {
                unmanagedPointer = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedPointer);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedPointer);
            }
        }
    }
}
