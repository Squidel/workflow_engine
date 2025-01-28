# StateMachineLibrary

StateMachineLibrary is a flexible and extensible .NET library for managing state transitions in applications. It allows users to define states, transitions, triggers, and associated actions, making it ideal for workflows, approvals, or other state-dependent processes.

## Features

- **Fluent API** for defining states, transitions, and triggers.
- **Trigger-based actions**: Evaluate conditions and execute actions seamlessly.
<!-- - **Extendable**: Add notifications, logging, or other custom behaviors. -->
- **Database integration**: Save state transitions to a database.
- **Dynamic configuration**: Define and update states or triggers programmatically.

---

## Shortcomings & Limitations

### Current Shortcomings:

1. **Basic Error Handling**: Error handling is limited; exceptions may not provide detailed context.
2. **Validation**: Limited checks exist to ensure the library is used correctly (e.g., `Build` must be called before saving transitions).
3. **Trigger Structure**: While triggers are highly flexible, there's no enforced structure for the object passed to them (e.g., ensuring it contains `Notification` properties like `Message` or `Status`).
4. **Logging**: The logging mechanism is simple and lacks advanced features like filtering logs by level (e.g., `Info`, `Debug`, `Error`).
5. **Concurrency**: No explicit mechanisms exist to handle concurrency issues in a multi-threaded environment.
6. **Serialization**: Triggers or transitions cannot be easily serialized out-of-the-box for storage or external usage.

### Future Improvements:

- Add more robust **validation** mechanisms.
- Introduce **default implementations** for common use cases (e.g., notifications, logging).
- Support advanced logging with configurable log levels.
- Enable **serialization** for easier configuration sharing.
- Add ability to use data from other databases (files, mysql, etc)

---

## Installation

You can install the library via NuGet:

```bash
dotnet add package StateMachineLibrary --version 1.0.0-alpha
```

## Getting Started

### Define a simple state transition

```csharp

 var stateMachine = StateMachineFactory.Create("data source=database connection");
                var test = stateMachine.GetNextState(21);
                stateMachine.PerformTransition(entities, request, all, (context, rec, nextState, related) =>
                {
                    rec.StateId = nextState;

                    foreach(var e in related)
                    {
                        e.RequestCode = "it wrk";
                    }

                    context.SaveChanges();
                });
```

### Evaluate triggers

```csharp
var myObject = new MyObject { IsReadyToPublish = true, Status = "Draft" };
stateMachine.EvaluateTriggers(myObject);
```

## Contributing

Contributions are welcome! If you find a bug or have a feature request, please open an issue or submit a pull request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
