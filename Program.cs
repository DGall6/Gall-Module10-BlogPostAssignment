using NLog;
using NLog.LayoutRenderers;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
    Console.WriteLine("Enter your selection:");
    Console.WriteLine("1) Display all blogs");
    Console.WriteLine("2) Add Blog");
    Console.WriteLine("3) Create Post");
    Console.WriteLine("4) Display Posts");
    Console.WriteLine("5) Delete Blog");
    Console.WriteLine("6) Edit Blog");
    Console.WriteLine("Enter to quit");
    string? choice = Console.ReadLine();
    Console.Clear();
    logger.Info("Option {choice} selected", choice);

    if (choice == "1")
    {
        // display blogs
        var db = new DataContext();
        var query = db.Blogs.OrderBy(b => b.Name);
        if (query.Any())
        {
            Console.WriteLine($"{query.Count()} Blogs returned");
            foreach (var item in query)
            {
                Console.WriteLine(item.Name);
            }
        }
        else
        {
            logger.Error("There are no blogs to display");
        }
    }
    else if (choice == "2")
    {
        // Add blog
        var db = new DataContext();
        Blog? blog = InputBlog(db, logger);
        if (blog != null)
        {
            //blog.BlogId = BlogId;
            db.AddBlog(blog);
            logger.Info("Blog added - {name}", blog.Name);
        }
    }
    else if (choice == "3")
    {
        // Create Post
        Console.WriteLine("Choose the Blog to post to:");
        var db = new DataContext();
        var blog = GetBlog(db);
        if (blog != null)
        {
            // Add post
            Post? post = InputPost(db, blog, logger);
            if(post != null)
            {
                db.AddPost(post);
                blog.Posts.Add(post);
                logger.Info($"Post '{post.Title}' added to Blog '{blog.Name}'");
            }
        }
        else
        {
            logger.Error("Blog not found");
        }
    }
    else if (choice == "4")
    {
        // Display Posts
        Console.WriteLine("Choose the Blog to display posts of:");
        var db = new DataContext();
        var blog = GetBlog(db);
        if (blog != null)
        {
            // Display posts
            var query = db.Posts.Where(p => p.Blog == blog);
            if (query.Any())
            {
                Console.WriteLine($"\nThe Blog '{blog.Name}' has {query.Count()} post(s)");
                foreach (var p in query)
                {
                    Console.WriteLine($"\n{blog.Name}\n{p.Title}\n{p.Content}");
                }
            }
            else
            {
                logger.Error("Blog has no posts");
            }
        }
        else
        {
            logger.Error("Blog not found");
        }
    }
    else if (choice == "5")
    {
        // delete blog
        Console.WriteLine("Choose the blog to delete:");
        var db = new DataContext();
        var blog = GetBlog(db);
        if (blog != null)
        {
            // delete blog
            db.DeleteBlog(blog);
            logger.Info($"Blog (id: {blog.BlogId}) deleted");
        }
        else
        {
            logger.Error("Blog is null");
        }
    }
    else if (choice == "6")
    {
        // edit blog
        Console.WriteLine("Choose the blog to edit:");
        var db = new DataContext();
        var blog = GetBlog(db);
        if (blog != null)
        {
            // input blog
            Blog? UpdatedBlog = InputBlog(db, logger);
            if (UpdatedBlog != null)
            {
                UpdatedBlog.BlogId = blog.BlogId;
                db.EditBlog(UpdatedBlog);
                logger.Info($"Blog (id: {blog.BlogId}) updated");
            }
        }
    }
    else if (String.IsNullOrEmpty(choice))
    {
        break;
    }
    else
    {
        logger.Error("Invalid Input");
    }
    Console.WriteLine();
} while (true);

logger.Info("Program ended");

static Blog? GetBlog(DataContext db)
{
    // display all blogs
    var blogs = db.Blogs.OrderBy(b => b.BlogId);
    foreach (Blog b in blogs)
    {
        Console.WriteLine($"{b.BlogId}: {b.Name}");
    }
    if (int.TryParse(Console.ReadLine(), out int BlogId))
    {
        Blog blog = db.Blogs.FirstOrDefault(b => b.BlogId == BlogId)!;
        return blog;
    }
    return null;
}

static Blog? InputBlog(DataContext db, NLog.Logger logger)
{
    Blog blog = new();
    Console.WriteLine("Enter the Blog name");
    blog.Name = Console.ReadLine();

    ValidationContext context = new(blog, null, null);
    List<ValidationResult> results = [];

    var isValid = Validator.TryValidateObject(blog, context, results, true);
    if (isValid)
    {
        // check for unique name
        if (db.Blogs.Any(b => b.Name == blog.Name))
        {
            // generate validation error
            isValid = false;
            results.Add(new ValidationResult("Blog name exists", ["Name"]));
        }
        else
        {
            logger.Info("Validation passed");
        }
    }
    if (!isValid)
    {
        foreach (var result in results)
        {
            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
        return null;
    }
    return blog;
}

static Post? InputPost(DataContext db, Blog blog, NLog.Logger logger){
    // A bit weird since it asks for both title and 
    Post post = new();
    Console.WriteLine("Enter the Post title:");
    post.Title = Console.ReadLine();

    ValidationContext context = new(post, null, null);
    List<ValidationResult> results = [];

    var isValid = Validator.TryValidateObject(post, context, results, true);
    if (isValid)
    {
        // check for unique name
        if (db.Posts.Any(p => p.Title == post.Title))
        {
            // generate validation error
            isValid = false;
            results.Add(new ValidationResult("Post title exists", ["Title"]));
        }
        else
        {
            logger.Info("Validation passed");
        }
    }
    if (!isValid)
    {
        foreach (var result in results)
        {
            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
        return null;
    }
    Console.WriteLine("Enter the Post Content:");
    post.Content = Console.ReadLine();
    post.BlogId = blog.BlogId;
    post.Blog = blog;
    return post;
}