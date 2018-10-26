using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecretSanta.Extentions;

namespace SecretSanta_Test
{
    [TestClass]
    public class SecretSantaExtensionsTest
    {
        private IList<Participant> _testList;

        [TestInitialize]
        public void SetUp()
        {
            _testList = new List<Participant>
            {
                new Participant() {FirstName = "Name 1", LastName = "Last"},
                new Participant() {FirstName = "Name 2", LastName = "Last"},
                new Participant() {FirstName = "Name 3", LastName = "Last"},
                new Participant() {FirstName = "Name 4", LastName = "Last"},
                new Participant() {FirstName = "Name 5", LastName = "Last"}
            };
        }

        [TestMethod]
        public void Helpers_GetShuffle_AllReturned_1000Tries()
        {
            for (var i = 0; i < 1000; i++)
            {
                var result = _testList.GetShuffle();

                foreach (var a in _testList)
                {
                    Assert.IsTrue(result.Contains(a));
                }
            }
        }

        [TestMethod]
        public void Helpers_GetPermutations_AllPermutationsReturned()
        {
            var result = _testList.GetPermutations().Count();
            var expected = Factorial(_testList.Count());

            Assert.AreEqual(expected, result, "There should be n! permutations, where n = {0}", _testList.Count());
        }

        private static int Factorial(int n)
        {
            if (n <= 1)
                return 1;

            return n * Factorial(n - 1);
        }

        [TestMethod]
        public void Helpers_GetPermutations_AllUnique()
        {
            var result = _testList.GetPermutations().ToList();

            for (var current = 0; current < result.Count; current++)
            {
                for (var compare = current + 1; compare < result.Count; compare++)
                {
                    Assert.AreEqual(result[current].Count, result[compare].Count,
                        "All lists should have the same number of elements");
                    CheckOrderingIsDifferent(result[current], result[compare]);
                }
            }
        }

        private static void CheckOrderingIsDifferent<T>(IList<T> first, IList<T> second)
        {
            var differenceDetected = first.Where((t, i) => t.Equals(second[i])).Any();

            Assert.IsTrue(differenceDetected, "No difference was found");
        }

        [TestMethod]
        public void Helpers_ToDictionary_ReturnsDictionary()
        {
            var pairs = GetEnumKVPairs();
            var result = pairs.ToDictionary();

            Assert.AreEqual(pairs.Count(), result.Count);

            foreach (var pair in pairs)
            {
                Assert.IsTrue(result.Contains(pair));
            }
        }

        private IEnumerable<KeyValuePair<Participant, Participant>> GetEnumKVPairs()
        {
            for (var i = 0; i < _testList.Count; i++)
            {
                if (i < _testList.Count - 1)
                {
                    yield return new KeyValuePair<Participant, Participant>(_testList[i], _testList[i + 1]);
                }
                else
                {
                    yield return new KeyValuePair<Participant, Participant>(_testList[i], _testList[0]);
                }
            }
        }

        [TestMethod]
        public void Helpers_ZipToKV_ReturnsValidZip()
        {
            var numberList = new List<int>() {1, 2, 3, 4, 5};
            var result = numberList.ZipToKV(numberList);

            Assert.AreEqual(numberList.Count, result.Count(), "Zipped list should eb same length");

            foreach (var (key, value) in result)
            {
                Assert.AreEqual(key, value, "Values did not match");
            }
        }
    }
}