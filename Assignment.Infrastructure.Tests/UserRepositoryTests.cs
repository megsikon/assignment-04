namespace Assignment.Infrastructure.Tests;

public class UserRepositoryTests
{
    private readonly KanbanContext _context;
    private readonly UserRepository _repo;

    public UserRepositoryTests() {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

        var User1 = new User("Alice","al@ice.org"){Id=1, Name = "Alice", Email = "al@ice.org"};
        var User3 = new User("Gertrud", "ger@trud.org"){Id=2, Name="Gertrud", Email="ger@trud.org"};

        context.Users.Add(User1);
        context.Users.Add(User3);

        context.SaveChanges();

        _context = context;
        _repo = new UserRepository(_context);
    }

#region Create
    [Fact]
    public void Create_new_User_returns_Created_User(){
        var(response,created) = _repo.Create(new UserCreateDTO("Bob","Bo@b.dk"));
        response.Should().Be(Created);
        created.Should().Be(new UserDTO(3,"Bob","Bo@b.dk").Id);
    }
    [Fact]
    public void Create_User_given_exisiting_user_returns_Conflict(){
        var(response,user) = _repo.Create(new UserCreateDTO("Alice","Ali@ce.org"));
        response.Should().Be(Conflict);
        var existingUser= new UserDTO(1,"Alice","Ali@ce.org");
        user.Should().Be(new UserDTO(1,"Alice","Ali@ce.org").Id);    
    }
#endregion

#region Delete
    [Fact]
    public void Delete_given_non_existing_UserId_returns_NotFound() => _repo.Delete(42).Should().Be(NotFound);

    [Fact]
    public void Delete_user_without_force_return_Conflict() {
        var response = _repo.Delete(1);
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Delete_user_with_force_returns_Deleted() {
        var response = _repo.Delete(1, true);
        response.Should().Be(Response.Deleted);
    }
#endregion

#region Update
    [Fact]
    public void Update_given_non_existing_User_returns_NotFound() => _repo.Update(new UserUpdateDTO(42,"BinBon","Bin@Bon.dk")).Should().Be(NotFound);
 
    [Fact]
    public void Update_User_returns_Updated_with_User(){
        var response = _repo.Update(new UserUpdateDTO(1,"Beatrice", "Bea@trice.or"));
        response.Should().Be(Updated);
        var entity = _context.Users.Find(1)!;
        entity.Name.Should().Be("Beatrice");
    }

    [Fact]
    public void Update_User_given_exisiting_user_returns_Conflict(){
        var response = _repo.Update(new UserUpdateDTO(1,"Gertrud", "ger@trud.org"));
        response.Should().Be(Conflict);
        var entity = _context.Users.Find(1)!;
        entity.Name.Should().Be("Alice");
    }
#endregion

#region Read
    [Fact]
    public void ReadAll_returns_all_Users() => _repo.ReadAll().Should().BeEquivalentTo(new[] {new UserDTO(1,"Alice","al@ice.org"),new UserDTO(2, "Gertrud","ger@trud.org")});

    [Fact]
    public void Read_User_given_non_existing_id_returns_null() => _repo.Read(42).Should().BeNull();

    [Fact]
    public void Read_User_given_existing_id_returns_user() => _repo.Read(1).Should().Be(new UserDTO(1,"Alice","al@ice.org"));
#endregion

    public void Dispose()
        {
            _context.Dispose();
        }    
}
