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
                new object[] { CreateNegativeNumber(), Fixture.Create<Guid>(), Fixture.Create<Guid>(), Fixture.Create<uint>() },
            };

        private static int CreateNegativeNumber()
        {
            var number = Fixture.Create<int>();

            return number >= 0
                ? -number
                : number;
        }
    }
}
