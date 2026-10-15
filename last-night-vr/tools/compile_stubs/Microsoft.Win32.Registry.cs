// Compile-check stand-in: Unity's editor profile (.NET Framework) has Microsoft.Win32.Registry; the
// .NET Standard reference set used by this checker doesn't. Only OpenXR's Windows runtime picker uses it.
namespace Microsoft.Win32
{
    public sealed class RegistryKey : System.IDisposable
    {
        public RegistryKey OpenSubKey(string name, bool writable) => null;
        public RegistryKey OpenSubKey(string name) => null;
        public string[] GetValueNames() => new string[0];
        public string[] GetSubKeyNames() => new string[0];
        public object GetValue(string name) => null;
        public object GetValue(string name, object defaultValue) => defaultValue;
        public void Close() { }
        public void Dispose() { }
    }

    public static class Registry
    {
        public static readonly RegistryKey LocalMachine = new RegistryKey();
        public static readonly RegistryKey CurrentUser = new RegistryKey();
        public static object GetValue(string keyName, string valueName, object defaultValue) => defaultValue;
    }
}
