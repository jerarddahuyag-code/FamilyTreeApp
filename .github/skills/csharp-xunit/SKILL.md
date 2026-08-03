---
name: csharp-xunit
description: 'Best practices for XUnit unit testing in C# .NET, including data-driven tests, mocking, and test organization. Use this skill whenever the user is writing unit tests in C#, creating a test project, working with XUnit, adding test coverage, writing Theory or Fact tests, using InlineData or MemberData, mocking with Moq or NSubstitute, or asking about test patterns in .NET. Also use when the user mentions test-driven development (TDD), test fixtures, test assertions, or code coverage in a C#/.NET context.'
---

# XUnit Best Practices for C# .NET

Your goal is to help write effective, maintainable unit tests with XUnit in C# .NET projects. This covers project setup, standard tests, data-driven testing, assertions, mocking, and organization.

## Project Setup

### Creating a Test Project

Use `dotnet new xunit` to scaffold a test project rather than manually creating one. Follow the naming convention `[ProjectName].Tests` (e.g., `MyApp.Tests` for a project called `MyApp`).

```bash
dotnet new xunit -n MyApp.Tests
cd MyApp.Tests
dotnet add reference ../MyApp/MyApp.csproj
```

### Required Packages

The `dotnet new xunit` template includes the essentials, but verify these packages are present. Add any that are missing:

- `Microsoft.NET.Test.Sdk` — the test platform host
- `xunit` — the test framework itself
- `xunit.runner.visualstudio` — enables test discovery in Visual Studio and `dotnet test`

For mocking and richer assertions, also consider:

```bash
dotnet add package Moq
dotnet add package FluentAssertions
```

### Running Tests

```bash
dotnet test                              # Run all tests
dotnet test --filter "Category=Unit"     # Run by trait
dotnet test --filter "FullyQualifiedName~CalculatorTests"  # Run by class name
dotnet test --logger "console;verbosity=detailed"          # Verbose output
```

## Test Structure

XUnit is intentionally minimal — no `[TestClass]` attribute is needed. A public class with `[Fact]` or `[Theory]` methods is automatically discovered.

### The AAA Pattern

Structure every test as **Arrange → Act → Assert**. This makes tests easy to read at a glance and diagnose when they fail.

```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(2, 3);

        // Assert
        Assert.Equal(5, result);
    }
}
```

### Naming Convention

Name tests using the pattern: **MethodName_Scenario_ExpectedBehavior**

This convention communicates intent clearly in test output. When a test fails, the name alone should tell you what went wrong.

**Examples:**
- `Add_TwoPositiveNumbers_ReturnsSum`
- `Divide_ByZero_ThrowsDivideByZeroException`
- `GetUser_WithInvalidId_ReturnsNull`
- `CreateOrder_WhenInventoryInsufficient_ThrowsOutOfStockException`

### Setup and Teardown

Use the constructor for setup and `IDisposable.Dispose()` for teardown — XUnit creates a **new instance** of the test class for every test, so constructor-based setup is inherently isolated.

```csharp
public class OrderServiceTests : IDisposable
{
    private readonly OrderService _sut;
    private readonly Mock<IOrderRepository> _mockRepo;

    public OrderServiceTests()
    {
        // Runs before each test
        _mockRepo = new Mock<IOrderRepository>();
        _sut = new OrderService(_mockRepo.Object);
    }

    public void Dispose()
    {
        // Runs after each test — clean up unmanaged resources if needed
    }

    [Fact]
    public void PlaceOrder_ValidOrder_SavesSuccessfully()
    {
        // Arrange
        var order = new Order { Id = 1, Total = 99.99m };
        _mockRepo.Setup(r => r.Save(order)).Returns(true);

        // Act
        var result = _sut.PlaceOrder(order);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.Save(order), Times.Once);
    }
}
```

### Async Setup and Teardown

For async initialization, implement `IAsyncLifetime` instead of using the constructor:

```csharp
public class DatabaseTests : IAsyncLifetime
{
    private TestDatabase _db;

    public async Task InitializeAsync()
    {
        _db = await TestDatabase.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.CleanupAsync();
    }
}
```

## Standard Tests with `[Fact]`

Use `[Fact]` for tests that have no parameters and verify a single behavior.

**Key principles:**
- One logical assertion per test — if a test fails, you should know exactly which behavior broke
- Tests must be independent — they can run in any order and produce the same result
- Avoid test interdependencies (one test setting up state for another)

```csharp
[Fact]
public void ParseEmail_ValidEmail_ReturnsEmailParts()
{
    var parser = new EmailParser();

    var result = parser.Parse("user@example.com");

    Assert.Equal("user", result.LocalPart);
    Assert.Equal("example.com", result.Domain);
}

[Fact]
public void ParseEmail_MissingAtSign_ThrowsFormatException()
{
    var parser = new EmailParser();

    Assert.Throws<FormatException>(() => parser.Parse("invalid-email"));
}
```

