using Microsoft.VisualStudio.TestTools.UnitTesting;
using nitwitapi.Logic;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Logic
{
    [TestClass]
    public class MentionLogicTests
    {
        [TestMethod]
        public void GetMentionedUsernames()
        {
            // Arrange
            var testcases = GetTestCases();

            // Act + Assert
            foreach (var testcase in testcases)
            {
                var usernames = new MentionLogic().GetMentionedUsernames(testcase.Content).ToList();
                Assert.AreEqual(testcase.ExpectedUsernames.Count, usernames.Count());
                for (int i = 0; i < testcase.ExpectedUsernames.Count; i++)
                    Assert.AreEqual(testcase.ExpectedUsernames[i], usernames[i]);
            }
        }

        private IEnumerable<TestCase> GetTestCases()
        {
            return new List<TestCase>
            {
                new TestCase { Content = string.Empty, ExpectedUsernames = new List<string>() },
                new TestCase { Content = "hoi", ExpectedUsernames = new List<string>() },
                new TestCase { Content = "@henk", ExpectedUsernames = new List<string>() },
                new TestCase { Content = "@henk @piet", ExpectedUsernames = new List<string>() },
                new TestCase { Content = "@piet hoi @henk", ExpectedUsernames = new List<string> { "henk" } },
                new TestCase { Content = "hoi @henk", ExpectedUsernames = new List<string> { "henk" } },
                new TestCase { Content = "hoi @henk moio", ExpectedUsernames = new List<string> { "henk" } },
                new TestCase { Content = "hoi @henk moio @piet", ExpectedUsernames = new List<string> { "henk", "piet" } },
            };
        }
    }
}
