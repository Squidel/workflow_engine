# StateMachine Library

## Overview
The **StateMachine Library** is a flexible, extendable, and fluent .NET library for managing state transitions, evaluating conditions, and triggering actions based on the state of an object. The library supports dynamic state transitions, condition evaluations, and trigger-based mechanisms for automating workflows.

### Key Features
- Manage state transitions with conditions stored in a database or defined in code.
- Evaluate object conditions dynamically using expressions.
- Trigger actions based on conditions with automatic logging and notification support.
- Fluent API for easy configuration and extension.
- Support for complex workflows with multiple transition options (e.g., Approve, Reject, Cancel, Void).

---

## Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo-url.git
   ```
2. Add the library project to your solution.
3. Reference the library in your main project.

---

## Getting Started
### Define a State Transition
Use the `StateMachine` class to define a state transition:
```csharp
var stateMachine = new StateMachine();

stateMachine.AddTransition("Pending", "Approved")
            .WithCondition(obj => obj.Amount > 100)
            .WithTrigger(obj => obj.Status = "Approved");
```

### Evaluate Triggers
To evaluate a set of triggers:
```csharp
var triggerManager = new TriggerManager();

triggerManager.AddTrigger(
    obj => obj.Status == "Pending" && obj.Amount > 100,
    obj => obj.Status = "Approved"
);

triggerManager.EvaluateTriggers(myObject);
```

### Get Previous State
Use the `GetPreviousState` method to retrieve the previous state of a record:
```csharp
var previousState = stateMachine.GetPreviousState(myObject);
```

---

## Fluent API Usage
The library provides a fluent API for configuring transitions and triggers:
```csharp
var stateMachine = new StateMachine()
    .AddTransition("Draft", "Submitted")
        .WithCondition(obj => obj.IsComplete)
        .WithTrigger(obj => obj.Message = "Submission successful")
    .AddTransition("Submitted", "Approved")
        .WithCondition(obj => obj.ReviewScore > 80)
        .WithTrigger(obj => obj.Status = "Approved");
```

---

## Advanced Features
### Notifications
You can add notifications that will be triggered when a condition is met:
```csharp
triggerManager.AddTrigger(
    obj => obj.Status == "Pending",
    obj => obj.Notification = "Your request is pending approval."
);
```

### Automatic Logging
Enable automatic logging of state transitions and actions:
```csharp
stateMachine.EnableLogging(log => Console.WriteLine(log));
```

---

## Customization
You can extend the library by:
- Adding custom conditions.
- Creating custom triggers.
- Implementing additional logging mechanisms.
- Supporting different object types and properties dynamically.

---

## Example Workflow
Here’s an example of using the library to manage a bid evaluation workflow:
```csharp
var stateMachine = new StateMachine();

stateMachine.AddTransition("EOI Sent", "Bid Received")
    .WithCondition(obj => obj.BidEvaluationTypeId == BidEvaluationTypeCodes.DIRECT_CONTRACTING)
    .WithTrigger(obj => ViewBag.allowEOI = true);

stateMachine.AddTransition("Bid Received", "Bids Evaluated")
    .WithCondition(obj => obj.EOISent && obj.Bids.Count > 0)
    .WithTrigger(obj => ViewBag.Eoi = true);

stateMachine.EvaluateTriggers(bidObject);
```

---

## Best Practices
- Use meaningful state names and conditions to ensure clarity in your workflows.
- Utilize logging to track state changes and triggers for debugging purposes.
- Keep triggers modular and reusable.

---

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Submit a pull request for review.

---

## License
This project is licensed under the [MIT License](LICENSE).

---

## Contact
For any questions or feedback, please contact [your-email@example.com].

