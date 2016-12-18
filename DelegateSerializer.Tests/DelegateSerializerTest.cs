using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DelegateSerializer.Tests
{
    public class DelegateSerializerTest
    {
        private DelegateSerializer delegateSerializer;

        [SetUp]
        public void SetUp()
        {
            delegateSerializer = new DelegateSerializer(new TypeResolver());
        }

        [Test]
        public void TestAdd()
        {
            Func<int, int, int> func = (a, b) => a + b;
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Func<int, int, int>) delegateSerializer.Deserialize<Func<int, int, int>>(serialized);
            Assert.AreEqual(6, deFunc(1, 5));
        }

        [Test]
        public void TestCall()
        {
            Action<string> func =  s => Console.WriteLine("Invoke {0}", s);
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Action<string>)delegateSerializer.Deserialize<Action<string>>(serialized);
            deFunc("test");
        }

        [Test]
        public void TestLambda()
        {
            Func<IEnumerable<int>, IEnumerable<int>> func = s => s.Where(t => t%2 + 1 == 1).OrderBy(t => t);
            //Func<IEnumerable<int>, IEnumerable<int>> func = s => s.Select(t => t + 1);
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Func<IEnumerable<int>, IEnumerable<int>>)delegateSerializer.Deserialize<Func<IEnumerable<int>, IEnumerable<int>>>(serialized);
            deFunc(new[] { 1, 6, 2, 4, 3, 5 });

/*
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("asm.dll"),
                AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(Guid.NewGuid().ToString(), "asm.dll");
            var typeBuilder = moduleBuilder.DefineType(
                string.Format("{0}", Guid.NewGuid()), TypeAttributes.Public, null);


            methodSerializer.BuildMethod(typeBuilder, "Test", serialized, moduleBuilder);
            var type = typeBuilder.CreateType();
            assemblyBuilder.Save(@"asm.dll");
            
            Console.WriteLine(new XmlSerializer().SerializeToString(result));*/

            
        }


        private static Guid megaGuid = Guid.Empty;
        [Test]
        public void TestClojure()
        {
            var guids = new[]
                        {
                            Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.Empty, Guid.NewGuid()
                        };
            //var megaGuid = Guid.Empty;
            Func<IEnumerable<Guid>, IEnumerable<Guid>> func =
                s => s.Where(t => t != megaGuid).OrderBy(t => t.ToString());

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Func<IEnumerable<Guid>, IEnumerable<Guid>>)delegateSerializer.Deserialize<Func<IEnumerable<Guid>, IEnumerable<Guid>>>(serialized);
            deFunc(guids);
        }

        [Test]
        public void TestClojure2()
        {
            TestClojure3(Guid.Empty);
        }

        private void TestClojure3(Guid megaGuid2)
        {
            var guids = new[]
                        {
                            Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.Empty, Guid.NewGuid()
                        };
            Func<IEnumerable<Guid>, IEnumerable<Guid>> func =
                s => s.Where(t => t != megaGuid2).OrderBy(t => t.ToString());

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc =
                (Func<IEnumerable<Guid>, IEnumerable<Guid>>)
                delegateSerializer.Deserialize<Func<IEnumerable<Guid>, IEnumerable<Guid>>>(serialized);
            deFunc(guids);
        }

        [Test]
        public void TestForeachAndConstructor()
        {
            Func<IEnumerable<int>, IEnumerable<int>> func = s =>
                                                            {
                                                                var list = new List<int>();
                                                                foreach (var i in s)
                                                                    list.Add(i + 1);
                                                                return list;
                                                            };
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Func<IEnumerable<int>, IEnumerable<int>>)delegateSerializer.Deserialize<Func<IEnumerable<int>, IEnumerable<int>>>(serialized);
            deFunc(new[] { 1, 6, 2, 4, 3, 5 });

        }


        [Test]
        public IEnumerable<int> TestMethod()
        {
            for (var i = 0; i < 5; i++)
                yield return i + 1;
        }

        [Test]
        public void TestExceptions()
        {
            Action func = () =>
                          {
                              try
                              {
                                  Console.WriteLine("Try");
                              }
                              catch (IOException)
                              {
                                  Console.WriteLine("IOException");
                              }
                              catch (Exception)
                              {
                                  Console.WriteLine("Exception");
                              }
                              catch
                              {
                                  Console.WriteLine("Object");
                              }
                              finally
                              {
                                  Console.WriteLine("Finnaly");
                              }
                          };
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Action)delegateSerializer.Deserialize<Action>(serialized);
            
            deFunc();
        }



        [Test]
        public void TestAnonymouseClass()
        {
            var guids = new[]
                        {
                            Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.Empty, Guid.NewGuid()
                        };
            //var megaGuid = Guid.Empty;
            Func<IEnumerable<Guid>, IEnumerable<object>> func =
                s => s.Select(t => new {Id = t, Length = t.ToString().Length});

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Func<IEnumerable<Guid>, IEnumerable<object>>)delegateSerializer.Deserialize<Func<IEnumerable<Guid>, IEnumerable<object>>>(serialized);
            deFunc(guids);
        }


        public object CreateGuid(Guid g)
        {
            return new { Id = g, Length = g.ToString().Length };
        }

        [Test]
        public void TestAnonymouseClass2()
        {
            Func<Guid, object> func = CreateGuid;
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Func<Guid, object>)delegateSerializer.Deserialize<Func<Guid, object>>(serialized);
            deFunc(Guid.NewGuid());
        }
    }
}