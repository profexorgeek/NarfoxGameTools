using Microsoft.VisualStudio.TestTools.UnitTesting;
using NarfoxGameTools.Services;
using System;

namespace NarfoxGameToolsTest
{
    public enum TestEnum
    {
        Unknown,
        Value1,
        Value2
    };

    public class TestModel
    {
        public int IntegerData { get; set; }
        public string StringData { get; set; }
        public float FloatData { get; set; }
        public TestEnum EnumValue1 { get; set; }
        public TestEnum EnumValue2 { get; set; }
    }

    [TestClass]
    public class TestFileService
    {
        [TestMethod]
        public void TestSerialization()
        {
            var model1 = new TestModel
            {
                IntegerData = 5,
                StringData = "Hello world",
                FloatData = 123.456f,
                EnumValue1 = TestEnum.Value1,
                EnumValue2 = TestEnum.Value2
            };

            var json = FileService.Instance.Serialize(model1);
            var model2 = FileService.Instance.Deserialize<TestModel>(json);

            Assert.AreEqual(model1.IntegerData, model2.IntegerData);
            Assert.AreEqual(model1.StringData, model2.StringData);
            Assert.AreEqual(model1.FloatData, model2.FloatData);
            Assert.AreEqual(model1.EnumValue1, model2.EnumValue1);
            Assert.AreEqual(model1.EnumValue2, model2.EnumValue2);
        }

    }
}
