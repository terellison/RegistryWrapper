using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RegistryWrapper.Tests
{
    [TestClass]
    public class RegistryWrapperTests
    {
        
        [TestMethod]
        public void RegistryReadTest()
        {
            
            var helper = new RegistryWrapper();
            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE",
                RegistryWrapper.RegistryVersion.Only64Bit, new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("TestKey1", "TestValue1")
            });
            
            var result = helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE", RegistryWrapper.RegistryVersion.Only64Bit);
            foreach (var item in result._64BitValues)
            {
                Debug.WriteLine("Key: " + item.Key + " Value: " + item.Value);
            }

            Assert.IsNotNull(result, "The given key has no values");
            Assert.IsTrue(result._64BitValues.Count>0);
            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", @"TestKey1", RegistryWrapper.RegistryVersion.Only64Bit);

        }

        [TestMethod]
        public void RegistryWriteValueTest()
        {
            var helper = new RegistryWrapper();

            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE",
                RegistryWrapper.RegistryVersion.Only64Bit, new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Alice", "Allison"),
                new KeyValuePair<string, object>("Bob", "Boberson")
            });

            Assert.IsNotNull(helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", 
                    RegistryWrapper.RegistryVersion.Only64Bit)._64BitValues,
                "Write failed, value does not exist after write attempt");
            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", @"Alice", RegistryWrapper.RegistryVersion.Only64Bit);
            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", @"Bob", RegistryWrapper.RegistryVersion.Only64Bit);
        }

        [TestMethod]
        public void RegistryWriteValueTo32Test()
        {
            var helper = new RegistryWrapper();
            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", RegistryWrapper.RegistryVersion.Only32Bit,
                new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("TestKey1", "TestValue1")
            });
            Assert.IsNotNull(helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE",
                RegistryWrapper.RegistryVersion.Only32Bit)._32BitValues);
            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", "TestKey1", RegistryWrapper.RegistryVersion.Only32Bit);
        }

        [TestMethod]
        public void RegistryWriteValueTo64Test()
        {
            var helper = new RegistryWrapper();
            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE",
                RegistryWrapper.RegistryVersion.Only64Bit, new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("TestKey1", "TestValue1")
            });
            Assert.IsNotNull(helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", RegistryWrapper.RegistryVersion.Only64Bit)._64BitValues);
            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", "TestKey1", RegistryWrapper.RegistryVersion.Only64Bit);

        }

        [TestMethod]
        public void RegistryWriteSubKeyTest()
        {
            var helper = new RegistryWrapper();

            helper.WriteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", 
                RegistryWrapper.RegistryVersion.Only64Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Key1", "Value1"),
                new KeyValuePair<string, object>("Key2 ", "Value2"),
                new KeyValuePair<string, object>("Key3", "Value3")
            });

            Assert.IsNotNull(helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only64Bit)._64BitValues,
                "No value at specified subKey after write attempt.");
            helper.DeleteSubKeyTree(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only64Bit);
        }

        [TestMethod]
        public void RegistryWriteSubKeyTo32Test()
        {
            var helper = new RegistryWrapper();

            helper.WriteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", 
                RegistryWrapper.RegistryVersion.Only32Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Key1", "Value1"),
                new KeyValuePair<string, object>("Key2 ", "Value2"),
                new KeyValuePair<string, object>("Key3", "Value3")
            });

            Assert.IsTrue(helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only32Bit)
                              ._32BitValues.Count > 0, "No value at specified subKey after write attempt.");
            helper.DeleteSubKeyTree(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only32Bit);
        }

        [TestMethod]
        public void RegistryWriteSubKeyWithLayersTest()
        {
            var helper = new RegistryWrapper();

            helper.WriteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test\Layer1\Layer2", 
                RegistryWrapper.RegistryVersion.Only64Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Key1", "Value1"),
                new KeyValuePair<string, object>("Key2 ", "Value2"),
                new KeyValuePair<string, object>("Key3", "Value3")
            });

            Assert.IsNotNull(helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test\Layer1\Layer2", RegistryWrapper.RegistryVersion.Only64Bit)
                    ._64BitValues,"No value at specified subKey after write attempt.");
            helper.DeleteSubKeyTree(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test\Layer1\Layer2", RegistryWrapper.RegistryVersion.Only64Bit);
        }

        [TestMethod]
        public void RegistryDeleteSubKeyTest()
        {
            var helper = new RegistryWrapper();
            helper.WriteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Charlie", RegistryWrapper.RegistryVersion.Only64Bit);
            helper.DeleteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Charlie", RegistryWrapper.RegistryVersion.Only64Bit);
            Assert.IsTrue(helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Charlie",
                RegistryWrapper.RegistryVersion.Only64Bit)._64BitValues.Count == 0);
        }

        [TestMethod]
        public void RegistryDeleteSubKeyTreeTest()
        {
            var helper = new RegistryWrapper();
            helper.WriteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Charlie", RegistryWrapper.RegistryVersion.Only64Bit);
            helper.WriteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Charlie\Test", RegistryWrapper.RegistryVersion.Only64Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Key1", "Value1")
            });
            helper.DeleteSubKeyTree(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Charlie", RegistryWrapper.RegistryVersion.Only64Bit);
            Assert.IsTrue(helper.ReadKey(@"SOFTWARE\Charlie", RegistryWrapper.RegistryVersion.Only64Bit)._64BitValues.Count == 0);
        }

        [TestMethod]
        public void RegistryDeleteValueTest()
        {
            var helper = new RegistryWrapper();
            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE",
                RegistryWrapper.RegistryVersion.Only64Bit, new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("TestKey", "TestValue")
            });

            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", "TestKey", RegistryWrapper.RegistryVersion.Only64Bit);
            Assert.IsFalse(helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine,
                @"SOFTWARE", RegistryWrapper.RegistryVersion.Only64Bit)
                    ._64BitValues.Contains(new KeyValuePair<string, object>("TestKey", "TestValue")),
                "Given key still contains value after delete attempt.");
        }

        [TestMethod]
        public void RegistryDeleteValueFrom32Test()
        {
            var helper = new RegistryWrapper();
            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", 
                RegistryWrapper.RegistryVersion.Only32Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("TestKey", "TestValue")
            });

            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", "TestKey", RegistryWrapper.RegistryVersion.Only32Bit);
            Assert.IsFalse(helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine,
                    @"SOFTWARE", RegistryWrapper.RegistryVersion.Only32Bit)
                    ._64BitValues.
                    Contains(new KeyValuePair<string, object>("TestKey", "TestValue")),
                "Given key still contains value after delete attempt.");
        }

        [TestMethod]
        public void RegistryDeleteValueFrom64Test()
        {
            var helper = new RegistryWrapper();
            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", 
                RegistryWrapper.RegistryVersion.Only64Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("TestKey", "TestValue")
            });

            helper.DeleteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE", "TestKey", RegistryWrapper.RegistryVersion.Only64Bit);
            Assert.IsFalse(helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine,
                        @"SOFTWARE", RegistryWrapper.RegistryVersion.Only64Bit)
                    ._64BitValues.
                    Contains(new KeyValuePair<string, object>("TestKey", "TestValue")),
                "Given key still contains value after delete attempt.");
        }

        [TestMethod]
        public void DeleteSubKeyWithSubKeyTreeTest()
        {
            var helper = new RegistryWrapper();

            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test\TestLayer2",
                RegistryWrapper.RegistryVersion.Only64Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Alice", "Allison"),
                new KeyValuePair<string, object>("Bob", "Boberson")
            });

            try { helper.DeleteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only64Bit); }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Threw expected exception because given key has subKeys.");
                helper.DeleteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test",
                RegistryWrapper.RegistryVersion.Only64Bit, true);
            }
            Assert.IsTrue(helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test", 
                              RegistryWrapper.RegistryVersion.Only64Bit)._64BitValues.Count == 0);
        }

        [TestMethod]
        public void DeleteSubKeyWithSubKeyTreeFrom32Test()
        {
            var helper = new RegistryWrapper();

            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test\TestLayer2", 
                RegistryWrapper.RegistryVersion.Only32Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Alice", "Allison"),
                new KeyValuePair<string, object>("Bob", "Boberson")
            });

            try { helper.DeleteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only32Bit); }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Threw expected exception because given key has subKeys.");
                helper.DeleteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test",
                    RegistryWrapper.RegistryVersion.Only32Bit, true);
            }
            Assert.IsTrue(helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only32Bit).
                _32BitValues.Count == 0);
        }

        [TestMethod]
        public void DeleteSubKeyWithSubKeyTreeFrom64Test()
        {
            var helper = new RegistryWrapper();

            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test\TestLayer2", 
                RegistryWrapper.RegistryVersion.Only64Bit,new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Alice", "Allison"),
                new KeyValuePair<string, object>("Bob", "Boberson")
            });

            try { helper.DeleteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only64Bit); }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Threw expected exception because given key has subKeys.");
                helper.DeleteSubKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only64Bit, true);
            }
            Assert.IsTrue(helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test", RegistryWrapper.RegistryVersion.Only64Bit).
                _64BitValues.Count == 0);
        }

        [TestMethod]
        public void RegistryWriteToBothTest()
        {
            var helper = new RegistryWrapper();

            helper.WriteValue(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test\TestLayer2",
                RegistryWrapper.RegistryVersion.Both,new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Alice", "Allison"),
                    new KeyValuePair<string, object>("Bob", "Boberson")
                });

            var result = helper.ReadKey(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test\TestLayer2",
                RegistryWrapper.RegistryVersion.Both);

            Assert.IsTrue(result._64BitValues.Contains(new KeyValuePair<string, object>("Alice", "Allison")));
            Assert.IsTrue(result._32BitValues.Contains(new KeyValuePair<string, object>("Alice", "Allison")));
            helper.DeleteSubKeyTree(RegistryWrapper.RegistryKeyRoot.LocalMachine, @"SOFTWARE\Test", RegistryWrapper.RegistryVersion.Both);
        }

        [TestMethod]
        public void GetSubKeyNamesTest()
        {
            var helper = new RegistryWrapper();

            var result = helper.ReadKey(@"HKEY_LOCAL_MACHINE\SOFTWARE", RegistryWrapper.RegistryVersion.Both);

            Assert.IsTrue(result._64BitSubKeyNames.Length > 0);
            Assert.IsTrue(result._32BitSubKeyNames.Length > 0);

            foreach (var item in result._64BitSubKeyNames)
            {
                Debug.WriteLine(item);
            }
            Debug.WriteLine("\n32 bit subkeys:\n");
            foreach (var item in result._32BitSubKeyNames)
            {
                Debug.WriteLine(item);
            }
        }
    }
}
