namespace Assignment.Infrastructure;

public class UserRepository
{
    private readonly KanbanContext _context;
    public UserRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        throw new NotImplementedException();
    }
    
    public IReadOnlyCollection<UserDTO> ReadAll()
    {
        throw new NotImplementedException();
    }
    
    public UserDTO Read(int userId)
    {
        throw new NotImplementedException();
    }
    
    public Response Update(UserUpdateDTO user)
    {
        throw new NotImplementedException();
    }
    
    public Response Delete(int userId, bool force = false)
    {
        throw new NotImplementedException();
    }
}
