namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests : IDisposable
{
    
    private readonly KanbanContext _context;
    private readonly TagRepository _repo;
    public TagRepositoryTests() {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        context.Tags.AddRange(new Tag("TagNameHere"){Id = 1,Name="TagNameHere",WorkItems = new List<WorkItem>()},new Tag("SecondTagNameHere"){Id = 2,Name="SecondTagNameHere",WorkItems = new List<WorkItem>()});
        // context.Tasks.AddRange(new Task{"someTitle",1,"someDescription",new List<String> {"one", "two"}}, new Task{"SecondTitle",2,"SecondDescription",new List<String> {"three", "four"}});

        context.SaveChanges();
        _context = context;
        _repo = new TagRepository(_context);

    }

    [Fact]
    public void Update_given_non_existing_Tag_returns_NotFound() => _repo.Update(new TagUpdateDTO(42,"TagName")).Should().Be(NotFound);
    [Fact]
    public void Delete_given_non_existing_TagId_returns_NotFound() => _repo.Delete(42).Should().Be(NotFound);


    //create
    [Fact]
    public void Create_Tag_returns_Created_with_tag(){
        var(response,created) = _repo.Create(new TagCreateDTO("Created Tag"));
        response.Should().Be(Created);
        created.Should().Be(new TagDTO(3,"Created Tag").Id);
    }
    [Fact]
    public void Create_tag_given_exisiting_tag_returns_Conflict(){
        var(response,tag) = _repo.Create(new TagCreateDTO("TagNameHere"));
        response.Should().Be(Conflict);
        var existingTag = new TagDTO(1,"TagNameHere");
        tag.Should().Be(new TagDTO(1,"TagNameHere").Id);    
    }

    [Fact]
    public void Delete_tag_without_force() {
        var response = _repo.Delete(1);
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void Delete_tag_with_force() {
        var response = _repo.Delete(1, true);
        response.Should().Be(Response.Updated);
    }

    //read
    [Fact]
    public void ReadAll_returns_all_tag() => _repo.ReadAll().Should().BeEquivalentTo(new[] {new TagDTO(1,"TagNameHere"), new TagDTO(2,"SecondTagNameHere")});

    [Fact]
    public void Read_Tag_given_non_existing_id_returns_null() => _repo.Read(42).Should().BeNull();

    [Fact]
    public void Read_Tag_given_existing_id_returns_tag() => _repo.Read(2).Should().Be(new TagDTO(2, "SecondTagNameHere"));

    
    //update
    [Fact]
    public void Update_Tag_returns_Updated_with_tag(){
        var response = _repo.Update(new TagUpdateDTO(2,"Updated Tag"));
        response.Should().Be(Updated);
        var entity = _context.Tags.Find(2)!;
        entity.Name.Should().Be("Updated Tag");
    }

    [Fact]
    public void Update_tag_given_exisiting_tag_returns_Conflict(){
        var response = _repo.Update(new TagUpdateDTO(1,"SecondTagNameHere"));
        response.Should().Be(Conflict);
        var entity = _context.Tags.Find(1)!;
        entity.Name.Should().Be("TagNameHere");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
