using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FluentAI.Example;

public static class Keychain
{
    [DllImport("/System/Library/Frameworks/Security.framework/Security", EntryPoint = "SecKeychainFindGenericPassword")]
    public static extern int SecKeychainFindGenericPassword(
        IntPtr keychain,
        int serviceNameLength,
        string serviceName,
        int accountNameLength,
        string accountName,
        out uint passwordLength,
        out IntPtr passwordData,
        IntPtr itemRef
    );

    [DllImport("/System/Library/Frameworks/Security.framework/Security", EntryPoint = "SecKeychainItemFreeContent")]
    public static extern int SecKeychainItemFreeContent(
        IntPtr attrList,
        IntPtr data
    );

    public static string GetKeychainPassword(string serviceName, string accountName)
    {
        uint passwordLength;
        IntPtr passwordData;
        IntPtr itemRef = IntPtr.Zero;

        int result = SecKeychainFindGenericPassword(
            IntPtr.Zero,
            serviceName.Length,
            serviceName,
            accountName.Length,
            accountName,
            out passwordLength,
            out passwordData,
            itemRef
        );

        if (result != 0)
        {
            throw new InvalidOperationException($"Error retrieving keychain item: {result}");
        }

        byte[] passwordBytes = new byte[passwordLength];
        Marshal.Copy(passwordData, passwordBytes, 0, (int)passwordLength);
        string password = Encoding.UTF8.GetString(passwordBytes);

        SecKeychainItemFreeContent(IntPtr.Zero, passwordData);

        return password;
    }
}