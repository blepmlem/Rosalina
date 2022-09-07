namespace Rosalina.TypeStubs;

public interface ILayout
{
	[GeneratorHelper(GeneratorTarget.RootVisualElementProperty)]
	public VisualElement Root { get; set; }

	[GeneratorHelper(GeneratorTarget.InitializeBindingsMethod)]
	public void BindInternals();
}