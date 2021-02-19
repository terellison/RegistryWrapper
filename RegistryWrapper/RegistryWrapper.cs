using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace RegistryWrapper
{
    /// <summary>
    /// Wrapper for accessing the Windows Registry
    /// </summary>
    public class RegistryWrapper
    {
        /// <summary>
        /// Enum for determining the root key to be accessed (e.g. HKEY_LOCAL_MACHINE = LocalMachine)
        /// </summary>
        public enum RegistryKeyRoot
        {
            /// <summary>
            /// For accessing HKEY_Users
            /// </summary>
            Users,
            /// <summary>
            /// For accessing HKEY_Current_User
            /// </summary>
            CurrentUser,
            /// <summary>
            /// For accessing HKEY_Local_Machine
            /// </summary>
            LocalMachine,
            /// <summary>
            /// For accessing HKEY_Current_Config
            /// </summary>
            CurrentConfig,
            /// <summary>
            /// For accessing HKEY_Classes_Root
            /// </summary>
            ClassesRoot,
            /// <summary>
            /// OBSOLETE For accessing HKEY_Dyn_Data
            /// </summary>
            [Obsolete("The DynData registry key only works on Win9x, which is no longer supported by the CLR.  On NT-based operating systems, use the PerformanceData registry key instead.")]
            DynData,
            /// <summary>
            /// For accessing HKEY_Performance_Data
            /// </summary>
            PerformanceData
        }

        /// <summary>
        /// Enum for determining which version of the registry to access. Can be 32-Bit, 64-Bit, or both
        /// </summary>
        public enum RegistryVersion
        {
            /// <summary>
            /// Restricts access to exclusively the 32-Bit registry
            /// </summary>
            Only32Bit,
            /// <summary>
            /// Restricts access to exclusively the 64-Bit registry
            /// </summary>
            Only64Bit,
            /// <summary>
            /// Allows for access to both the 32-Bit and 64-Bit registries simultaneously
            /// </summary>
            Both
        }
        /// <summary>
        /// Returns the registry key at the path specified
        /// </summary>
        /// <param name="keyPath">Full path of the key to be read</param>
        /// <param name="registryVersion">Version of the registry to be read from</param>
        /// <returns>A key container with the values</returns>
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
        /// <summary>
        /// Returns the registry key at the path specified
        /// </summary>
        /// <param name="registryKeyRoot">Enum of the root key to read from (e.g. HKEY_LOCAL_MACHINE = LocalMachine)</param>
        /// <param name="keyPath">Path to the key to read from (without the root key)</param>
        /// <param name="registryVersion">Version of the registry to read from. Can be 32-Bit, 64-Bit, or both</param>
        /// <returns></returns>
        public RegistryKeyContainer ReadKey(RegistryKeyRoot registryKeyRoot , string keyPath, RegistryVersion registryVersion)
        {
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

            var keyContainer = new RegistryKeyContainer(registryKeyRoot,keyPath, _64BitValues, _32BitValues, registryVersion);
            return keyContainer;
        }
        /// <summary>
        /// Writes a value to given key
        /// </summary>
        /// <param name="registryKeyRoot">Enum of the root key to read from (e.g. HKEY_LOCAL_MACHINE = LocalMachine)</param>
        /// <param name="key">Path to the key to write to. If part of the given key does not already exist it will be written</param>
        /// <param name="registryVersion">Version of the registry to write to. Can be 32-Bit, 64-Bit, or both</param>
        /// <param name="valueToWrite">Value to write to the key. should be given in the structure (Name, Data)</param>
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
        /// <summary>
        /// Writes a subkey to a given key
        /// </summary>
        /// <param name="registryKeyRoot">Enum of the root key to read from (e.g. HKEY_LOCAL_MACHINE = LocalMachine)</param>
        /// <param name="subKey">Path of the subkey to be written to. Allows for nested subkeys to be written even if parts of the path
        /// do not already exist.</param>
        /// <param name="registryVersion">Version of the registry to write to. Can be 32-Bit, 64-Bit, or both</param>
        /// <param name="valuesToWrite">Values to be written to the given subkey</param>
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
        /// <summary>
        /// Deletes a given subkey
        /// </summary>
        /// <param name="registryKeyRoot">Enum of the root key to delete from (e.g. HKEY_LOCAL_MACHINE = LocalMachine)</param>
        /// <param name="subKey">Path of the subkey to be deleted</param>
        /// <param name="registryVersion">Version of the registry to delete from. Can be 32-Bit, 64-Bit, or both</param>
        /// <param name="forceSubKeyTreeDeletion">Can be used to force deletion of subkey tree. Defaults to false</param>
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
                        throw new NullReferenceException(NullRefMsg, e);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion)
                        {
                            DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion);
                        }
                        else throw new InvalidOperationException(InvalidOpMsg, e);
                    }

                    rk = DetermineRootKey(registryKeyRoot);
                    try
                    {
                        rk.DeleteSubKey(subKey);
                    }
                    catch (NullReferenceException e)
                    {
                        throw new NullReferenceException(NullRefMsg, e);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion)
                        {
                            DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion);
                        }
                        else throw new InvalidOperationException(InvalidOpMsg, e);
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
                    catch (NullReferenceException e) { throw new NullReferenceException(NullRefMsg, e);}
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion) { DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion); }
                        else throw new InvalidOperationException(InvalidOpMsg, e);
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
                        throw new NullReferenceException(NullRefMsg, e);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (forceSubKeyTreeDeletion)
                        {
                            DeleteSubKeyTree(registryKeyRoot, subKey, registryVersion);
                        }
                        else throw new InvalidOperationException(InvalidOpMsg, e);
                    }
                    break;

            }
            rk.Flush();
        }
        /// <summary>
        /// Deletes a given subkey and all of its subkeys
        /// </summary>
        /// <param name="registryKeyRoot">Enum of the root key to delete from (e.g. HKEY_LOCAL_MACHINE = LocalMachine)</param>
        /// <param name="subKey">Path of the subkey to be deleted</param>
        /// <param name="registryVersion">Version of the registry to delete from. Can be 32-Bit, 64-Bit, or both</param>
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
        /// <summary>
        /// Deletes a value from a given subkey
        /// </summary>
        /// <param name="registryKeyRoot">Enum of the root key to delete from (e.g. HKEY_LOCAL_MACHINE = LocalMachine)</param>
        /// <param name="subKey">Path of the subkey to delete the value from</param>
        /// <param name="value">Value to be deleted</param>
        /// <param name="registryVersion">Version of the registry to delete from. Can be 32-Bit, 64-Bit, or both</param>
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
                        throw new UnauthorizedAccessException(UnauthorizedAccessMsg, e);

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
                        throw new UnauthorizedAccessException(UnauthorizedAccessMsg, e);

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
                        throw new UnauthorizedAccessException(UnauthorizedAccessMsg, e);

                    }
                    break;
            }
                    rk.Flush();
        }
        /// <summary>
        /// Determines the root key to be accessed
        /// </summary>
        /// <param name="registryKeyRoot"></param>
        /// <returns>Returns the underlying type that's being wrapped</returns>
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
                #pragma warning disable 618
                case RegistryKeyRoot.DynData:
                    rk = Registry.DynData;
                    break;
                #pragma warning restore 618
                case RegistryKeyRoot.PerformanceData:
                    rk = Registry.PerformanceData;
                    break;
                default: 
                    rk = Registry.CurrentUser;
                    break;
            }

            return rk;
        }
        /// <summary>
        /// Determines the root key to be accessed from a string
        /// </summary>
        /// <param name="registryKeyRoot"></param>
        /// <returns>Returns the enum of the root key to be accessed</returns>
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
                #pragma warning disable 618
                case "HKEY_DYN_DATA":
                    rk = RegistryKeyRoot.DynData;
                    break;
                #pragma warning restore 618
                case "HKEY_PERFORMANCE_DATA":
                    rk = RegistryKeyRoot.PerformanceData;
                    break;
                default: 
                    rk = RegistryKeyRoot.CurrentUser;
                    break;
            }
            return rk;
        }
        /// <summary>
        /// Determines the RegistryHive to be used (the root key but for 32-Bit systems)
        /// </summary>
        /// <param name="registryKeyRoot"></param>
        /// <returns>Returns the RegistryHive (root key)</returns>
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
                #pragma warning disable 618
                case RegistryKeyRoot.DynData: // Obsolete; On NT based systems use PerformanceData instead
                    rh = RegistryHive.DynData;
                    break;
                #pragma warning restore 618
                case RegistryKeyRoot.PerformanceData:
                    rh = RegistryHive.PerformanceData;
                    break;
                default: 
                    rh = RegistryHive.CurrentUser;
                    break;
            }
            return rh;
        }
        /// <summary>
        /// Determines the version of the registry to be accessed (64-Bit or 32-Bit)
        /// </summary>
        /// <param name="registryVersion"></param>
        /// <returns>Returns the appropriate RegistryView</returns>
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
        /// <summary>
        /// Message for invalid operations
        /// </summary>
        private static readonly string InvalidOpMsg = "The given subKey has subKeys of its own, cannot delete. " +
                                                      "Specify parameter forceSubKeyTreeDeletion to" +
                                                      " force deletion of this subKey and all" +
                                                      " of it's subKeys, or call DeleteSubKeyTree";
        /// <summary>
        /// Message for reading from a subkey that does not exist
        /// </summary>
        private static readonly string NullRefMsg = "The given subkey does not exist";
        /// <summary>
        /// When you forget to run as Administrator, this is what you will see :)
        /// </summary>
        private static readonly string UnauthorizedAccessMsg = "You do not have permission to write to this key." +
                                                               "Try running as administrator.";
    }
}
