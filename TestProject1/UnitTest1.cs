namespace TestProject1;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var result = NameCleaner.CleanName("The Matrix");
        Assert.That(result, Is.EqualTo("matrix"));
    }
    
    [Test]
    public void Test2()
    {
        var result = NameCleaner.CleanName("THE_RING");
        Assert.That(result, Is.EqualTo("ring"));
    }
    
    [Test]
    public void Test3()
    {
        var result = NameCleaner.CleanName("Marley & Me");
        Assert.That(result, Is.EqualTo("marleyme"));
    }
    
    [Test]
    public void Test4()
    {
        var result = NameCleaner.CleanName("MUMMA_MIA!");
        Assert.That(result, Is.EqualTo("mummamia"));
    }
    
    [Test]
    public void Test5()
    {
        var result = MovieBot.GetImdbId("MUMMA_MIA!");
        Assert.That(result, Is.EqualTo(null));
    }
    
    [Test]
    public void Test6()
    {
        var result = MovieBot.GetImdbId("Something Something [tt1234567]");
        Assert.That(result, Is.EqualTo("tt1234567"));
    }
}