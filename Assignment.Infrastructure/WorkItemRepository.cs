namespace Assignment.Infrastructure;

public class WorkItemRepository
{
   private readonly KanbanContext _context;
    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int WorkId) Create(WorkItemCreateDTO workItem)
    {
        var user = _context.Users.Find(workItem.AssignedToId);
        if (user == null)
            {
                return (Response.BadRequest, 0);
            }
        
        var itemTagsList = workItem.Tags.ToList();
        var tagList = new List<Tag>();
        foreach (var t in itemTagsList)
        {
            tagList.Add(new Tag(t));
        }

        var entity = _context.Items.FirstOrDefault(t => t.Title == workItem.Title);
        Response response;
        if (entity is null) {
            entity = new WorkItem (workItem.Title) {
                        Title = workItem.Title,
                        AssignedTo = user,
                        Description = workItem.Description,
                        Tags = tagList,
                        Created = System.DateTime.UtcNow,
                        StateUpdated = System.DateTime.UtcNow
                    };
            _context.Items.Add(entity);
            _context.SaveChanges();
            response = Response.Created;
        } else {
            response = Response.Conflict;
        }

        return (response, entity.Id);
    }
    public IReadOnlyCollection<WorkItemDTO> ReadAll()
    {
        var workItem = from t in _context.Items
                  select new WorkItemDTO(
                      t.Id,
                      t.Title,
                      t.AssignedTo!.Name,
                      t.Tags.Select(t => t.Name).ToList(),
                      t.State);
        return workItem.ToList();
    
    }
    public IReadOnlyCollection<WorkItemDTO> ReadAllRemoved()
    {
        var workItem = from t in _context.Items
                  where t.State == State.Removed
                  select new WorkItemDTO(
                      t.Id,
                      t.Title,
                      t.AssignedTo!.Name,
                      t.Tags.Select(t => t.Name).ToList(),
                      t.State);
        return workItem.ToList();
    }
    
    public IReadOnlyCollection<WorkItemDTO> ReadAllByTag(string tag)
    {
        var workItem = from t in _context.Items
                  where t.Tags.Select(s => s.Name).Contains(tag)
                  select new WorkItemDTO(
                      t.Id,
                      t.Title,
                      t.AssignedTo!.Name,
                      t.Tags.Select(t => t.Name).ToList(),
                      t.State);
        return workItem.ToList();
    }
    
    public IReadOnlyCollection<WorkItemDTO> ReadAllByUser(int userId)
    {
        var workItem = from t in _context.Items
                  where t.Id == userId
                  select new WorkItemDTO(
                      t.Id,
                      t.Title,
                      t.AssignedTo!.Name,
                      t.Tags.Select(t => t.Name).ToList(),
                      t.State);
        return workItem.ToList();
    }
    
    public IReadOnlyCollection<WorkItemDTO> ReadAllByState(State state)
    {
       var workItem = from t in _context.Items
                  where t.State == state
                  select new WorkItemDTO(
                      t.Id,
                      t.Title,
                      t.AssignedTo!.Name,
                      t.Tags.Select(t => t.Name).ToList(),
                      t.State);
        return workItem.ToList();
    }
    
    public WorkItemDetailsDTO Read(int workItemId)
    {
        // throw new NotImplementedException();
        var workItem = from t in _context.Items
                    where t.Id == workItemId
                    select new WorkItemDetailsDTO(
                        t.Id,
                        t.Title,
                        t.Description!,
                        t.Created,
                        t.AssignedTo!.Name,
                        t.Tags.Select(t => t.Name).ToList(),
                        t.State,
                        t.StateUpdated);
        return workItem.FirstOrDefault()!;
    }
    
    public Response Update(WorkItemUpdateDTO workItem)
    {
        var entity = _context.Items.Find(workItem.Id);
        Response response;

        if(entity is null){
            response = Response.NotFound;
        }else if (_context.Items.FirstOrDefault(t => t.Id != workItem.Id && t.Title == workItem.Title) != null)
        {
            response = Response.Conflict;
        }else{
            entity.Title = workItem.Title;
            entity.StateUpdated = System.DateTime.UtcNow; 
            
            _context.SaveChanges();
            response = Response.Updated;
        }
        return response;
    }
        
    public Response Delete(int workItemId)
    {
        var entity = _context.Items.Find(workItemId);
        Response response;

        if(entity is null){
            response = Response.NotFound;
        }else if(entity.State == State.New ){
            _context.Items.Remove(entity);
            _context.SaveChanges();
            response = Response.Deleted;
        }else if(entity.State == State.Active){
            entity.State = State.Removed;
            _context.SaveChanges();
            response = Response.Updated; 
        }else{
            response = Response.Conflict;
        }
        return response;
    }
}
