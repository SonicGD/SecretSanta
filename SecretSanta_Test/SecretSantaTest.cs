using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SecretSanta;

namespace SecretSanta_Test
{
    [TestClass]
    public class SecretSantaTest
    {
        private IList<Participant> _participants;
        private IDictionary<Participant, Participant> _banned;

        [TestInitialize]
        public void SetUp()
        {
            _participants = new List<Participant>()
            {
                new Participant() { FirstName = "A" },
                new Participant() { FirstName = "B" },
                new Participant() { FirstName = "C" },
                new Participant() { FirstName = "D" }
            };

            _banned = new Dictionary<Participant, Participant>
            {
                {_participants[0], _participants[2]}, {_participants[1], _participants[3]}
            };
        }

        [TestMethod]
        public void SecretSanta_Generate_ReturnsASet()
        {
            var result = SecretSantaGenerator.Generate(_participants);

            CheckForValidSantaList(result);
        }

        private void CheckForValidSantaList(IDictionary<Participant, Participant> santaList)
        {
            foreach (var sender in santaList.Keys)
            {
                Assert.IsTrue(_participants.Contains(sender), "A participant was not included as a gifter");
            }

            foreach (var receiver in santaList.Values)
            {
                Assert.IsTrue(_participants.Contains(receiver), "A participant was not included as a giftee");
            }

            foreach (var (key, value) in santaList)
            {
                Assert.AreNotEqual(key, value, "A participant should never have to gift to themselves");
            }
        }

        [TestMethod]
        public void SecretSanta_GenerateAll_ReturnsAllSets()
        {
            foreach (var list in SecretSantaGenerator.GenerateAll(_participants))
            {
                CheckForValidSantaList(list);
            }
        }

        [TestMethod]
        public void SecretSanta_Generate_WithBanned_ReturnsASet()
        {
            var result = SecretSantaGenerator.Generate(_participants, _banned);

            CheckForValidSantaList(result);
            CheckResultHasNoBannedPair(result);
        }

        private void CheckResultHasNoBannedPair(IDictionary<Participant, Participant> result)
        {
            foreach (var bannedPair in _banned)
            {
                Assert.IsFalse(result.Contains(bannedPair));
            }
        }

        [TestMethod]
        public void SecretSanta_GenerateAll_WithBanned_ReturnsAllSets()
        {
            var result = SecretSantaGenerator.GenerateAll(_participants, _banned);

            foreach (var list in result)
            {
                CheckForValidSantaList(list);
                CheckResultHasNoBannedPair(list);
            }
        }
    }
}
