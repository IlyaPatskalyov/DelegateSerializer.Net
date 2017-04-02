using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            delegateSerializer = DelegateSerializer.Create();
        }

        [Test]
        public void TestAdd()
        {
            Func<int, int, int> func = (a, b) => a + b;
            var serialized = delegateSerializer.Serialize(func.Method);
            Console.WriteLine(JsonConvert.SerializeObject(serialized, Formatting.Indented));

            var deFunc = delegateSerializer.Deserialize<Func<int, int, int>>(serialized);
            Assert.AreEqual(6, deFunc(1, 5));
        }

        [Test]
        public void TestAddManyParameters()
        {
            Func<int, int, int, int, int, int, int, int> func = (a, b, c, d, e, f, g) => a + b + c + d + e + f + g;
            var serialized = delegateSerializer.Serialize(func.Method);
            Console.WriteLine(JsonConvert.SerializeObject(serialized, Formatting.Indented));

            var deFunc = delegateSerializer.Deserialize<Func<int, int, int, int, int, int, int, int>>(serialized);
            Assert.AreEqual(28, deFunc(1, 2, 3, 4, 5, 6, 7));
        }

        [Test]
        public void TestCall()
        {
            Action<string> func = s => Console.WriteLine("Invoke {0}", s);
            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = delegateSerializer.Deserialize<Action<string>>(serialized);
            deFunc("test");
        }

        [Test]
        public void TestFunc()
        {
            Func<IEnumerable<int>, IEnumerable<int>> func = s => s.Where(t => t % 2 + 1 == 1).OrderBy(t => t);
            var serialized = delegateSerializer.Serialize(func.Method);

            Console.WriteLine(JsonConvert.SerializeObject(serialized, Formatting.Indented));
            var deFunc = delegateSerializer.DeserializeForDebug<Func<IEnumerable<int>, IEnumerable<int>>>(serialized);
            deFunc(new[] {1, 6, 2, 4, 3, 5});
        }


        private static Guid megaGuid = Guid.Empty;

        [Test]
        [Ignore("Not implemented")]
        public void TestClojureToField()
        {
            var data = Enumerable.Range(0, 10)
                                 .Select(_ => Guid.NewGuid())
                                 .ToArray();
            Func<IEnumerable<Guid>, IEnumerable<Guid>> func =
                s => s.Where(t => t != megaGuid).OrderBy(t => t.ToString());

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = delegateSerializer.Deserialize<Func<IEnumerable<Guid>, IEnumerable<Guid>>>(serialized);
            deFunc(data);
        }

        [Test]
        [Ignore("Not implemented")]
        [TestCase("a62f1421-7e4c-455c-a4ce-dae53430e341")]
        public void TestClojureToParameter(string value)
        {
            var data = Enumerable.Range(0, 10)
                                 .Select(_ => Guid.NewGuid())
                                 .ToArray();
            Func<IEnumerable<Guid>, IEnumerable<Guid>> func =
                s => s.Where(t => t.ToString() != value).OrderBy(t => t.ToString());

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc = delegateSerializer.Deserialize<Func<IEnumerable<Guid>, IEnumerable<Guid>>>(serialized);
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
            var deFunc = delegateSerializer.Deserialize<Func<IEnumerable<int>, IEnumerable<int>>>(serialized);
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
            var deFunc = delegateSerializer.Deserialize<Action>(serialized);

            deFunc();
        }


        [Test]
        [Ignore("Not implemented")]
        public void TestAnonymouseClass()
        {
            var data = Enumerable.Range(0, 10)
                                 .Select(_ => Guid.NewGuid())
                                 .ToArray();
            Func<Guid[], object[]> func = s => s.Select(t => new {Id = t, Length = t.ToString().Length}).ToArray();

            var serialized = delegateSerializer.Serialize(func.Method);
            var deFunc =
                (Func<Guid[], object[]>) delegateSerializer
                    .Deserialize<Func<IEnumerable<Guid>, IEnumerable<object>>>(serialized);
            deFunc(data);
        }
    }
}