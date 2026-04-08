# Lecture 9 - Concepts

## Implementing the Queueing Entities

### Step 1: ReviewQueue Model

Create `Models/ReviewQueue.cs`:

```csharp
namespace Buckeye.Lending.Api.Models;

public class ReviewQueue
{
    public int Id { get; set; }

    public string OfficerId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — one queue has many items
    public ICollection<ReviewItem> Items { get; set; } = new List<ReviewItem>();
}
```

Then create `Validators/ReviewQueueValidator.cs`:

```csharp
using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class ReviewQueueValidator : AbstractValidator<ReviewQueue>
{
    public ReviewQueueValidator()
    {
        RuleFor(x => x.OfficerId)
            .NotEmpty();
    }
}
```

**Concept:** Simple. An Id, an OfficerId — required, because every queue must belong to someone — timestamps, and a navigation property for the items collection. The `= new List<>()` default prevents null reference exceptions when you access Items on a new queue.

Notice the model is clean — no validation attributes. Instead, validation rules live in a dedicated `AbstractValidator<T>` class. This is the FluentValidation pattern: separate concerns. The model describes the shape of the data; the validator describes the rules.

### Step 2: ReviewItem Model

Create `Models/ReviewItem.cs`:

```csharp
namespace Buckeye.Lending.Api.Models;

public class ReviewItem
{
    public int Id { get; set; }

    // Foreign key to the queue
    public int QueueId { get; set; }
    public ReviewQueue Queue { get; set; } = null!;

    // Foreign key to the loan application
    public int LoanApplicationId { get; set; }
    public LoanApplication LoanApplication { get; set; } = null!;

    public int Priority { get; set; } = 3;

    public string? Notes { get; set; }
}
```

Then create `Validators/ReviewItemValidator.cs`:

```csharp
using FluentValidation;
using Buckeye.Lending.Api.Models;

namespace Buckeye.Lending.Api.Validators;

public class ReviewItemValidator : AbstractValidator<ReviewItem>
{
    public ReviewItemValidator()
    {
        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5);
    }
}
```

**Concept:** ReviewItem has two foreign keys: QueueId links it to the queue, LoanApplicationId links it to the loan application being reviewed. Both have navigation properties so we can use `.Include()` in queries. Priority defaults to 3 — middle of the range. Notes are nullable because they're optional.

The `= null!` on the navigation properties tells the compiler 'I know this looks null, but EF Core will populate it when I use Include.' This is standard EF Core convention.

The `InclusiveBetween(1, 5)` rule in the validator replaces the old `[Range(1, 5)]` attribute — same constraint, but expressed as a fluent rule.

### Step 3: Update DbContext

Open `Data/LendingContext.cs` and add:

```csharp
public DbSet<ReviewQueue> ReviewQueues { get; set; }
public DbSet<ReviewItem> ReviewItems { get; set; }
```

### Step 4: Run Migration

First, we need to install the EF Core tools if you haven't already:

```bash
dotnet tool install --global dotnet-ef
```

Then, create and apply the migration:

```bash
dotnet ef migrations add AddReviewQueue
dotnet ef database update
```

**Concept:** This will create the necessary tables in the database for our new models. The migration will include the creation of the `ReviewQueues` and `ReviewItems` tables, along with the appropriate foreign key constraints.

Migrations are a powerful feature of EF Core that allow us to evolve our database schema over time as our application requirements change. By creating a migration, we can ensure that our database stays in sync with our application's data model.

---

## Registering FluentValidation

Before building the controller, we need to wire up FluentValidation in `Program.cs`. Add the using statement and register validators:

```csharp
using FluentValidation;
```

Then after `AddControllers()`, register all validators from the assembly:

