﻿using Espera.Core.Analytics;
using Espera.Core.Settings;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Espera.Core.Tests
{
    public class AnalyticsClientTest
    {
        public class TheInitializeAsyncMethod
        {
            [Fact]
            public async Task Authenticates()
            {
                var coreSettings = new CoreSettings { EnableAutomaticReports = true, AnalyticsToken = "cooltoken" };

                var endpoint = Substitute.For<IAnalyticsEndpoint>();
                var client = new AnalyticsClient(endpoint);

                await client.InitializeAsync(coreSettings);

                Assert.True(client.IsAuthenticated);
                endpoint.Received().AuthenticateUserAsync(Arg.Any<string>());
            }

            [Fact]
            public async Task CreatesUserIfAnalyticsTokenIsNull()
            {
                var coreSettings = new CoreSettings { EnableAutomaticReports = true, AnalyticsToken = null };

                var endpoint = Substitute.For<IAnalyticsEndpoint>();
                var client = new AnalyticsClient(endpoint);

                await client.InitializeAsync(coreSettings);

                endpoint.Received().CreateUserAsync();
            }

            [Fact]
            public async Task DoesntAuthenticeWithoutPermission()
            {
                var coreSettings = new CoreSettings { EnableAutomaticReports = false };

                var client = new AnalyticsClient(Substitute.For<IAnalyticsEndpoint>());
                await client.InitializeAsync(coreSettings);

                Assert.False(client.IsAuthenticated);
            }

            [Fact]
            public async Task DoesntCreateUserIfAnalyticsTokenIsSaved()
            {
                var coreSettings = new CoreSettings { EnableAutomaticReports = true, AnalyticsToken = "cooltoken" };

                var endpoint = Substitute.For<IAnalyticsEndpoint>();
                var client = new AnalyticsClient(endpoint);

                await client.InitializeAsync(coreSettings);

                endpoint.DidNotReceive().CreateUserAsync();
            }

            [Fact]
            public async Task RecordDeviceInformation()
            {
                var coreSettings = new CoreSettings { EnableAutomaticReports = true, AnalyticsToken = null };

                var endpoint = Substitute.For<IAnalyticsEndpoint>();
                var client = new AnalyticsClient(endpoint);

                await client.InitializeAsync(coreSettings);

                endpoint.Received().RecordDeviceInformationAsync();
            }

            [Fact]
            public async Task StoresAnalyticsToken()
            {
                var coreSettings = new CoreSettings { EnableAutomaticReports = true, AnalyticsToken = null };

                var client = new AnalyticsClient(Substitute.For<IAnalyticsEndpoint>());

                await client.InitializeAsync(coreSettings);

                Assert.NotNull(coreSettings.AnalyticsToken);
            }
        }

        public class TheRecordCrashAsyncMethod
        {
            [Fact]
            public async Task AuthenticatesIfForced()
            {
                var coreSettings = new CoreSettings { EnableAutomaticReports = false };

                var endpoint = Substitute.For<IAnalyticsEndpoint>();
                var client = new AnalyticsClient(endpoint);

                await client.InitializeAsync(coreSettings);

                await client.RecordCrashAsync(new Exception(), true);

                Assert.True(client.IsAuthenticated);
                endpoint.ReceivedWithAnyArgs().RecordErrorAsync(null, null);
            }
        }
    }
}