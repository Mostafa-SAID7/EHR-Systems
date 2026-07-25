# Design Patterns - Complete Guide

## What are Design Patterns?

Design Patterns are **reusable solutions to common programming problems**.

They are:
- **Proven** - Used successfully for decades
- **Best practices** - Recommended by industry
- **Language-agnostic** - Apply to any language
- **Time-savers** - Don't reinvent the wheel

```
Problem              Pattern               Solution
─────────────────────────────────────────────────────
Create objects?  →  Factory            →  Without naming classes
Add behavior?    →  Decorator          →  Dynamically
Complex ops?     →  Strategy           →  Encapsulated algorithms
State changes?   →  State              →  Behavior depends on state
Pass requests?   →  Command            →  As objects
Multiple roles?  →  Observer           →  Notify all listeners
```

---

## Current Status

✅ **Covered:**
- SOLID Principles (5 principles)
- Repository Pattern (+ Unit of Work)

❌ **Missing:**
- 30+ critical patterns
- See COVERAGE_ANALYSIS.md for details

---

## Three Categories of Patterns

### 1. Creational Patterns (How to Create Objects)
```
Problem: Creating objects directly is inflexible
Solution: 
  - Singleton (one instance)
  - Factory (hide construction)
  - Builder (complex construction)
  - Prototype (clone instead of create)
```

### 2. Structural Patterns (How to Organize Objects)
```
Problem: Objects don't fit together
Solution:
  - Adapter (make incompatible interfaces work)
  - Decorator (add behavior without modifying)
  - Facade (simplify complex systems)
  - Proxy (control access)
```

### 3. Behavioral Patterns (How Objects Interact)
```
Problem: Complex interactions, state changes
Solution:
  - Observer (notify multiple objects)
  - Strategy (swap algorithms)
  - State (change behavior based on state)
  - Command (encapsulate requests)
  - Visitor (add operations to objects)
```

---

## Most Important Patterns (Start Here)

### TIER 1: Use Almost Every Day ⭐⭐⭐

1. **Dependency Injection** - Decouple object creation
   ```csharp
   public class Service
   {
       public Service(IRepository repo) { } // Inject, don't create
   }
   ```

2. **Singleton** - One instance globally
   ```csharp
   public class Logger
   {
       private static Logger _instance = new();
       public static Logger Instance => _instance;
   }
   ```

3. **Factory** - Create objects without naming classes
   ```csharp
   var user = UserFactory.Create("Ahmed", "user@example.com");
   ```

4. **Repository** - ✅ Exists
   ```csharp
   var user = await _repository.GetUserAsync(1);
   ```

5. **Strategy** - Encapsulate algorithms
   ```csharp
   var sorter = new QuickSort();  // or MergeSort
   sorter.Sort(data);
   ```

### TIER 2: Use Weekly ⭐⭐

6. **Observer / Events** - Notify on changes
   ```csharp
   user.OnCreated += NotificationService.SendWelcome;
   ```

7. **Decorator** - Add behavior dynamically
   ```csharp
   var loggedService = new LoggingDecorator(baseService);
   ```

8. **Command** - Encapsulate requests
   ```csharp
   var cmd = new CreateUserCommand(dto);
   await bus.SendAsync(cmd);
   ```

9. **DTO** - Transfer data between layers
   ```csharp
   public class CreateUserDto { public string Name { get; set; } }
   ```

10. **Value Object** - Immutable data
    ```csharp
    public class Email
    {
        public string Value { get; }
        public Email(string value) { Value = value; } // Immutable
    }
    ```

### TIER 3: Use Occasionally ⭐

11. **Adapter** - Make incompatible interfaces work
12. **Facade** - Simplify complex systems
13. **Builder** - Construct complex objects
14. **Proxy** - Control access
15. **State** - Behavior depends on state

---

## Design Patterns by Real-World Problem