```csharp
// FluentValidation — register all validators from this assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

**Concept:** One line does the work. `AddValidatorsFromAssemblyContaining<Program>()` scans the assembly for every class that extends `AbstractValidator<T>` and registers them in DI as `IValidator<T>`. No need to register each validator individually — add a new validator class and it's automatically picked up.

> **Note:** The old `FluentValidation.AspNetCore` package and its `AddFluentValidationAutoValidation()` method are **deprecated and no longer supported**. Instead of hooking into ASP.NET's synchronous model validation pipeline, we inject `IValidator<T>` into controllers and call `ValidateAsync` explicitly. This is the approach recommended by the FluentValidation author ([see #1959](https://github.com/FluentValidation/FluentValidation/issues/1959)) and has two key advantages:
>
> 1. **Async support** — validators with `MustAsync` rules (like our `AddToQueueRequestValidator` that checks the database) actually run asynchronously.
> 2. **Explicit control** — you see exactly where and when validation happens in your controller code. No hidden "magic."

### Manual Validation Pattern

In each controller, inject the validator and call it before processing:

```csharp
private readonly IValidator<MyModel> _validator;

public MyController(LendingContext context, IValidator<MyModel> validator)
{
    _context = context;
    _validator = validator;
}

[HttpPost]
public async Task<ActionResult<MyModel>> Create(MyModel model)
{
    var result = await _validator.ValidateAsync(model);
    if (!result.IsValid)
    {
        result.AddToModelState(ModelState);
        return ValidationProblem(ModelState);
    }

    // ... proceed with business logic
}
```

The `AddToModelState` extension method copies FluentValidation errors into ASP.NET's `ModelState`, and `ValidationProblem()` returns a standard RFC 7807 problem details response with structured error information — consistent with how the rest of our API handles errors.

---

## Controller Skeleton

Create `Dtos/ReviewQueueRequests.cs`:

```csharp
namespace Buckeye.Lending.Api.Dtos;

public class AddToQueueRequest
{
    public int LoanApplicationId { get; set; }
    public int Priority { get; set; } = 3;
}

public class UpdateItemRequest
{
    public int Priority { get; set; }
    public string? Notes { get; set; }
}
```

Then create `Validators/AddToQueueRequestValidator.cs`:

```csharp
using FluentValidation;
using Buckeye.Lending.Api.Dtos;

namespace Buckeye.Lending.Api.Validators;

public class AddToQueueRequestValidator : AbstractValidator<AddToQueueRequest>
{
    public AddToQueueRequestValidator()
    {
        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5);
    }
}
```

DTOs stay clean too — validation rules live in their own validator class.

Create `Controllers/ReviewQueueController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Models;
using Buckeye.Lending.Api.Dtos;

namespace Buckeye.Lending.Api.Controllers;

[ApiController]
[Route("api/review-queue")]
public class ReviewQueueController : ControllerBase
{
    private readonly LendingContext _context;
    private const string CurrentOfficerId = "default-officer";