Multiple `Assert` calls that verify different facets of the same behavior are fine (e.g., checking both `LocalPart` and `Domain` above). The goal is one *logical behavior* per test, not literally one `Assert` call.

## Data-Driven Tests with `[Theory]`

Use `[Theory]` when the same test logic applies to multiple inputs. This eliminates copy-paste tests and makes it easy to add new cases.

### `[InlineData]` — For Simple Inline Values

Best for primitive types and small data sets:

```csharp
[Theory]
[InlineData(2, 3, 5)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
[InlineData(int.MaxValue, 1, unchecked(int.MaxValue + 1))]
public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
{
    var calculator = new Calculator();

    var result = calculator.Add(a, b);

    Assert.Equal(expected, result);
}
```

### `[MemberData]` — For Complex or Reusable Data

Use when test data is too complex for `[InlineData]`, needs to include objects, or should be shared across tests:

```csharp
public class StringFormatterTests
{
    public static IEnumerable<object[]> FormatTestData =>
        new List<object[]>
        {
            new object[] { "hello world", "Title", "Hello World" },
            new object[] { "HELLO WORLD", "Lower", "hello world" },
            new object[] { "Hello World", "Upper", "HELLO WORLD" },
        };

    [Theory]
    [MemberData(nameof(FormatTestData))]
    public void Format_VariousCases_ReturnsExpected(
        string input, string formatType, string expected)
    {
        var formatter = new StringFormatter();

        var result = formatter.Format(input, formatType);

        Assert.Equal(expected, result);
    }
}
```

You can also reference a method from another class:

```csharp
[Theory]
[MemberData(nameof(TestDataProvider.GetEdgeCases), MemberType = typeof(TestDataProvider))]
public void Process_EdgeCases_HandlesGracefully(string input, bool expected) { ... }
```

### `[ClassData]` — For Complex Data Generation

Use when test data requires its own logic, constructor injection, or is shared across many test classes:

```csharp
public class ValidEmailTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "user@example.com" };
        yield return new object[] { "user.name+tag@domain.co.uk" };
        yield return new object[] { "user@subdomain.domain.com" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(ValidEmailTestData))]
public void Validate_ValidEmails_ReturnsTrue(string email)
{
    var validator = new EmailValidator();

    Assert.True(validator.IsValid(email));
}
```

### Choosing the Right Data Source

| Attribute | Use When |
|---|---|
| `[InlineData]` | Small number of primitive-type test cases |
| `[MemberData]` | Data involves objects, needs reuse, or is generated by a method |
| `[ClassData]` | Data logic is complex, needs its own class, or is shared widely |

## Assertions

### Value and Reference Equality

```csharp
Assert.Equal(expected, actual);           // Value equality (uses .Equals())
Assert.NotEqual(unexpected, actual);
Assert.Same(expectedRef, actualRef);      // Reference equality (same object)
Assert.NotSame(otherRef, actualRef);
```

### Boolean and Null Checks

```csharp
Assert.True(condition);
Assert.False(condition);
Assert.Null(value);
Assert.NotNull(value);
```

### Collections

```csharp
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);                // Asserts exactly one element
Assert.All(collection, item => Assert.True(item.IsActive));  // Assert on every element
```

### Strings

```csharp
Assert.Contains("substring", actualString);
Assert.StartsWith("prefix", actualString);
Assert.EndsWith("suffix", actualString);
Assert.Matches(@"^\d{3}-\d{4}$", actualString);        // Regex match
Assert.DoesNotMatch(@"[<>]", actualString);
```

### Exceptions

```csharp
// Synchronous
var ex = Assert.Throws<ArgumentNullException>(() => service.Process(null));
Assert.Equal("paramName", ex.ParamName);

// Asynchronous
var ex = await Assert.ThrowsAsync<InvalidOperationException>(
    () => service.ProcessAsync(invalidInput));
Assert.Contains("not allowed", ex.Message);
```

### Type Assertions

```csharp
Assert.IsType<SpecificType>(result);       // Exact type
Assert.IsAssignableFrom<IBase>(result);    // Type or subtype
```

### Ranges and Precision

```csharp
Assert.InRange(value, low: 1, high: 100);
Assert.Equal(3.14, actual, precision: 2);  // Floating-point with tolerance
```

## Mocking and Isolation

Use mocking to isolate the unit under test from its dependencies. This skill uses **Moq** in examples, but the same principles apply to NSubstitute or any mocking framework.

### Basic Moq Usage

