using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NeoEAV.Data.DataClasses;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Attribute = NeoEAV.Data.DataClasses.Attribute;


namespace NeoEAVTests
{
    [Ignore]
    [TestClass]
    public class DataTests
    {
        Stopwatch timer = new Stopwatch();
        Dictionary<string, Random> rngSet = new Dictionary<string, Random>();

        public DataTests()
        {
            rngSet["Types"] = new Random(0);
            rngSet["Data"] = new Random(1);
            rngSet["Raw"] = new Random(2);
        }

        private EAVDataType GetNextType()
        {
            return((EAVDataType) rngSet["Types"].Next(1, 6));
        }

        private string GetNextValue(EAVDataType dataType)
        {
            if (rngSet["Raw"].NextDouble() >= 0.9)
                return(Guid.NewGuid().ToString());

            DateTime dtBase = new DateTime(1967, 4, 29);

            switch (dataType)
            {
                case EAVDataType.Boolean:
                    return (rngSet["Data"].NextDouble() >= 0.5 ? Boolean.TrueString : Boolean.FalseString);
                case EAVDataType.DateTime:
                    return (dtBase.AddSeconds(rngSet["Data"].Next(-315360000, 315360001)).ToString("O")); // +/- 10 years
                case EAVDataType.Float:
                    return ((rngSet["Data"].NextDouble() * Math.PI).ToString("R"));
                case EAVDataType.Integer:
                    return (rngSet["Data"].Next().ToString());
                case EAVDataType.String:
                    return (Guid.NewGuid().ToString().Replace("-", null));
                default:
                    return (null);
            }
        }

        private void StoreValueData(List<Dictionary<EAVDataType, string>> valueSet)
        {
            EAVEntityContext ctx = new EAVEntityContext();
            Subject subject = ctx.Subjects.Single(it => it.MemberID == "ETL Subject 1");
            Container container = ctx.Containers.Single(it => it.Name == "ETL Root Container");

            timer.Reset();
            timer.Start();
            int repeat = 0;
            foreach (var values in valueSet)
            {
                ContainerInstance instance = new ContainerInstance() { RepeatInstance = repeat++ };

                container.ContainerInstances.Add(instance);
                subject.ContainerInstances.Add(instance);

                foreach (var value in values)
                {
                    Attribute attribute = container.Attributes.Single(it => it.DataTypeID == value.Key);
                    Value v = new Value() { RawValue = value.Value };

                    attribute.Values.Add(v);
                    instance.Values.Add(v);
                }
            }
            Debug.WriteLine(String.Format("\t{0}", timer.Elapsed));

            //ctx.SaveChanges();
            Debug.WriteLine(String.Format("\t{0}", timer.Elapsed));
            timer.Stop();
        }

        private void StoreValue2Data(List<Dictionary<EAVDataType, string>> valueSet)
        {
            EAVEntityContext ctx = new EAVEntityContext();
            Subject subject = ctx.Subjects.Single(it => it.MemberID == "ETL Subject 2");
            Container container = ctx.Containers.Single(it => it.Name == "ETL Root Container");

            Boolean b;
            Int32 i;
            Single s;
            DateTime d;

            timer.Reset();
            timer.Start();
            int repeat = 0;
            foreach (var values in valueSet)
            {
                ContainerInstance instance = new ContainerInstance() { RepeatInstance = repeat++ };

                container.ContainerInstances.Add(instance);
                subject.ContainerInstances.Add(instance);

                foreach (var value in values)
                {
                    Attribute attribute = container.Attributes.Single(it => it.DataTypeID == value.Key);
                    Value2 v = new Value2() { RawValue = value.Value };

                    switch (value.Key)
                    {
                        case EAVDataType.Boolean:
                            v.BooleanValue = Boolean.TryParse(value.Value, out b) ? (Boolean?)b : null;
                            break;
                        case EAVDataType.DateTime:
                            v.DateTimeValue = DateTime.TryParse(value.Value, out d) ? (DateTime?)d : null;
                            break;
                        case EAVDataType.Float:
                            v.FloatValue = Single.TryParse(value.Value, out s) ? (Single?)s : null;
                            break;
                        case EAVDataType.Integer:
                            v.IntegerValue = Int32.TryParse(value.Value, out i) ? (Int32?)i : null;
                            break;
                    }

                    attribute.OtherValues.Add(v);
                    instance.OtherValues.Add(v);
                }
            }
            Debug.WriteLine(String.Format("\t{0}", timer.Elapsed));

            //ctx.SaveChanges();
            Debug.WriteLine(String.Format("\t{0}", timer.Elapsed));
            timer.Stop();
        }

