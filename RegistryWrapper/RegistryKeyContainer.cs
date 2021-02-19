using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace RegistryWrapper
{
    /// <summary>
    /// Data structure for containing values read from the registry
    /// </summary>
    public class RegistryKeyContainer
    {
        /// <summary>
        /// Root where values were read from
        /// </summary>
        public RegistryWrapper.RegistryKeyRoot RegistryKeyRoot { get; }
        /// <summary>
        /// Key corresponding to the values
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// List of 64-Bit values corresponding to the key in the form (keyName, value)
        /// </summary>
        public List<KeyValuePair<string, object>> _64BitValues { get;}
        /// <summary>
        /// List of 32-Bit values corresponding to the key in the form (keyName, value)
        /// </summary>
        public List<KeyValuePair<string, object>> _32BitValues { get; }
        /// <summary>
        /// Version of registry where values were read from
        /// </summary>
        public RegistryWrapper.RegistryVersion RegistryVersion { get; }
        /// <summary>
        /// List of 32-Bit subkeys for the given Key
        /// </summary>
        public string[] _32BitSubKeyNames { get; private set; }
        /// <summary>
        /// List of 64-Bit subkeys for the given key
        /// </summary>
        public string[] _64BitSubKeyNames { get; private set; }

        internal RegistryKeyContainer(RegistryWrapper.RegistryKeyRoot registryKeyRoot,string key, List<KeyValuePair<string, object>> _64BitValues,
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
                    _64BitSubKeyNames = new string[0];
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
                    _32BitSubKeyNames = new string[0];
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