```csharp
public class NotificationServiceTests
{
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _mockEmailSender = new Mock<IEmailSender>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _sut = new NotificationService(_mockEmailSender.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SendNotification_ValidRecipient_SendsEmail()
    {
        // Arrange
        var notification = new Notification
        {
            Recipient = "user@example.com",
            Subject = "Test",
            Body = "Hello"
        };

        _mockEmailSender
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _sut.SendAsync(notification);

        // Assert
        _mockEmailSender.Verify(
            s => s.SendAsync(notification.Recipient, notification.Subject, notification.Body),
            Times.Once);
    }

    [Fact]
    public async Task SendNotification_SendFails_LogsError()
    {
        // Arrange
        _mockEmailSender
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new SmtpException("Connection refused"));

        var notification = new Notification
        {
            Recipient = "user@example.com",
            Subject = "Test",
            Body = "Hello"
        };

        // Act & Assert
        await Assert.ThrowsAsync<SmtpException>(() => _sut.SendAsync(notification));
    }
}
```

### Moq Argument Matchers

```csharp
_mock.Setup(s => s.Get(It.IsAny<int>()));              // Any int
_mock.Setup(s => s.Get(It.Is<int>(x => x > 0)));       // Positive int
_mock.Setup(s => s.Get(It.IsInRange(1, 10, Range.Inclusive))); // 1–10
_mock.Setup(s => s.Find(It.IsRegex(@"^\d+$")));        // Regex match
```

### Design for Testability

Depend on interfaces rather than concrete classes. This makes mocking straightforward and keeps tests fast:

```csharp
// ✅ Good — easily mockable
public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;
}

// ❌ Bad — hard to isolate
public class OrderService
{
    private readonly SqlOrderRepository _repo = new();
}
```

## Shared Test Context (Fixtures)

### `IClassFixture<T>` — Shared Within a Single Test Class

Use when setup is expensive (e.g., database connection, HTTP client) and can be safely shared across tests in one class:

```csharp
public class DatabaseFixture : IDisposable
{
    public IDbConnection Connection { get; }

    public DatabaseFixture()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();
        // Run migrations, seed data, etc.
    }

    public void Dispose() => Connection.Dispose();
}

public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetUser_ExistingId_ReturnsUser()
    {
        var repo = new UserRepository(_fixture.Connection);
        var user = repo.GetById(1);
        Assert.NotNull(user);
    }
}
```

### `ICollectionFixture<T>` — Shared Across Multiple Test Classes

Use when several test classes need the same expensive resource:

```csharp
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database")]
public class UserRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    public UserRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void GetUser_ExistingId_ReturnsUser() { ... }
}

[Collection("Database")]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    public OrderRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void GetOrder_ValidId_ReturnsOrder() { ... }
}
```

## Test Organization and Categorization

### File Structure

Mirror the source project's structure in the test project so it's obvious which tests cover which code:

```
MyApp/
├── Services/
│   └── OrderService.cs
├── Models/
│   └── Order.cs

MyApp.Tests/
├── Services/
│   └── OrderServiceTests.cs
├── Models/
│   └── OrderTests.cs
```

### Traits for Categorization

Use `[Trait]` to tag tests for filtered execution:

```csharp
[Fact]
[Trait("Category", "Unit")]
public void Add_SimpleValues_ReturnsSum() { ... }

[Fact]
[Trait("Category", "Integration")]
public void SaveOrder_ToDatabase_PersistsSuccessfully() { ... }
```

Run only specific categories:

```bash
dotnet test --filter "Category=Unit"
```

### Skipping Tests

Use the `Skip` property when a test needs to be temporarily disabled:

```csharp
[Fact(Skip = "Waiting on API v2 endpoint deployment")]
public void CallExternalApi_ReturnsData() { ... }

[Theory(Skip = "Flaky on CI — investigating timing issue")]
[InlineData(1)]
public void ProcessAsync_ConcurrentCalls_HandlesCorrectly(int id) { ... }
```

### Diagnostic Output

Use `ITestOutputHelper` instead of `Console.WriteLine` — XUnit captures this output per-test and displays it on failure:

```csharp
public class DiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public DiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Process_LargeInput_CompletesWithinTimeout()
    {
        var sw = Stopwatch.StartNew();

        var result = _sut.Process(GenerateLargeInput());

        sw.Stop();
        _output.WriteLine($"Processing took {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000, "Processing exceeded 5s timeout");
    }
}
```

## Common Patterns

### Testing Async Code

```csharp
[Fact]
public async Task GetUserAsync_ValidId_ReturnsUser()
{
    // Arrange
    _mockRepo.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(new User { Id = 1, Name = "Alice" });

    // Act
    var result = await _sut.GetUserAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Alice", result.Name);
}
```

### Testing Collections of Results

```csharp
[Fact]
public void Search_MatchingTerm_ReturnsFilteredResults()
{
    var results = _sut.Search("active");

    Assert.NotEmpty(results);
    Assert.All(results, r => Assert.True(r.IsActive));
    Assert.DoesNotContain(results, r => r.Status == "Archived");
}
```

### Testing Events

```csharp
[Fact]
public void UpdatePrice_RaisesPropertyChanged()
{
    var product = new Product();
    var raised = false;

    product.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(Product.Price))
            raised = true;
    };

    product.Price = 29.99m;

    Assert.True(raised, "PropertyChanged event was not raised for Price");
}
```
