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
}