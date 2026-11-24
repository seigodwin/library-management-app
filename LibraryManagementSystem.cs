namespace W03Assignment;

enum MemberType
{
    Adult,
    Student
} 

sealed class Book
{
    public string Id { get; }
    public string Title { get; }
    public string Author { get; }
    public int PublicationYear { get; }
    public bool IsAvailable { get; internal set; } = true;

    public Book(string id, string title, string author, int publicationYear)
    {
        Id = id;
        Title = title;
        Author = author;
        PublicationYear = publicationYear;
    }

    public override string ToString() => $"{Id} | {Title} by {Author} ({PublicationYear}) | {(IsAvailable ? "Available" : "On loan")}";
}

sealed class Member
{
    public string Id { get; }
    public string Name { get; }
    public string Email { get; }
    public MemberType Type { get; }
    public List<Loan> Loans { get; } = [];
    public int LoanLimit => Type == MemberType.Student ? 3 : 5;
    public int LoanDays => Type == MemberType.Student ? 14 : 21;

    public Member(string id, string name, string email, MemberType type)
    {
        Id = id;
        Name = name;
        Email = email;
        Type = type;
    }

    public override string ToString() => $"{Id} | {Name} | {Email} | {Type} | Active loans: {Loans.Count(l => !l.IsReturned)}";
}

sealed class Loan
{
    public Book Book { get; }
    public Member Member { get; }
    public DateOnly BorrowedOn { get; }
    public DateOnly DueDate { get; }
    public DateOnly? ReturnedOn { get; private set; }
    public bool IsReturned => ReturnedOn.HasValue;

    public Loan(Book book, Member member, DateOnly borrowedOn, DateOnly dueDate)
    {
        Book = book;
        Member = member;
        BorrowedOn = borrowedOn;
        DueDate = dueDate;
    }

    public void Return(DateOnly returnedOn) => ReturnedOn = returnedOn;

    public string Status(DateOnly today)
    {
        if (IsReturned)
        {
            return $"Returned on {ReturnedOn:yyyy-MM-dd}";
        }

        return today > DueDate ? $"Overdue by {today.DayNumber - DueDate.DayNumber} day(s)" : $"Due {DueDate:yyyy-MM-dd}";
    }
}

sealed class Library
{
    private readonly Dictionary<string, Book> books = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Member> members = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Loan> loans = [];
    private int nextBookNumber = 1;
    private int nextMemberNumber = 1;

    public IReadOnlyCollection<Book> Books => books.Values;
    public IReadOnlyCollection<Member> Members => members.Values;
    public IReadOnlyCollection<Loan> Loans => loans;

    public (bool Success, string Message, Book? Value) AddBook(string title, string author, int publicationYear)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
            return (false, "Title and author are required.", null);
        if (publicationYear < 0 || publicationYear > DateTime.Today.Year)
            return (false, $"Publication year must be between 0 and {DateTime.Today.Year}.", null);

