using UnityEngine.Assertions;
using UnitySpec;

[Binding]
public class Binding
{
    readonly ScenarioContext context;
    public Binding(ScenarioContext context)
    {
        this.context = context;
    }

    [Given(@"(a|b)=(\d)")]
    public void GivenVariableEqualsValue(string variable, byte value)
    {
        context.Add(variable, value);
    }

    [When("I add a and b")]
    public void WhenIAddAAndB()
    {
        context.Add("sum", (byte)context["a"] + (byte)context["b"]);
    }

    [Then(@"the sum should be (\d)")]
    public void ThenTheSumShouldBe(int expected)
    {
        Assert.AreEqual(expected, context["sum"]);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        context.Clear();
    }
}
