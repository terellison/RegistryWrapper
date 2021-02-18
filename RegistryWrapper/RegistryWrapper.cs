using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace RegistryWrapper
{
    public class RegistryWrapper
    {

        public enum RegistryKeyRoot { Users, CurrentUser, LocalMachine, CurrentConfig, ClassesRoot, DynData, PerformanceData }

        public enum RegistryVersion { Only32Bit, Only64Bit, Both }

        public RegistryKeyContainer ReadKey(string keyPath, RegistryVersion registryVersion)
        {
            var keyPathSplit = keyPath.Split('\\');
            var rootKeyString = keyPathSplit[0];
            var registryKeyRoot = DetermineRootKey(rootKeyString);
            keyPath = "";
            for (var i = 1; i < keyPathSplit.Length; i++)
            {
                var t = keyPathSplit[i];
                keyPath += (t + "\\");
            }
            return ReadKey(registryKeyRoot, keyPath, registryVersion);
        }

        public RegistryKeyContainer ReadKey(RegistryKeyRoot registryKeyRoot , string keyPath, RegistryVersion registryVersion)
        {
            RegistryKeyContainer keyContainer;
            var _32BitValues = new List<KeyValuePair<string, object>>();
            var _64BitValues = new List<KeyValuePair<string, object>>();
            RegistryHive rh;
            RegistryView rv;
            RegistryKey rk;
            RegistryKey sk;
            switch (registryVersion)
            {
                case RegistryVersion.Both:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = RegistryView.Registry32;
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    try
                    {
                        sk = rk.OpenSubKey(keyPath);
                        foreach (var valueName in sk.GetValueNames())
                        {
                            _32BitValues.Add(new KeyValuePair<string, object>(valueName, sk.GetValue(valueName)));
                        }
                    }
                    catch (NullReferenceException)
                    {}
                    rk = DetermineRootKey(registryKeyRoot);
                    try
                    {
                        sk = rk.OpenSubKey(keyPath);
                        foreach (var valueName in sk.GetValueNames())
                        {
                            _64BitValues.Add(new KeyValuePair<string, object>(valueName, sk.GetValue(valueName)));
                        }
                    }
                    catch (NullReferenceException)
                    {}
                    break;
                case RegistryVersion.Only32Bit: 
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = RegistryView.Registry32;
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    try
                    {
                        sk = rk.OpenSubKey(keyPath);
                        foreach (var valueName in sk.GetValueNames())
                        {
                            _32BitValues.Add(new KeyValuePair<string, object>(valueName, sk.GetValue(valueName)));
                        }
                    }
                    catch (NullReferenceException) {}
                    break;
                default:
                    rk = DetermineRootKey(registryKeyRoot);
                    try
                    {
                        sk = rk.OpenSubKey(keyPath);
                        foreach (var valueName in sk.GetValueNames())
                        {
                            _64BitValues.Add(new KeyValuePair<string, object>(valueName, sk.GetValue(valueName)));
                        }
                    }
                    catch (NullReferenceException) {}
                    break;
            }

            keyContainer = new RegistryKeyContainer(registryKeyRoot,keyPath, _64BitValues, _32BitValues, registryVersion);
            return keyContainer;
        }

        public void WriteValue(RegistryKeyRoot registryKeyRoot, string key, RegistryVersion registryVersion,
            List<KeyValuePair<string, object>> valueToWrite)
        {
            RegistryHive rh;
            RegistryView rv;
            RegistryKey rk;
            RegistryKey subKey;
            switch (registryVersion)
            {
                case RegistryVersion.Both:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    subKey = rk.CreateSubKey(key);
                    foreach (var item in valueToWrite)
                    {
                        subKey.SetValue(item.Key, item.Value);
                    }
                    rk = DetermineRootKey(registryKeyRoot);
                    subKey = rk.CreateSubKey(key);
                    foreach (var item in valueToWrite)
                    {
                        subKey.SetValue(item.Key, item.Value);
                    }
                    rk.Flush();
                    break;
                case RegistryVersion.Only32Bit:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    subKey = rk.CreateSubKey(key);
                    foreach (var item in valueToWrite)
                    {
                        subKey.SetValue(item.Key, item.Value);
                    }
                    rk.Flush();
                    break;
                default:
                    rk = DetermineRootKey(registryKeyRoot);
                    subKey = rk.CreateSubKey(key);
                    foreach (var item in valueToWrite)
                    {
                        subKey.SetValue(item.Key, item.Value);
                    }
                    rk.Flush();
                    break;

            }
            
        }

        public void WriteSubKey(RegistryKeyRoot registryKeyRoot, string subKey, 
            RegistryVersion registryVersion, [Optional] List<KeyValuePair<string, object>> valuesToWrite)
        {
            RegistryKey rk;
            RegistryHive rh;
            RegistryView rv;
            RegistryKey newSk;

            switch (registryVersion)
            {
                case RegistryVersion.Both:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    newSk = rk.CreateSubKey(subKey);
                    if (valuesToWrite != null)
                    {
                        foreach (var item in valuesToWrite)
                        {
                            newSk.SetValue(item.Key, item.Value);
                        }
                    }
                    rk = DetermineRootKey(registryKeyRoot);
                    newSk = rk.CreateSubKey(subKey);

                    if (valuesToWrite != null)
                    {
                        foreach (var item in valuesToWrite)
                        {
                            newSk.SetValue(item.Key, item.Value);
                        }
                    }
                    break;
                case RegistryVersion.Only32Bit:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    newSk = rk.CreateSubKey(subKey);
                    if (valuesToWrite != null)
                    {
                        foreach (var item in valuesToWrite)
                        {
                            newSk.SetValue(item.Key, item.Value);
                        }
                    }
                    break;
                default:
                    rk = DetermineRootKey(registryKeyRoot);
                    newSk = rk.CreateSubKey(subKey);
                    if (valuesToWrite != null)
                    {
                        foreach (var item in valuesToWrite)
                        {
                            newSk.SetValue(item.Key, item.Value);
                        }
                    }
                    break;   
            }
            rk.Flush();
        }

        public void DeleteSubKey(RegistryKeyRoot registryKeyRoot, string subKey, RegistryVersion registryVersion , [Optional] bool forceSubKeyTreeDeletion)
        {
            RegistryKey rk;
            RegistryHive rh;
            RegistryView rv;
            switch (registryVersion)
            {
                case RegistryVersion.Both:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    try
                    {
                        rk.DeleteSubKey(subKey);
                    }
                    catch (NullReferenceException e)
                    {
                        throw new NullReferenceException(nullRefMsg, e);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion)
                        {
                            DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion);
                        }
                        else throw new InvalidOperationException(invalidOpMsg, e);
                    }

                    rk = DetermineRootKey(registryKeyRoot);
                    try
                    {
                        rk.DeleteSubKey(subKey);
                    }
                    catch (NullReferenceException e)
                    {
                        throw new NullReferenceException(nullRefMsg, e);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion)
                        {
                            DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion);
                        }
                        else throw new InvalidOperationException(invalidOpMsg, e);
                    }

                    break;
                case RegistryVersion.Only32Bit:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    try
                    {
                        rk.DeleteSubKey(subKey);
                    }
                    catch (NullReferenceException e) { throw new NullReferenceException(nullRefMsg, e);}
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion) { DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion); }
                        else throw new InvalidOperationException(invalidOpMsg, e);
                    }
                    break;
                default:
                    rk = DetermineRootKey(registryKeyRoot);
                    try
                    {
                        rk.DeleteSubKey(subKey);
                    }
                    catch (NullReferenceException e)
                    {
                        throw new NullReferenceException(nullRefMsg, e);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion)
                        {
                            DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion);
                        }
                        else throw new InvalidOperationException(invalidOpMsg, e);
                    }
                    break;

            }
            rk.Flush();
        }

        public void DeleteSubKeyTree(RegistryKeyRoot registryKeyRoot, string subKey,RegistryVersion registryVersion)
        {
            RegistryKey rk;
            RegistryHive rh;
            RegistryView rv;

            switch (registryVersion)
            {
                case RegistryVersion.Both:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    rk.DeleteSubKeyTree(subKey);
                    rk = DetermineRootKey(registryKeyRoot);
                    rk.DeleteSubKeyTree(subKey);
                    break;
                case RegistryVersion.Only32Bit:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    rk.DeleteSubKeyTree(subKey);
                    break;
                default:
                    rk = DetermineRootKey(registryKeyRoot);
                    rk.DeleteSubKeyTree(subKey);
                    break;
            }
            rk.Flush();
        }

        public void DeleteValue(RegistryKeyRoot registryKeyRoot, string subKey, string value, RegistryVersion registryVersion)
        {
            RegistryKey rk;
            RegistryHive rh;
            RegistryView rv;
            RegistryKey sk;
            switch (registryVersion)
            {
                case RegistryVersion.Both:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    try
                    {
                        sk = rk.OpenSubKey(subKey, true);
                        sk.DeleteValue(value, true);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new UnauthorizedAccessException("You do not have permission to write to this key." +
                                                              "Try running as administrator.", e);

                    }

                    rk = DetermineRootKey(registryKeyRoot);
                    try
                    {
                        sk = rk.OpenSubKey(subKey, true);
                        sk.DeleteValue(value, true);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new UnauthorizedAccessException(unauthorizedAccessMsg, e);

                    }

                    break;
                case RegistryVersion.Only32Bit:
                    rh = DetermineRegistryHive(registryKeyRoot);
                    rv = DetermineRegistryView(RegistryVersion.Only32Bit);
                    rk = RegistryKey.OpenBaseKey(rh, rv);
                    try
                    {
                        sk = rk.OpenSubKey(subKey, true);
                        sk.DeleteValue(value, true);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new UnauthorizedAccessException(unauthorizedAccessMsg, e);

                    }
                    break;
                default:
                    rk = DetermineRootKey(registryKeyRoot);
                    try
                    {
                        sk = rk.OpenSubKey(subKey, true);
                        sk.DeleteValue(value, true);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new UnauthorizedAccessException(unauthorizedAccessMsg, e);

                    }
                    break;
            }
                    rk.Flush();
        }

        internal RegistryKey DetermineRootKey(RegistryKeyRoot registryKeyRoot)
        {
            RegistryKey rk;

            switch (registryKeyRoot)
            {
                case RegistryKeyRoot.LocalMachine:
                    rk = Registry.LocalMachine;
                    break;
                case RegistryKeyRoot.CurrentConfig:
                    rk = Registry.CurrentConfig;
                    break;
                case RegistryKeyRoot.ClassesRoot:
                    rk = Registry.ClassesRoot;
                    break;
                case RegistryKeyRoot.Users:
                    rk = Registry.Users;
                    break;
                case RegistryKeyRoot.DynData: // Obsolete; On NT based systems use PerformanceData instead
                    #pragma warning disable 618
                    rk = Registry.DynData;
                    #pragma warning restore 618
                    break;
                case RegistryKeyRoot.PerformanceData:
                    rk = Registry.PerformanceData;
                    break;
                default: 
                    rk = Registry.CurrentUser;
                    break;
            }

            return rk;
        }

        private RegistryKeyRoot DetermineRootKey(string registryKeyRoot)
        {
            RegistryKeyRoot rk;
            switch (registryKeyRoot)
            {
                case "HKEY_LOCAL_MACHINE":
                    rk = RegistryKeyRoot.LocalMachine;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    rk = RegistryKeyRoot.CurrentConfig;
                    break;
                case "HKEY_CLASSES_ROOT":
                    rk = RegistryKeyRoot.ClassesRoot;
                    break;
                case "HKEY_USERS":
                    rk = RegistryKeyRoot.Users;
                    break;
                case "HKEY_DYN_DATA": // Obsolete; On NT based systems use PerformanceData instead
                    rk = RegistryKeyRoot.DynData;
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    rk = RegistryKeyRoot.PerformanceData;
                    break;
                default: 
                    rk = RegistryKeyRoot.CurrentUser;
                    break;
            }
            return rk;
        }

        internal RegistryHive DetermineRegistryHive(RegistryKeyRoot registryKeyRoot)
        {
            RegistryHive rh;

            switch (registryKeyRoot)
            {
                case RegistryKeyRoot.LocalMachine:
                    rh = RegistryHive.LocalMachine;
                    break;
                case RegistryKeyRoot.CurrentConfig:
                    rh = RegistryHive.CurrentConfig;
                    break;
                case RegistryKeyRoot.ClassesRoot:
                    rh = RegistryHive.ClassesRoot;
                    break;
                case RegistryKeyRoot.Users:
                    rh = RegistryHive.Users;
                    break;
                case RegistryKeyRoot.DynData: // Obsolete; On NT based systems use PerformanceData instead
                    rh = RegistryHive.DynData;
                    break;
                case RegistryKeyRoot.PerformanceData:
                    rh = RegistryHive.PerformanceData;
                    break;
                default: 
                    rh = RegistryHive.CurrentUser;
                    break;
            }
            return rh;
        }

        internal RegistryView DetermineRegistryView(RegistryVersion registryVersion)
        {
            RegistryView rv;

            switch (registryVersion)
            {   
                case RegistryVersion.Only32Bit:
                    rv = RegistryView.Registry32;
                    break;
                case RegistryVersion.Only64Bit:
                    rv = RegistryView.Registry64;
                    break;
                
                default: 
                    rv = RegistryView.Default;
                    break;
            }
            return rv;
        }

        private string invalidOpMsg = "The given subKey has subKeys of its own, cannot delete. " +
                              "Specify parameter forceSubKeyTreeDeletion to" +
                              " force deletion of this subKey and all" +
                              " of it's subKeys, or call DeleteSubKeyTree";
        private string nullRefMsg = "The given suKey does not exist";

        private string unauthorizedAccessMsg = "You do not have permission to write to this key." +
                                               "Try running as administrator.";
    }
}
