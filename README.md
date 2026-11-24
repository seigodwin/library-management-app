# Library Management System

A console-based library management system built with C# and .NET 10. The application stores books, members, and loans in memory while it is running.

## Features

- Add and list books
- Register and list members
- Borrow and return books
- Track book availability
- Calculate due dates automatically
- Show active, returned, and overdue loans
- Apply different borrowing rules to adult and student members
- Validate required fields, email addresses, years, IDs, and borrowing limits

## Class Design

The application uses separate classes so that each part of the system has a clear responsibility:

- **`Book`** represents a book in the library. It stores the title, author, publication year, ID, and availability.
- **`Member`** represents a library member. It stores the member's details and tracks their loans. Adult members can borrow up to 5 books for 21 days, while student members can borrow up to 3 books for 14 days.
- **`Loan`** represents one borrowing transaction. It stores the book, member, borrowing date, due date, and return date.
- **`Library`** manages the books, members, and loans. It contains the main business rules for adding records, borrowing books, and returning books.
- **`ConsoleApplication`** handles the menu and user input. It calls the `Library` class instead of containing the business logic itself.
- **`MemberType`** identifies whether a member is an adult or a student.

The system follows object-oriented design by giving each class its own data and behavior. The console interface is separated from the library operations, making the code easier to understand and maintain.

## Requirements

- .NET 10 SDK
- Visual Studio 2026 or another C# development environment

Check the installed SDK with:

```bash
dotnet --version
```

## Setup and Running

### Using Visual Studio

1. Open `w03_assignment.slnx` in Visual Studio.
2. Select the `w03_assignment` project as the startup project if necessary.
3. Press **Ctrl+F5** to run without debugging, or press **F5** to run with debugging.

### Using the .NET CLI

Open a terminal in the `w03_assignment` project directory:

```bash
cd w03_assignment
dotnet restore
dotnet run
```

To build the project without running it:

```bash
dotnet build
```

## Using the Application

Use the numbered menu to:

1. Add a book
2. Add a member
3. Borrow a book using its book ID and a member ID
4. Return a book using its book ID
5. List all books
6. List all members
7. List all loans
0. Exit

Book and member IDs are generated automatically, for example `B001` and `M001`.

## Data Storage

The application uses in-memory collections. All books, members, and loans are cleared when the application closes. A database or file-based storage system could be added later if permanent storage is required.