        [TestMethod]
        public void TestInsertion()
        {
            // Create values
            List<Dictionary<EAVDataType, string>> valueSet = new List<Dictionary<EAVDataType, string>>();
            timer.Reset();
            timer.Start();
            for (int i = 0; i < 10000; ++i)
            {
                Dictionary<EAVDataType, string> values = new Dictionary<EAVDataType, string>();
                foreach (EAVDataType dataType in Enum.GetValues(typeof(EAVDataType)))
                {
                    values[dataType] = GetNextValue(dataType);
                }
                valueSet.Add(values);
            }
            timer.Stop();
            Debug.WriteLine(String.Format("Data creation - {0}", timer.Elapsed));

            // Create and assign to first value table type
            StoreValueData(valueSet);

            // Create and assign to second value table type
            StoreValue2Data(valueSet);
        }

        private TimeSpan ExtractValues(int iterations)
        {
            Random rng = new Random(3);
            EAVEntityContext ctx = new EAVEntityContext();
            Subject subject = ctx.Subjects.Single(it => it.MemberID == "ETL Subject 1"); // 11, 12
            Container container = ctx.Containers.Single(it => it.Name == "ETL Root Container"); // 6

            timer.Reset();
            timer.Start();
            for (int i = 0; i < iterations; ++i)
            {
                int ri = rng.Next(0, 10000);
                ContainerInstance instance = subject.ContainerInstances.Single(it => it.Container == container && it.RepeatInstance == ri);

                foreach (Value value in instance.Values)
                {
                    string raw = value.RawValue;
                    object obj = value.ObjectValue;
                }
            }
            timer.Stop();

            return (timer.Elapsed);
        }

        private TimeSpan ExtractValues2(int iterations)
        {
            Random rng = new Random(3);
            EAVEntityContext ctx = new EAVEntityContext();
            Subject subject = ctx.Subjects.Single(it => it.MemberID == "ETL Subject 2"); // 11, 12
            Container container = ctx.Containers.Single(it => it.Name == "ETL Root Container"); // 6

            timer.Reset();
            timer.Start();
            for (int i = 0; i < iterations; ++i)
            {
                int ri = rng.Next(0, 10000);
                ContainerInstance instance = subject.ContainerInstances.Single(it => it.Container == container && it.RepeatInstance == ri);

                foreach (Value2 value in instance.OtherValues)
                {
                    string raw = value.RawValue;
                    object obj = value.ObjectValue;
                }
            }
            timer.Stop();

            return (timer.Elapsed);
        }

        [TestMethod]
        public void TestExtraction()
        {
            int[] its = { 100, 250, 500, 750, 1000, 2500, 5000};

            foreach (int it in its)
            {
                TimeSpan smallest = TimeSpan.MaxValue;
                TimeSpan largest = TimeSpan.MinValue;
                TimeSpan sum = TimeSpan.FromTicks(0);

                for (int i = 0; i < 10; ++i)
                {
                    TimeSpan t1 = ExtractValues(it);
                    TimeSpan t2 = ExtractValues2(it);

                    TimeSpan diff = t2 - t1;

                    if (diff < smallest) smallest = diff;
                    if (diff > largest) largest = diff;

                    sum += diff;
                }

                TimeSpan avg = TimeSpan.FromTicks(sum.Ticks / 10);

                Debug.WriteLine(String.Format("Over 10 iterations of extracting {0} random values, the average time differential was {1} [{2} - {3}]", it, avg, smallest, largest));
            }
        }
    }
}