### Problem: How do I create objects?
- ✅ **Factory** - Without naming classes directly
- ✅ **Builder** - Complex objects with many options
- ✅ **Singleton** - Exactly one instance

### Problem: How do I add features without modifying code?
- ✅ **Decorator** - Add behavior dynamically
- ✅ **Strategy** - Swap algorithms at runtime
- ✅ **Observer** - Notify many objects of events

### Problem: How do I simplify complex operations?
- ✅ **Facade** - Hide complexity behind simple interface
- ✅ **Adapter** - Make things that don't fit work together
- ✅ **Command** - Encapsulate requests as objects

### Problem: How do I organize large systems?
- ✅ **Repository** - Centralize data access
- ✅ **Service Layer** - Business logic separation
- ✅ **Mediator** - Centralize complex communication

### Problem: How do I handle complex state changes?
- ✅ **State** - Change behavior based on state
- ✅ **Observer** - React to state changes
- ✅ **Command** - Queue state-changing requests

---

## Patterns Used in EHR Codebase

```
✅ VISIBLE NOW:
├── Repository Pattern (data layer)
├── Dependency Injection (constructor injection everywhere)
├── Factory (creating domain entities)
├── Observer/Events (domain events, integration events)
├── Command (CQRS - commands separate from queries)
├── Service Layer (application services)
├── DTO (Data Transfer Objects for APIs)
├── Middleware (ASP.NET Core request pipeline)
├── Value Objects (Money, Email, Phone as value objects)
├── SOLID Principles (attempted adherence)
├── Decorator (probably in validators)
└── Adapter (external service integration)

❌ NEED TO DOCUMENT:
├── How patterns work together
├── Configuration and setup
├── Common mistakes
├── Trade-offs and when to use
└── Interview Q&A
```

---

## Learning Path

### For Quick Interview Prep (2-3 days)
1. SOLID Principles (✅ exists)
2. Singleton
3. Factory
4. Dependency Injection
5. Repository (✅ exists)
6. Strategy
7. Observer/Events
8. Command
9. Decorator
10. DTO

### For Comprehensive Understanding (1-2 weeks)
1. All Tier 1 patterns
2. All Tier 2 patterns
3. Read COVERAGE_ANALYSIS.md
4. Review EHR codebase examples
5. Practice implementing patterns

### For Mastery (1 month)
1. Complete all patterns
2. Understand trade-offs
3. Know when to use/avoid
4. Can explain all variations
5. Can design new patterns if needed

---

## Pattern Frequency in Interviews

| Frequency | Patterns |
|-----------|----------|
| Asked 90%+ | Singleton, Factory, DI, Strategy, Repository, Observer |
| Asked 60%+ | Builder, Adapter, Command, DTO, Decorator |
| Asked 30%+ | Composite, State, Facade, Proxy, Mediator |
| Asked 10%+ | Visitor, Interpreter, Chain, Prototype |

---

## Folder Structure (Current → Target)

```
Current:
docs/DesignPatterns/
├── solid-principles.md
└── repository-pattern.md

Target:
docs/DesignPatterns/
├── README.md (This file)
├── COVERAGE_ANALYSIS.md (Gap analysis)
├── Interview-QA.md (Coming soon)
│
├── Creational/
│   ├── singleton.md
│   ├── factory.md
│   ├── abstract-factory.md
│   ├── builder.md
│   └── ...
│
├── Structural/
│   ├── adapter.md
│   ├── decorator.md
│   ├── facade.md
│   ├── proxy.md
│   └── ...
│
├── Behavioral/
│   ├── observer.md
│   ├── strategy.md
│   ├── command.md
│   ├── state.md
│   └── ...
│
├── Architectural/
│   ├── dependency-injection.md
│   ├── service-layer.md
│   ├── specification.md
│   └── ...
│
├── Enterprise/
│   ├── repository-pattern.md (move existing)
│   ├── dto.md
│   ├── value-object.md
│   ├── mapper.md
│   └── ...
│
└── SOLID/
    └── solid-principles.md (move existing)
```

