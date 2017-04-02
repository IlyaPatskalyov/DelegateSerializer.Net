using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelegateSerializer.DataBuilders;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DelegateSerializer.Tests
{
    public class DelegateSerializerTest
    {
        private DelegateSerializer delegateSerializer;

        [SetUp]
        public void SetUp()
        {
            var typeResolver = new TypeResolver();
            var typeInfoDataBuilder = new TypeInfoDataBuilder();
            delegateSerializer = new DelegateSerializer(typeResolver,
                                                        typeInfoDataBuilder,
                                                        new MethodInfoDataBuilder(typeInfoDataBuilder),
                                                        new ExceptionHandlingClauseDataBuilder(typeInfoDataBuilder, typeResolver),
                                                        new ConstructorInfoDataBuilder(typeInfoDataBuilder),
                                                        new LocalVariableInfoDataBuilder(typeInfoDataBuilder));
        }

        [Test]
        public void TestAdd()
        {
            Func<int, int, int> func = (a, b) => a + b;
            var serialized = delegateSerializer.Serialize(func.Method);
            Console.WriteLine(JsonConvert.SerializeObject(serialized, Formatting.Indented));

            var deFunc = (Func<int, int, int>) delegateSerializer.Deserialize<Func<int, int, int>>(serialized);
            Assert.AreEqual(6, deFunc(1, 5));
        }

        [Test]
        public void TestMegaAdd()
        {
            Func<int, int, int, int, int, int, int, int> func = (a, b, c, d, e, f, g) => a + b + c + d + e + f + g;
            var serialized = delegateSerializer.Serialize(func.Method);
            Console.WriteLine(JsonConvert.SerializeObject(serialized, Formatting.Indented));

            var deFunc =
                (Func<int, int, int, int, int, int, int, int>) delegateSerializer
                    .Deserialize<Func<int, int, int, int, int, int, int, int>>(serialized);
            Assert.AreEqual(28, deFunc(1, 2, 3, 4, 5, 6, 7));
        }

        [Test]
        public void TestCall()
        {
            Action<string> func = s => Console.WriteLine("Invoke {0}", s);
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Action<string>) delegateSerializer.Deserialize<Action<string>>(serialized);
            deFunc("test");
        }

        [Test]
        public void TestLambda()
        {
            Func<IEnumerable<int>, IEnumerable<int>> func = s => s.Where(t => t % 2 + 1 == 1).OrderBy(t => t);
            var serialized = delegateSerializer.Serialize(func.Method);

            Console.WriteLine(JsonConvert.SerializeObject(serialized, Formatting.Indented));
            delegateSerializer.Deserialize2(serialized);
            var deFunc =
                (Func<IEnumerable<int>, IEnumerable<int>>) delegateSerializer.Deserialize<Func<IEnumerable<int>, IEnumerable<int>>>(serialized);
            deFunc(new[] {1, 6, 2, 4, 3, 5});
        }


        private static Guid megaGuid = Guid.Empty;

        [Test]
        public void TestClojure()
        {
            var guids = new[]
                        {
                            Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.Empty, Guid.NewGuid()
                        };
            Func<IEnumerable<Guid>, IEnumerable<Guid>> func =
                s => s.Where(t => t != megaGuid).OrderBy(t => t.ToString());

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc =
                (Func<IEnumerable<Guid>, IEnumerable<Guid>>) delegateSerializer.Deserialize<Func<IEnumerable<Guid>, IEnumerable<Guid>>>(serialized);
            deFunc(guids);
        }

        [Test]
        public void TestClojure2()
        {
            TestClojure3(Guid.Empty);
        }

        private void TestClojure3(Guid megaGuid2)
        {
            var data = Enumerable.Range(0, 10)
                                 .Select(_ => Guid.NewGuid())
                                 .ToArray();
            Func<IEnumerable<Guid>, IEnumerable<Guid>> func =
                s => s.Where(t => t != megaGuid2).OrderBy(t => t.ToString());

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc =
                (Func<IEnumerable<Guid>, IEnumerable<Guid>>)
                delegateSerializer.Deserialize<Func<IEnumerable<Guid>, IEnumerable<Guid>>>(serialized);
            deFunc(data);
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
            var deFunc =
                (Func<IEnumerable<int>, IEnumerable<int>>) delegateSerializer.Deserialize<Func<IEnumerable<int>, IEnumerable<int>>>(serialized);
            deFunc(new[] {1, 6, 2, 4, 3, 5});
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
#pragma warning disable 1058
                              catch
#pragma warning restore 1058
                              {
                                  Console.WriteLine("Object");
                              }
                              finally
                              {
                                  Console.WriteLine("Finnaly");
                              }
                          };
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Action) delegateSerializer.Deserialize<Action>(serialized);

            deFunc();
        }


        [Test]
        public void TestAnonymouseClass()
        {
            var data = Enumerable.Range(0, 10)
                                 .Select(_ => Guid.NewGuid())
                                 .ToArray();
            Func<IEnumerable<Guid>, IEnumerable<object>> func =
                s => s.Select(t => new {Id = t, Length = t.ToString().Length});

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc =
                (Func<IEnumerable<Guid>, IEnumerable<object>>) delegateSerializer
                    .Deserialize<Func<IEnumerable<Guid>, IEnumerable<object>>>(serialized);
            deFunc(data);
        }


        public object CreateGuid(Guid g)
        {
            return new {Id = g, Length = g.ToString().Length};
        }

        [Test]
        public void TestAnonymouseClass2()
        {
            Func<Guid, object> func = CreateGuid;
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = (Func<Guid, object>) delegateSerializer.Deserialize<Func<Guid, object>>(serialized);
            deFunc(Guid.NewGuid());
        }
    }
}