        var book = new Book($"B{nextBookNumber++:000}", title.Trim(), author.Trim(), publicationYear);
        books.Add(book.Id, book);
        return (true, $"Book added with ID {book.Id}.", book);
    }

    public (bool Success, string Message, Member? Value) AddMember(string name, string email, MemberType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name is required.", null);
        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
            return (false, "Enter a valid email address.", null);

        var member = new Member($"M{nextMemberNumber++:000}", name.Trim(), email.Trim(), type);
        members.Add(member.Id, member);
        return (true, $"Member added with ID {member.Id}.", member);
    }

    public (bool Success, string Message, Loan? Value) BorrowBook(string bookId, string memberId, DateOnly? borrowedOn = null)
    {
        if (!books.TryGetValue(bookId.Trim(), out var book))
            return (false, "Book not found.", null);
        if (!members.TryGetValue(memberId.Trim(), out var member))
            return (false, "Member not found.", null);
        if (!book.IsAvailable)
            return (false, "That book is already on loan.", null);

        var activeLoans = member.Loans.Count(l => !l.IsReturned);
        if (activeLoans >= member.LoanLimit)
            return (false, $"{member.Type} members may have at most {member.LoanLimit} active loan(s).", null);

        var startDate = borrowedOn ?? DateOnly.FromDateTime(DateTime.Today);
        var loan = new Loan(book, member, startDate, startDate.AddDays(member.LoanDays));
        book.IsAvailable = false;
        member.Loans.Add(loan);
        loans.Add(loan);
        return (true, $"Loan created. Due date: {loan.DueDate:yyyy-MM-dd}.", loan);
    }

    public (bool Success, string Message) ReturnBook(string bookId, DateOnly? returnedOn = null)
    {
        var loan = loans.LastOrDefault(l => !l.IsReturned && l.Book.Id.Equals(bookId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (loan is null)
            return (false, "No active loan was found for that book.");

        loan.Return(returnedOn ?? DateOnly.FromDateTime(DateTime.Today));
        loan.Book.IsAvailable = true;
        return (true, $"Book returned. {loan.Status(DateOnly.FromDateTime(DateTime.Today))}.");
    }

}

sealed class ConsoleApplication
{
    private readonly Library library = new();

    public void Run()
    {
        Console.WriteLine("=== Library Management System ===");
        while (true)
        {
            ShowMenu();
            switch (ReadRequired("Select an option: "))
            {
                case "1": AddBook(); break;
                case "2": AddMember(); break;
                case "3": BorrowBook(); break;
                case "4": ReturnBook(); break;
                case "5": ListBooks(); break;
                case "6": ListMembers(); break;
                case "7": ListLoans(); break;
                case "0": return;
                default: Console.WriteLine("Invalid option. Choose a number from 0 to 7."); break;
            }
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("\n1. Add book\n2. Add member\n3. Borrow book\n4. Return book\n5. List books\n6. List members\n7. List loans\n0. Exit");
    }

    private void AddBook()
    {
        var title = ReadRequired("Title: ");
        var author = ReadRequired("Author: ");
        var year = ReadInt("Publication year: ");
        Console.WriteLine(library.AddBook(title, author, year).Message);
    }

    private void AddMember()
    {
        var name = ReadRequired("Name: ");
        var email = ReadRequired("Email: ");
        var type = ReadMemberType();
        Console.WriteLine(library.AddMember(name, email, type).Message);
    }

    private void BorrowBook()
    {
        var result = library.BorrowBook(ReadRequired("Book ID: "), ReadRequired("Member ID: "));
        Console.WriteLine(result.Message);
    }

    private void ReturnBook()
    {
        Console.WriteLine(library.ReturnBook(ReadRequired("Book ID: ")).Message);
    }

    private void ListBooks()
    {
        Console.WriteLine("\nBooks:");
        if (library.Books.Count == 0) Console.WriteLine("No books registered.");
        foreach (var book in library.Books.OrderBy(b => b.Id)) Console.WriteLine(book);
    }

    private void ListMembers()
    {
        Console.WriteLine("\nMembers:");
        if (library.Members.Count == 0) Console.WriteLine("No members registered.");
        foreach (var member in library.Members.OrderBy(m => m.Id)) Console.WriteLine(member);
    }

    private void ListLoans()
    {
        Console.WriteLine("\nLoans:");
        if (library.Loans.Count == 0) Console.WriteLine("No loans registered.");
        var today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var loan in library.Loans) Console.WriteLine($"{loan.Book.Id} | {loan.Book.Title} | {loan.Member.Name} | Borrowed {loan.BorrowedOn:yyyy-MM-dd} | Due {loan.DueDate:yyyy-MM-dd} | {loan.Status(today)}");
    }

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(value)) return value;
            Console.WriteLine("This value is required.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            var value = ReadRequired(prompt);
            if (int.TryParse(value, out var number)) return number;
            Console.WriteLine("Enter a whole number.");
        }
    }

    private static MemberType ReadMemberType()
    {
        while (true)
        {
            var value = ReadRequired("Member type (A)dult/(S)tudent: ");
            if (value.Equals("A", StringComparison.OrdinalIgnoreCase) || value.Equals("Adult", StringComparison.OrdinalIgnoreCase)) return MemberType.Adult;
            if (value.Equals("S", StringComparison.OrdinalIgnoreCase) || value.Equals("Student", StringComparison.OrdinalIgnoreCase)) return MemberType.Student;
            Console.WriteLine("Choose Adult or Student.");
        }
    }
}