---

## Key Concepts Quick Reference

### Creational
```
Singleton:     One instance globally
Factory:       Create without naming class
Builder:       Complex construction step-by-step
Prototype:     Clone instead of create new
```

### Structural
```
Adapter:       Make incompatible interfaces work
Decorator:     Add behavior dynamically
Facade:        Simplify complex systems
Proxy:         Control access to object
```

### Behavioral
```
Observer:      Notify many objects of changes
Strategy:      Swap algorithms dynamically
Command:       Encapsulate requests as objects
State:         Change behavior based on state
```

### Architectural
```
Repository:    Abstract data access
DTO:           Transfer data between layers
Value Object:  Immutable data container
Service Layer: Business logic separation
```

---

## Interview Pattern Framework

When asked about a pattern:

1. **What** - Definition
2. **Problem** - What problem does it solve?
3. **Solution** - How does it work? (show code)
4. **UML** - Visual representation
5. **Example** - Real-world use case
6. **When** - When to use
7. **When NOT** - When to avoid
8. **Trade-offs** - Pros and cons
9. **Variations** - Different implementations
10. **Related** - Similar patterns

---

## Each Pattern File Should Include

✅ **Minimum:**
- Definition
- Problem it solves
- C# code example
- When to use
- When NOT to use

✅ **Better:**
- UML diagram
- Multiple examples
- Real EHR codebase examples
- Interview Q&A
- Trade-offs
- Related patterns
- Common mistakes

---

## Success Criteria

Design Patterns folder is complete when:
- ✅ 35+ patterns documented
- ✅ Each with C# examples
- ✅ EHR codebase examples
- ✅ Interview Q&A consolidated
- ✅ Visual diagrams
- ✅ Clear when to use/avoid
- ✅ Trade-offs explained

---

## Priority Roadmap

### Phase 1: Immediate (Essential 15)
- [ ] Dependency Injection
- [ ] Factory
- [ ] Singleton  
- [ ] Strategy
- [ ] Observer
- [ ] Command
- [ ] DTO
- [ ] Value Object
- [ ] Decorator
- [ ] Adapter
- [ ] Builder
- [ ] Repository (✅ move existing)
- [ ] SOLID (✅ move existing)
- [ ] Service Layer
- [ ] Mapper

### Phase 2: Next (Important 10)
- [ ] Facade
- [ ] Proxy
- [ ] State
- [ ] Composite
- [ ] Mediator
- [ ] Chain of Responsibility
- [ ] Template Method
- [ ] Iterator
- [ ] Interpreter
- [ ] Prototype

### Phase 3: Advanced (Specialized 10)
- [ ] Visitor
- [ ] Memento
- [ ] Flyweight
- [ ] Bridge
- [ ] Abstract Factory
- [ ] Object Pool
- [ ] Saga
- [ ] Event Sourcing
- [ ] Lazy Loading
- [ ] Cache-Aside

---

## Next Steps

1. **Read this README**
2. **Review COVERAGE_ANALYSIS.md**
3. **Move existing files** to Architectural/SOLID folders
4. **Create Phase 1 patterns** (15 essential)
5. **Add Phase 2** (10 important)
6. **Complete Phase 3** (10 specialized)
7. **Create consolidated Interview-QA.md**

---

## Resources

- **COVERAGE_ANALYSIS.md** - What's missing, priorities
- **solid-principles.md** - ✅ Existing
- **repository-pattern.md** - ✅ Existing
- Gang of Four Design Patterns book
- Martin Fowler's Enterprise Patterns
- EHR Codebase - Real examples

---

## Status

**Current:** 2 files (6% coverage)  
**Target:** 35+ files (95% coverage)  
**Priority:** Create Phase 1 (essential patterns) next

For detailed gap analysis: **COVERAGE_ANALYSIS.md**
