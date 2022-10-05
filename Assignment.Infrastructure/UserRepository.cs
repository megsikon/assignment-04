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
        var entity = _context.Users.FirstOrDefault(u => u.Name == user.Name);
        Response res;
        if(entity is null){
            entity = new User(user.Name,user.Email);
            _context.Users.Add(entity);
            _context.SaveChanges();

            res = Response.Created;
        }else{
            res = Response.Conflict;
        }

        var created = new UserDTO(entity.Id,entity.Name,entity.Email);
        return (res,created.Id);
    }
    
    public IReadOnlyCollection<UserDTO> ReadAll()
    {
       var user = from u in _context.Users
                    orderby u.Name
                    select new UserDTO(u.Id,u.Name,u.Email);

        return user.ToList();
    }
    
    public UserDTO Read(int userId)
    {
        var user = from u in _context.Users
                where u.Id == userId
                select new UserDTO(u.Id,u.Name,u.Email);
        return user.FirstOrDefault()!;
    }
    
    public Response Update(UserUpdateDTO user)
    {
        var entity = _context.Users.Find(user.Id);
        Response response;

        if(entity is null){
            response = Response.NotFound;
        }else if (_context.Users.FirstOrDefault(u => u.Id != user.Id && u.Email == user.Email) != null)
        {
            response = Response.Conflict;
        }else{
            entity.Name = user.Name;
            entity.Email = user.Email;
            _context.SaveChanges();
            response = Response.Updated;
        }
        return response;
    }
    
    public Response Delete(int userId, bool force = false)
    {
        var user = _context.Users.Include(u=>u.Items).FirstOrDefault(u => u.Id == userId); 
        Response response;
        if(user is null){
            response = Response.NotFound;
        } else if(user.Items.Any()){
            response = Response.Conflict;
        }else{
            if(force){
                _context.Users.Remove(user);
                _context.SaveChanges();
                response = Response.Deleted;
            }else{
                response = Response.Conflict;
            }
        }
        return response;
    }
}
