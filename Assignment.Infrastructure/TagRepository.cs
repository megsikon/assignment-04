namespace Assignment.Infrastructure;

public class TagRepository
{
    private readonly KanbanContext _context;
    public TagRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Name == tag.Name);
        Response res;
        if(entity is null){
            entity = new Tag(tag.Name);
            _context.Tags.Add(entity);
            _context.SaveChanges();

            res = Response.Created;
        }else{
            res = Response.Conflict;
        }

        var created = new TagDTO(entity.Id,entity.Name);
        return (res,created.Id);
    }
    
    public IReadOnlyCollection<TagDTO> ReadAll()
    {
        var tags = from t in _context.Tags
                    orderby t.Name
                    select new TagDTO(t.Id,t.Name);

        return tags.ToArray();
    }
    
    public TagDTO Read(int tagId)
    {
        var tag = from t in _context.Tags
                where t.Id == tagId
                select new TagDTO(t.Id,t.Name);
        return tag.FirstOrDefault()!;
    }
    
    public Response Update(TagUpdateDTO tag)
    {
        var entity = _context.Tags.Find(tag.Id);
        Response response;

        if(entity is null){
            response = Response.NotFound;
        }else if (_context.Tags.FirstOrDefault(t => t.Id != tag.Id && t.Name == tag.Name) != null)
        {
            response = Response.Conflict;
        }else{
            entity.Name = tag.Name;
            _context.SaveChanges();
            response = Response.Updated;
        }
        return response;
    }

    public Response Delete(int tagId, bool force = false)
    {
        var tag = _context.Tags.Include(t=>t.WorkItems).FirstOrDefault(t=> t.Id==tagId); 
        Response response;
        if(tag is null){
            response = Response.NotFound;
        } else if(tag.WorkItems.Any()){
            response = Response.Conflict;
        }else{
            if(force){
                _context.Tags.Remove(tag);
                _context.SaveChanges();
                response = Response.Updated;
            }else{
                response = Response.Conflict;
            }
        }
        return response;
    }

}
