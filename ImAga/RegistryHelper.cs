using Microsoft.Win32;

namespace ImAga
{
    internal static class RegistryHelper
    {
        const string USER_ROOT = "HKEY_CURRENT_USER";
        const string SUBKEY = @"Software\iTee\ImAga";
        const string KEY_NAME = USER_ROOT + "\\" + SUBKEY;

        internal static void Write(string key, string value)
        {
            Registry.SetValue(KEY_NAME, key, value, RegistryValueKind.String);
        }
        internal static void Write(string key, int value)
        {
            Registry.SetValue(KEY_NAME, key, value, RegistryValueKind.DWord);
        }
        internal static string Read(string key)
        {
            return (string)Registry.GetValue(KEY_NAME, key, null);
        }

        internal static int? ReadInt(string key)
        {
            return (int?)Registry.GetValue(KEY_NAME, key, null);
        }

        internal static void Delete(string keyName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(SUBKEY, true))
            {
                key?.DeleteValue(keyName);
            }
        }
    }
}
