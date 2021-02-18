using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace RegistryWrapper
{
    public class RegistryKeyContainer
    {
        public RegistryWrapper.RegistryKeyRoot RegistryKeyRoot;
        public string Key;
        public List<KeyValuePair<string, object>> _64BitValues;
        public List<KeyValuePair<string, object>> _32BitValues;
        public RegistryWrapper.RegistryVersion RegistryVersion;
        public string[] _32BitSubKeyNames;
        public string[] _64BitSubKeyNames;

        public RegistryKeyContainer(RegistryWrapper.RegistryKeyRoot registryKeyRoot,string key, List<KeyValuePair<string, object>> _64BitValues,
            List<KeyValuePair<string, object>> _32BitValues, RegistryWrapper.RegistryVersion registryVersion)
        {
            RegistryKeyRoot = registryKeyRoot; 
            Key = key;
            this._64BitValues = _64BitValues;
            this._32BitValues = _32BitValues;
            RegistryVersion = registryVersion;
            GetSubKeyNames();
        }

        private void GetSubKeyNames()
        {

            var helper = new RegistryWrapper();
            RegistryKey rk;
            RegistryKey sk;
            switch (RegistryVersion)
            {
                case RegistryWrapper.RegistryVersion.Both:
                    rk = RegistryKey.OpenBaseKey(helper.DetermineRegistryHive(RegistryKeyRoot), RegistryView.Registry32);
                    sk = rk.OpenSubKey(Key);
                    try
                    {
                        _32BitSubKeyNames = sk.GetSubKeyNames();
                    }
                    catch (NullReferenceException)
                    {
                        _32BitSubKeyNames = new string[0];
                    }

                    rk = helper.DetermineRootKey(RegistryKeyRoot);
                    sk = rk.OpenSubKey(Key);
                    try
                    {
                        _64BitSubKeyNames = sk.GetSubKeyNames();
                    }
                    catch (NullReferenceException)
                    {
                        _64BitSubKeyNames = new string[0];
                    }
                    break;
                case RegistryWrapper.RegistryVersion.Only32Bit:
                    rk = RegistryKey.OpenBaseKey(helper.DetermineRegistryHive(RegistryKeyRoot), RegistryView.Registry32);
                    sk = rk.OpenSubKey(Key);
                    try
                    {
                        _32BitSubKeyNames = sk.GetSubKeyNames();
                    }
                    catch (NullReferenceException)
                    {
                        _32BitSubKeyNames = new string[0];
                    }
                    break;
                default:
                    rk = helper.DetermineRootKey(RegistryKeyRoot);
                    sk = rk.OpenSubKey(Key);
                    try
                    {
                        _64BitSubKeyNames = sk.GetSubKeyNames();
                    }
                    catch (NullReferenceException)
                    {
                        _64BitSubKeyNames = new string[0];
                    }
                    break;
            }
        }
    }
}
