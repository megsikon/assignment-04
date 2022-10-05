namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly WorkItemRepository _repo;
    
    public WorkItemRepositoryTests() {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

        var User1 = new User("Alice", "al@ice.org") {Id = 1, Name = "Alice", Email = "al@ice.org"};
        var User2 = new User("Bent", "be@nt.org") {Id = 2, Name = "Bent", Email = "be@nt.org"};

        var Task1 = new WorkItem("Task1") {Id = 1, Title = "Task1", AssignedTo = User1, Description = "hej", Tags = new List<Tag> {}};
        var Task2 = new WorkItem("Task2") {Id = 2, Title = "Task2", AssignedTo = User2, Description = "Empty Description", State = State.Resolved};
        var Task38 = new WorkItem("SomeTitle") {Id = 38, Description = "Empty Description", State = State.New};
        var Task40 = new WorkItem("SomeTitle") {Id = 40, Description = "Empty Description", State = State.Active};
        var Task42 = new WorkItem("SomeTitle") {Id = 42, Description = "Empty Description", State = State.Resolved};
        

        _context!.Items.Add(Task1);
        _context.Items.Add(Task2);
        _context.Items.Add(Task38);
        _context.Items.Add(Task40);
        _context.Items.Add(Task42);

        _context.Users.Add(User1);
        _context.Users.Add(User2);

        _context = context;
        _repo = new WorkItemRepository(_context);
    }


#region read

    [Fact]
    public void Read_should_return_given_task_details() {
        _repo.Read(1).Should().BeEquivalentTo(new WorkItemDTO(1, "Task1", "Alice", new List<string>(), State.New)); 
    }  

    [Fact]
    public void ReadAll_should_return_all_tasks() { 
        _repo.ReadAll().Should().BeEquivalentTo(new[] {new WorkItemDTO(1, "Task1", "Alice", new List<string>{}, State.New), new WorkItemDTO(2, "Task2", "Bent", new List<string>{}, State.New)});
    }

    [Fact]
    public void ReadAllRemoved_should_return_all_removed_tasks() {
        _repo.ReadAllRemoved().Should().BeEquivalentTo(new[] {new WorkItemDTO(1, "Task1", "Alice", new List<string>{}, State.Removed)});
    }

    [Fact]
    public void ReadAllByTag_should_return_all_tasks_with_given_tag() {
        _repo.ReadAllByTag("Important").Should().BeEquivalentTo(new[] {new WorkItemDTO(1, "Task1", "Alice", new List<string>{"Important"}, State.New)});
    }

    [Fact]
    public void ReadAllByUser_should_return_all_tasks_with_given_user() {
        var expected= new WorkItemDTO(1, "Task1", "Herman", new List<string>{}, State.New);
        _repo.ReadAllByUser(1).Should().BeEquivalentTo(new[] {expected});
    }

    [Fact]
    public void ReadAllByState_should_returm_all_tasks_with_given_state() {
        _repo.ReadAllByState(State.Resolved).Should().BeEquivalentTo(new[] {new WorkItemDTO(1, "Task1", "Alice", new List<string>{}, State.Resolved)});
    }
#endregion
    
#region create

    [Fact]
    public void Return_Created_Response_When_Creating_A_CreateTaskDTO() {
        //arrange
        var exp = Response.Created;
        //act
        var res = _repo.Create(new WorkItemCreateDTO(
            "title",
            1,
            "description",
            new List<string> {"one", "two"}
        ));

        //assert
        res.Response.Should().Be(exp);
    }

    [Fact]
    public void Create_new_Task_sets_Status_New()
    {
        //Arrange
        var expState = State.New;

        //Act
        var res = _repo.Create(new WorkItemCreateDTO(
            "someTitle",
            1,
            "someDescription",
            new List<String> {"one", "two"}
        )); 

        //Assert
        var entity = _context.Items.Find(1);
        entity.State.Should().Be(expState); 
    }


    [Fact]
    public void Create_new_Task_sets_Created_to_current_time()
    {
        //Arrange
        var expTime = DateTime.UtcNow;

        //Act
        var res = _repo.Create(new WorkItemCreateDTO(
            "someTitle",
            1,
            "someDescription",
            new List<String> {"one", "two"}
        )); 

        //Assert
        var entity = _context.Items.Find(1)!;
        entity.Created.Should().BeCloseTo(expTime, precision: TimeSpan.FromSeconds(5));
    }
#endregion

#region delete

    [Fact]
    public void Delete_given_non_existing_TaskId_returns_NotFound() => _repo.Delete(42).Should().Be(NotFound);

    [Fact]
    public void Delete_given_existing_Task_with_Status_New_deletes_Task()
    {
        //Act
        var response = _repo.Delete(38);

        //Assert
        response.Should().Be(Deleted);
        var entity = _context.Items.Find(38);
        entity.Should().BeNull();

    }

    [Fact]
    public void Delete_given_existing_Task_with_Status_Active_should_change_Status_to_Removed()
    {
        //Arrange
        var exp = State.Removed; 
        
        //Act
        _repo.Delete(40);

        //Assert
        var entity = _context.Items.Find(40)!; 
        entity.State.Should().Be(exp); 
        _context.Items.Find(40).Should().NotBeNull();
    }

    [Fact]
    public void Delete_given_existing_Task_with_Status_Resolved_should_return_Conflict()
    {
        //Act
        var response = _repo.Delete(42);

        //Assert
        response.Should().Be(Conflict);
        _context.Items.Find(42).Should().NotBeNull();
    }
#endregion

#region Update
    [Fact]
    public void Update_State_of_existing_Task_sets_StateChanged_to_current_time()
    {
        //Arrange
        var expTime = DateTime.UtcNow;
        
        _repo.Update(new WorkItemUpdateDTO(
            2,
            "NewTitle",
            2,
            "someDescription",
            new List<String> {"one", "two"},
            State.Active
            ));

        //Assert
        var entity = _context.Items.Find(2)!;
        entity.StateUpdated.Should().BeCloseTo(expTime, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_given_non_existing_Task_returns_NotFound() => _repo.Update(new WorkItemUpdateDTO(42,"SomeTitle",42,"Empty Description",new List<string> {"one", "two"},State.Active)).Should().Be(NotFound);

#endregion
    
    public void Dispose()
    {
        _context.Dispose();
    }
}