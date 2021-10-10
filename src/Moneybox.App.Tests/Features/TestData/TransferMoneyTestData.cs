using System;
using System.Collections.Generic;

using AutoFixture;

namespace Moneybox.App.Tests.Features.TestData
{
    public record TransferMoneyTestData
    {
        private static readonly Fixture Fixture;

        static TransferMoneyTestData()
        {
            Fixture = new Fixture();
        }

        public static IEnumerable<object[]> NoBalanceAccountCase =>
            new List<object[]>
            {
                new object[] { -1, Fixture.Create<Guid>(), Fixture.Create<Guid>(), Fixture.Create<uint>() },
                new object[] { -10, Fixture.Create<Guid>(), Fixture.Create<Guid>(), Fixture.Create<uint>() },
            };
    }
}
