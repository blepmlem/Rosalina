using System;
using System.Reflection;

namespace Rosalina.TypeStubs;

public class TargetTypeInfo
{
	public string ClassName { get; }
	public string RootVisualElement { get; }
	public string InitializeBindingsMethod { get; }

	public TargetTypeInfo(Type type)
	{
		ClassName = type.Name;
		foreach (MemberInfo mInfo in type.GetMembers())
		{
			if (mInfo.GetCustomAttribute(typeof(GeneratorHelperAttribute)) is GeneratorHelperAttribute attribute)
			{
				switch (attribute.GeneratorTarget)
				{
					case GeneratorTarget.RootVisualElementProperty:
						RootVisualElement = mInfo.Name;
						break;
					case GeneratorTarget.InitializeBindingsMethod:
						InitializeBindingsMethod = mInfo.Name;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		if (RootVisualElement == null || InitializeBindingsMethod == null)
		{
			throw new ArgumentException($"{type.Name} does not have the required {nameof(GeneratorHelperAttribute)} attributes");
		}
	}
}