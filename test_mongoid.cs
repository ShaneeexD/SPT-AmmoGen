using SPTarkov.Server.Core.Models.Common;

class Program
{
    static void Main()
    {
        var id = new MongoId("5447ac644bdc2d6c208b4567");
        string parentId = "5447ac644bdc2d6c208b4567";
        Console.WriteLine("parentId == id: " + (parentId == id));
        Console.WriteLine("parentId == id.ToString(): " + (parentId == id.ToString()));
        Console.WriteLine("parentId.Equals(id): " + parentId.Equals(id));
    }
}