    public ReviewQueueController(LendingContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ReviewQueue>> GetQueue()
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<ActionResult<ReviewItem>> AddToQueue(AddToQueueRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPut("{itemId}")]
    public async Task<ActionResult<ReviewItem>> UpdateItem(int itemId, UpdateItemRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpDelete("{itemId}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearQueue()
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
```

**Concept:** Shape first, logic second. Five endpoints. The constant at the top — that's our identity strategy for now. Constructor injection of the DbContext — same as every controller you've built.

Three things to notice.

1. `ActionResult<ReviewQueue>` on GetQueue — we return data, so we use the generic version. `IActionResult` on the deletes — no data to return, just a status code.
2. The route attribute is `api/review-queue`, not `api/[controller]` — because the controller name is `ReviewQueueController` but we want a hyphenated route.
3. Two DELETE routes — `{itemId}` and `clear`. ASP.NET can distinguish them because the route templates are different.

---

## Implementing GetQueue

Replace the GetQueue TODO:

```csharp
[HttpGet]
public async Task<ActionResult<ReviewQueue>> GetQueue()
{
    var queue = await _context.ReviewQueues
        .Include(q => q.Items)
        .ThenInclude(i => i.LoanApplication)
        .FirstOrDefaultAsync(q => q.OfficerId == CurrentOfficerId);

    if (queue == null)
    {
        return NotFound();
    }

    return Ok(queue);
}
```

**Concept:** Three lines of real logic. Find the queue for this officer, including items and their loan applications. If it doesn't exist, return 404. If it does, return 200 with the full queue.

This is a design decision we made: if the officer hasn't added anything yet, there is no queue. The frontend gets a 404 and displays an empty state — 'no applications in your review queue.' The alternative would be to auto-create an empty queue and return 200, but then you have empty queue records sitting in your database for every officer who's never used the feature. 404 is cleaner.

For M4: same decision. If the user hasn't added anything to their cart, return 404. Your React code handles it as 'your cart is empty.'

---

## Implementing AddToQueue (Upsert Pattern)

Replace the AddToQueue TODO:

```csharp
[HttpPost]
public async Task<ActionResult<ReviewItem>> AddToQueue(AddToQueueRequest request)
{
    // 1. Verify the loan application exists
    var loanApp = await _context.LoanApplications.FindAsync(request.LoanApplicationId);
    if (loanApp == null)
    {
        return BadRequest($"Loan application {request.LoanApplicationId} not found.");
    }

    // 2. Find or create the queue for this officer
    var queue = await _context.ReviewQueues
        .Include(q => q.Items)
        .FirstOrDefaultAsync(q => q.OfficerId == CurrentOfficerId);

    if (queue == null)
    {
        queue = new ReviewQueue { OfficerId = CurrentOfficerId };
        _context.ReviewQueues.Add(queue);
    }

    // 3. Check if this loan application is already in the queue (UPSERT)
    var existingItem = queue.Items
        .FirstOrDefault(i => i.LoanApplicationId == request.LoanApplicationId);

    if (existingItem != null)
    {
        // Update — loan already in queue, just update priority
        existingItem.Priority = request.Priority;
        queue.UpdatedAt = DateTime.UtcNow;
    }
    else
    {
        // Insert — new item
        var newItem = new ReviewItem
        {
            LoanApplicationId = request.LoanApplicationId,
            Priority = request.Priority
        };
        queue.Items.Add(newItem);
        queue.UpdatedAt = DateTime.UtcNow;
    }

    // 4. Save everything in one transaction
    await _context.SaveChangesAsync();

    // 5. Reload with navigation properties for the response
    var savedItem = await _context.ReviewItems
        .Include(i => i.LoanApplication)
        .FirstAsync(i => i.QueueId == queue.Id
            && i.LoanApplicationId == request.LoanApplicationId);

    return CreatedAtAction(nameof(GetQueue), savedItem);
}
```

**Concept:** This is the most complex endpoint, so let's break it down.

**Step 1:** Verify the loan application exists. If someone sends a request with a bad ID, we catch it early with a 400 Bad Request. Don't skip this — without it, you'd get a foreign key violation from the database, which is a 500 error. Much worse.

**Step 2:** Find or create the queue. This is the first new pattern. With products, you never had to 'find or create a container' — you just inserted. Here, the queue might not exist yet. If it doesn't, we create one. If it does, we use the existing one.

**Step 3:** The upsert. Check if this loan application is already in the queue. If yes — update the priority. If no — create a new ReviewItem and add it to the queue. Either way, update the timestamp.

This is the pattern you'll use for M4: check if the product is already in the cart. If yes, increment the quantity. If no, create a new CartItem.

**Step 4:** One SaveChanges call. Whether we created a queue, updated an existing item, or added a new one — it all saves in one database transaction. Either everything succeeds or nothing does. That's data consistency.

**Step 5:** Reload the saved item with its navigation properties so the response includes the loan application details. Return 201 Created."

**Summary:**

1. First POST — 201 Created. GET — there's our queue with one item.
2. POST with the same loan ID but different priority — 201 again, but look at the GET: still one item, priority updated.
3. That's the upsert working. No duplicates.

---
