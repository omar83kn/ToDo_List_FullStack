using Microsoft.EntityFrameworkCore;
using ToDo_List.Models;

namespace ToDo_List.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ListItem> ListItems => Set<ListItem>();

    // ✅ NEW: files table
    public DbSet<ListItemFile> ListItemFiles => Set<ListItemFile>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Person -> TodoLists
        b.Entity<Person>()
            .HasMany(p => p.TodoLists)
            .WithOne(t => t.Person)
            .HasForeignKey(t => t.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        // TodoList -> Items
        b.Entity<TodoList>()
            .HasMany(t => t.Items)
            .WithOne(i => i.TodoList)
            .HasForeignKey(i => i.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> Items (SetNull)
        b.Entity<Category>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        b.Entity<ListItem>()
            .HasIndex(i => new { i.TodoListId, i.SortOrder });

        // ✅ NEW: ListItem -> Files (1..many)
        b.Entity<ListItemFile>()
            .HasOne(f => f.ListItem)
            .WithMany(i => i.Files)
            .HasForeignKey(f => f.ListItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // optional but nice
        b.Entity<ListItemFile>()
            .HasIndex(f => f.ListItemId);
    }
}
