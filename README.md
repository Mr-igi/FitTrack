\# FitTrack



FitTrack is a gym management backend, built step by step in C# to learn the fundamentals of .NET and, later, Web API development. The goal is a server that stores gym data and exposes an API that any frontend (website, mobile app) can use.



This is a simplified take on products like PushPress: members, memberships, training sessions and bookings, without card payments or marketing features.



> \*\*Status:\*\* Phase 1 (console application) is complete. Phases 2 to 4 are in progress.



\## Features (Phase 1)



\- \*\*Roles:\*\* Admin, Trainer and Member, with role checks in the business rules.

\- \*\*Bookings:\*\* members book training sessions only if they have an active membership on the session date, are not already booked, and the session is not full or already started.

\- \*\*Cancellations:\*\* a booking can only be cancelled by its owner and at least 48 hours before the session.

\- \*\*Memberships:\*\* purchasing a membership stacks it after the member's current one, so coverage never overlaps.

\- \*\*Discounts:\*\* longer plans get a discount (10% for 90+ days, 20% for 365+ days), calculated with a `switch` expression.

\- \*\*Statistics (LINQ):\*\* active members, total revenue, revenue by plan, session occupancy, memberships expiring soon, members without bookings.

\- \*\*Events:\*\* `SessionFilled` is raised when the last spot in a session is taken, and `MembershipExpired` when a member's coverage runs out. A `NotificationService` subscribes to both.

\- \*\*Daily expiry check:\*\* `MarkExpiredMemberships` marks ended memberships and raises the event only once per membership, and only if the member has no later membership.



\## Tech



\- C# and .NET 10 or newer

\- Console application, no external packages

\- In-memory storage through a generic `IRepository<T>`



\## Project structure



```

src/FitTrack.ConsoleApp

├── Models         User, MembershipPlan, Membership, TrainingSession, Booking, report records

├── Repositories   IRepository<T> and InMemoryRepository<T>

├── Services       BookingService, MembershipService, StatisticsService, NotificationService

├── Exceptions     BookingException, MembershipException

└── Program.cs     Demo scenarios for every business rule

```



Services depend on `IRepository<T>`, not on a concrete storage, so the in-memory repository can later be replaced by a database (Phase 3) without changing the business logic.



\## How to run



```bash

git clone https://github.com/Mr-igi/FitTrack.git

cd FitTrack

dotnet run --project src/FitTrack.ConsoleApp

```



`Program.cs` runs a set of scenarios (successful and failing bookings, cancellations, purchases, statistics, expiry check) and prints `\[OK]`, `\[FAIL]` and `\[NOTIFY]` lines. A shortened example:



```

\--- Booking ---

\[OK]   Milos books CrossFit -> booking #1

\[FAIL] Milos books CrossFit again -> You are already booked for this session.

\[NOTIFY] Trainer Marko Jovanovic: "CrossFit" on 2026-10-20 18:00 is now full.

\[OK]   Petar books CrossFit -> booking #2



\--- Expiration check ---

Checking on 2026-11-05...

\[NOTIFY] Petar Ilic: your membership expired on 2026-10-30.

Marked 3 membership(s) as expired.

```



\## What I learned



\- \*\*OOP and design:\*\* domain models, business rules in services, custom exceptions, dependency injection through constructors.

\- \*\*Generics:\*\* a reusable `IRepository<T>` with an in-memory implementation.

\- \*\*LINQ:\*\* `Where`, `Select`, `GroupBy`, `OrderBy`, `Sum`, `Distinct`, and the difference between deferred and immediate execution (`ToList`).

\- \*\*Delegates and events:\*\* `Action<T>` events, subscribing with `+=`, and raising events after the state is saved.

\- \*\*Modern C#:\*\* `record` types for reports (value equality, `with` expressions), `switch` expressions with relational patterns, `DateOnly`, `required` members.

\- \*\*Git workflow:\*\* feature branches, pull requests and meaningful commit messages.



\## Roadmap



| Phase | Goal | Status |

|---|---|---|

| 1 | Console application with domain model and business rules | Done |

| 2 | Extract logic into `FitTrack.Core`, add xUnit tests for every rule | Next |

| 3 | Web API: EF Core and database, controllers, DTOs, JWT auth with roles, Swagger | Planned |

| 4 | Background service, Serilog, Docker, GitHub Actions CI, deploy to Azure | Planned |

| 5 | Optional React frontend connected to the API | Planned |

