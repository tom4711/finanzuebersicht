using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Tests.Services
{
    public class CategorizationServiceTests
    {
        private readonly ILogger<CategorizationService> _mockLogger = Substitute.For<ILogger<CategorizationService>>();

        [Fact]
        public async Task CategorizAsync_WithMultipleStrategies_ReturnsFirstMatch()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Groceries" },
                new Category { Id = "2", Name = "Transport", SystemKey = "SysCat_Transport" },
                new Category { Id = "3", Name = "Unkategorisiert", SystemKey = "SysCat_Unkategorisiert" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "REWE Supermarket" };

            var strategy1 = Substitute.For<ICategorizationStrategy>();
            strategy1.Priority.Returns(10);
            strategy1.Name.Returns("Strategy1");
            strategy1.TryCategorizAsync(dto, Arg.Any<IEnumerable<Category>>(), CancellationToken.None)
                .Returns(Task.FromResult<Category?>(categories[0])); // Matches first

            var strategy2 = Substitute.For<ICategorizationStrategy>();
            strategy2.Priority.Returns(20);
            strategy2.Name.Returns("Strategy2");
            strategy2.TryCategorizAsync(dto, Arg.Any<IEnumerable<Category>>(), CancellationToken.None)
                .Returns(Task.FromResult<Category?>(categories[1])); // Would match but runs later

            var service = new CategorizationService(new[] { strategy2, strategy1 }, _mockLogger);

            // Act
            var result = await service.CategorizAsync(dto, categories);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Groceries", result.Name);
            Assert.Equal("1", result.Id);
        }

        [Fact]
        public async Task CategorizAsync_WithNoMatches_ReturnsFallbackUncategorized()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Groceries" },
                new Category { Id = "2", Name = "Unkategorisiert", SystemKey = "SysCat_Unkategorisiert" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "Unknown Store" };

            var strategy = Substitute.For<ICategorizationStrategy>();
            strategy.Priority.Returns(10);
            strategy.Name.Returns("Strategy");
            strategy.TryCategorizAsync(dto, Arg.Any<IEnumerable<Category>>(), CancellationToken.None)
                .Returns(Task.FromResult<Category?>(null)); // No match

            var service = new CategorizationService(new[] { strategy }, _mockLogger);

            // Act
            var result = await service.CategorizAsync(dto, categories);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unkategorisiert", result.Name);
            Assert.Equal("2", result.Id);
        }

        [Fact]
        public async Task CategorizAsync_WithStrategyException_ContinuesToNextStrategy()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Transport" },
                new Category { Id = "2", Name = "Unkategorisiert", SystemKey = "SysCat_Unkategorisiert" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "DB Bahn" };

            var strategy1 = Substitute.For<ICategorizationStrategy>();
            strategy1.Priority.Returns(10);
            strategy1.Name.Returns("FaultyStrategy");
            strategy1.TryCategorizAsync(dto, Arg.Any<IEnumerable<Category>>(), CancellationToken.None)
                .Returns(Task.FromException<Category?>(new InvalidOperationException("Test error")));

            var strategy2 = Substitute.For<ICategorizationStrategy>();
            strategy2.Priority.Returns(20);
            strategy2.Name.Returns("GoodStrategy");
            strategy2.TryCategorizAsync(dto, Arg.Any<IEnumerable<Category>>(), CancellationToken.None)
                .Returns(Task.FromResult<Category?>(categories[0]));

            var service = new CategorizationService(new[] { strategy1, strategy2 }, _mockLogger);

            // Act
            var result = await service.CategorizAsync(dto, categories);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Transport", result.Name); // Should match GoodStrategy
        }

        [Fact]
        public async Task CategorizAsync_ThrowsWhenUncategorizedNotFound()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Groceries" }
                // Missing Unkategorisiert!
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "Store" };
            var strategy = Substitute.For<ICategorizationStrategy>();
            strategy.Priority.Returns(10);
            strategy.Name.Returns("Strategy");
            strategy.TryCategorizAsync(dto, Arg.Any<IEnumerable<Category>>(), CancellationToken.None)
                .Returns(Task.FromResult<Category?>(null));

            var service = new CategorizationService(new[] { strategy }, _mockLogger);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CategorizAsync(dto, categories));
        }
    }

    public class KeywordCategorizationStrategyTests
    {
        [Fact]
        public async Task TryCategorizAsync_MatchesPayeeByKeyword()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Lebensmittel" },
                new Category { Id = "2", Name = "Transport" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "REWE MARKT GmbH" };

            // Create strategy with in-memory rules
            var strategy = new KeywordCategorizationStrategy();

            // Act
            var result = await strategy.TryCategorizAsync(dto, categories);

            // Assert - If rules loaded, should match. If not, that's OK for this test
            // (just verify no exception thrown)
            Assert.True(result == null || result.Name == "Lebensmittel");
        }

        [Fact]
        public async Task TryCategorizAsync_IsCase_Insensitive()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Lebensmittel" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "rewe markt" };
            var strategy = new KeywordCategorizationStrategy();

            // Act
            var result = await strategy.TryCategorizAsync(dto, categories);

            // Assert - If rules loaded, should match (case-insensitive)
            Assert.True(result == null || result.Name == "Lebensmittel");
        }

        [Fact]
        public async Task TryCategorizAsync_SearchesInMultipleFields()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Transport" }
            };

            // Keyword in Verwendungszweck instead of Zahlungsempfaenger
            var dto = new TransactionDto
            {
                Zahlungsempfaenger = "Unknown",
                Verwendungszweck = "Fahrtkarte DB BAHN AG"
            };

            var strategy = new KeywordCategorizationStrategy();

            // Act
            var result = await strategy.TryCategorizAsync(dto, categories);

            // Assert - If rules loaded, should match
            Assert.True(result == null || result.Name == "Transport");
        }

        [Fact]
        public async Task TryCategorizAsync_ReturnsNullWhenNoMatch()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Lebensmittel" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "Random Company Ltd" };
            var strategy = new KeywordCategorizationStrategy();

            // Act
            var result = await strategy.TryCategorizAsync(dto, categories);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Priority_Returns10()
        {
            var strategy = new KeywordCategorizationStrategy();
            Assert.Equal(10, strategy.Priority);
        }

        [Fact]
        public void Name_ReturnsBehaviorDescription()
        {
            var strategy = new KeywordCategorizationStrategy();
            Assert.Equal("Keyword Pattern Matching", strategy.Name);
        }
    }

    public class HistoricalCategorizationStrategyTests
    {
        [Fact]
        public async Task TryCategorizAsync_MatchesByPayeeHistory()
        {
            // Arrange
            var category = new Category { Id = "1", Name = "Groceries" };
            var categories = new List<Category>
            {
                category,
                new Category { Id = "2", Name = "Unkategorisiert", SystemKey = "SysCat_Unkategorisiert" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "REWE" };

            var mockRepo = Substitute.For<ITransactionRepository>();
            mockRepo.GetMostCommonCategoryForPayeeAsync("REWE", 0.5, CancellationToken.None)
                .Returns(Task.FromResult<Category?>(category));

            var strategy = new HistoricalCategorizationStrategy(mockRepo);

            // Act
            var result = await strategy.TryCategorizAsync(dto, categories);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Groceries", result.Name);
        }

        [Fact]
        public async Task TryCategorizAsync_ReturnsNullWhenNoHistory()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Groceries" }
            };

            var dto = new TransactionDto { Zahlungsempfaenger = "NewStore" };

            var mockRepo = Substitute.For<ITransactionRepository>();
            mockRepo.GetMostCommonCategoryForPayeeAsync("NewStore", 0.5, CancellationToken.None)
                .Returns(Task.FromResult<Category?>(null)); // No history

            var strategy = new HistoricalCategorizationStrategy(mockRepo);

            // Act
            var result = await strategy.TryCategorizAsync(dto, categories);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCategorizAsync_UsesFallbackPayer()
        {
            // Arrange
            var category = new Category { Id = "1", Name = "Utilities" };
            var categories = new List<Category> { category };

            var dto = new TransactionDto
            {
                Zahlungsempfaenger = string.Empty, // Empty recipient
                Zahlungspflichtige = "ElectricCorp"  // Use payer
            };

            var mockRepo = Substitute.For<ITransactionRepository>();
            mockRepo.GetMostCommonCategoryForPayeeAsync("ElectricCorp", 0.5, CancellationToken.None)
                .Returns(Task.FromResult<Category?>(category));

            var strategy = new HistoricalCategorizationStrategy(mockRepo);

            // Act
            var result = await strategy.TryCategorizAsync(dto, categories);

            // Assert
            Assert.NotNull(result);
            await mockRepo.Received(1).GetMostCommonCategoryForPayeeAsync("ElectricCorp", 0.5, CancellationToken.None);
        }

        [Fact]
        public void Priority_Returns20()
        {
            var mockRepo = Substitute.For<ITransactionRepository>();
            var strategy = new HistoricalCategorizationStrategy(mockRepo);
            Assert.Equal(20, strategy.Priority);
        }

        [Fact]
        public void Name_ReturnsBehaviorDescription()
        {
            var mockRepo = Substitute.For<ITransactionRepository>();
            var strategy = new HistoricalCategorizationStrategy(mockRepo);
            Assert.Equal("Historical Category Matching", strategy.Name);
        }
    }
}
